using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Input;
using FinanceTracker.DAL.Entities;
using FinanceTracker.MAUI.Services;
using Microsoft.Maui.Graphics;

namespace FinanceTracker.MAUI.ViewModels;

public class AnalyticsViewModel : BaseViewModel
{
    private readonly IDataService _dataService;
    private readonly CultureInfo _culture = CultureInfo.GetCultureInfo("en-US");
    private int _loadVersion;

    public ObservableCollection<CategoryExpenseItem> CategoryExpenses { get; } = new();
    public ObservableCollection<CategoryBudgetItem> CategoryBudgets { get; } = new();

    private DateTime _selectedMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    public DateTime SelectedMonth
    {
        get => _selectedMonth;
        set
        {
            var normalizedMonth = new DateTime(value.Year, value.Month, 1);
            _selectedMonth = normalizedMonth > CurrentMonthStart ? CurrentMonthStart : normalizedMonth;
            OnPropertyChanged();
            OnPropertyChanged(nameof(MonthTitle));
            OnPropertyChanged(nameof(IsCurrentMonth));
            OnPropertyChanged(nameof(CanGoNextMonth));
            OnPropertyChanged(nameof(IsHistoryMonth));
            _ = LoadAnalyticsAsync();
        }
    }

    private decimal _monthlyIncome;
    public decimal MonthlyIncome
    {
        get => _monthlyIncome;
        set
        {
            _monthlyIncome = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(MonthlyIncomeText));
            NotifyIncomePlanProperties();
        }
    }

    private decimal _monthlyExpenses;
    public decimal MonthlyExpenses
    {
        get => _monthlyExpenses;
        set { _monthlyExpenses = value; OnPropertyChanged(); OnPropertyChanged(nameof(MonthlyExpensesText)); }
    }

    private decimal _monthlyBalance;
    public decimal MonthlyBalance
    {
        get => _monthlyBalance;
        set { _monthlyBalance = value; OnPropertyChanged(); OnPropertyChanged(nameof(MonthlyBalanceText)); }
    }

    private string _topExpenseCategory = "No expenses";
    public string TopExpenseCategory
    {
        get => _topExpenseCategory;
        set { _topExpenseCategory = value; OnPropertyChanged(); }
    }

    private int _transactionCount;
    public int TransactionCount
    {
        get => _transactionCount;
        set { _transactionCount = value; OnPropertyChanged(); OnPropertyChanged(nameof(TransactionCountText)); }
    }

    private decimal _plannedIncome;
    public decimal PlannedIncome
    {
        get => _plannedIncome;
        set
        {
            if (_plannedIncome == value) return;
            _plannedIncome = value;
            Preferences.Set(GetMonthPreferenceKey("planned_income"), (double)_plannedIncome);
            NotifyIncomePlanProperties();
        }
    }

    private decimal _spendingPercent;
    public decimal SpendingPercent
    {
        get => _spendingPercent;
        set
        {
            var clamped = ClampPercent(value);
            if (_spendingPercent == clamped) return;
            _spendingPercent = clamped;
            Preferences.Set(GetMonthPreferenceKey("spending_percent"), (double)_spendingPercent);
            NotifyIncomePlanProperties();
        }
    }

    private decimal _savingsPercent;
    public decimal SavingsPercent
    {
        get => _savingsPercent;
        set
        {
            var clamped = ClampPercent(value);
            if (_savingsPercent == clamped) return;
            _savingsPercent = clamped;
            Preferences.Set(GetMonthPreferenceKey("savings_percent"), (double)_savingsPercent);
            NotifyIncomePlanProperties();
        }
    }

    private decimal _investmentPercent;
    public decimal InvestmentPercent
    {
        get => _investmentPercent;
        set
        {
            var clamped = ClampPercent(value);
            if (_investmentPercent == clamped) return;
            _investmentPercent = clamped;
            Preferences.Set(GetMonthPreferenceKey("investment_percent"), (double)_investmentPercent);
            NotifyIncomePlanProperties();
        }
    }

    public string MonthTitle => SelectedMonth.ToString("MMMM yyyy", _culture);
    public string MonthlyIncomeText => FormatCurrency(MonthlyIncome);
    public string MonthlyExpensesText => FormatCurrency(MonthlyExpenses);
    public string MonthlyBalanceText => FormatCurrency(MonthlyBalance);
    public string TransactionCountText => $"{TransactionCount}";
    public bool HasExpenses => CategoryExpenses.Any();
    public bool HasNoExpenses => !HasExpenses;
    public bool IsCurrentMonth => SelectedMonth.Year == DateTime.Today.Year && SelectedMonth.Month == DateTime.Today.Month;
    public bool IsHistoryMonth => SelectedMonth < CurrentMonthStart;
    public bool CanGoNextMonth => SelectedMonth < CurrentMonthStart;
    private static DateTime CurrentMonthStart => new(DateTime.Today.Year, DateTime.Today.Month, 1);

    public string PlannedIncomeText
    {
        get => PlannedIncome <= 0 ? string.Empty : PlannedIncome.ToString("0.##", CultureInfo.InvariantCulture);
        set
        {
            var normalized = value?.Replace(',', '.') ?? string.Empty;
            if (decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed) && parsed >= 0)
                PlannedIncome = parsed;
            else if (string.IsNullOrWhiteSpace(value))
                PlannedIncome = 0;
        }
    }

    public string SpendingPercentText
    {
        get => SpendingPercent.ToString("0.##", CultureInfo.InvariantCulture);
        set => SetPercentText(value, v => SpendingPercent = v);
    }

    public string SavingsPercentText
    {
        get => SavingsPercent.ToString("0.##", CultureInfo.InvariantCulture);
        set => SetPercentText(value, v => SavingsPercent = v);
    }

    public string InvestmentPercentText
    {
        get => InvestmentPercent.ToString("0.##", CultureInfo.InvariantCulture);
        set => SetPercentText(value, v => InvestmentPercent = v);
    }

    public decimal PlannedSpendAmount => PlannedIncome * SpendingPercent / 100;
    public decimal PlannedSavingsAmount => PlannedIncome * SavingsPercent / 100;
    public decimal PlannedInvestmentAmount => PlannedIncome * InvestmentPercent / 100;
    public decimal ActualSpendLimit => MonthlyIncome > 0 ? MonthlyIncome * SpendingPercent / 100 : PlannedSpendAmount;
    public decimal IncomeDifference => MonthlyIncome - PlannedIncome;
    public bool IsIncomeBelowPlan => PlannedIncome > 0 && MonthlyIncome > 0 && MonthlyIncome < PlannedIncome;
    public string PlannedSpendText => FormatCurrency(PlannedSpendAmount);
    public string PlannedSavingsText => FormatCurrency(PlannedSavingsAmount);
    public string PlannedInvestmentText => FormatCurrency(PlannedInvestmentAmount);
    public string ActualSpendLimitText => FormatCurrency(ActualSpendLimit);
    public string IncomePlanStatusText => PlannedIncome <= 0
        ? "Enter expected income to plan monthly limits."
        : MonthlyIncome <= 0
            ? "No actual income yet. The plan is based on expected income for now."
            : IsIncomeBelowPlan
                ? $"Actual income is below plan by {FormatCurrency(Math.Abs(IncomeDifference))}. Consider reducing the spending limit to {ActualSpendLimitText}."
                : $"Actual income is {FormatCurrency(MonthlyIncome)}. Spending limit based on your share: {ActualSpendLimitText}.";

    public ICommand LoadAnalyticsCommand { get; }
    public ICommand PreviousMonthCommand { get; }
    public ICommand NextMonthCommand { get; }
    public ICommand CurrentMonthCommand { get; }

    public AnalyticsViewModel(IDataService dataService)
    {
        _dataService = dataService;
        Title = "Analytics";
        LoadAnalyticsCommand = new Command(async () => await LoadAnalyticsAsync());
        PreviousMonthCommand = new Command(() => SelectedMonth = SelectedMonth.AddMonths(-1));
        NextMonthCommand = new Command(() =>
        {
            if (CanGoNextMonth)
                SelectedMonth = SelectedMonth.AddMonths(1);
        });
        CurrentMonthCommand = new Command(() => SelectedMonth = DateTime.Today);
        LoadIncomePlan();
    }

    public async Task LoadAnalyticsAsync()
    {
        var version = Interlocked.Increment(ref _loadVersion);
        var monthStart = SelectedMonth;
        var nextMonth = monthStart.AddMonths(1);

        try
        {
            ErrorMessage = string.Empty;
            IsBusy = true;
            LoadIncomePlan(monthStart);

            var transactions = (await _dataService.GetAllTransactionsAsync())
                .Where(t => t.Date >= monthStart && t.Date < nextMonth)
                .ToList();

            var categories = (await _dataService.GetAllCategoriesAsync()).ToList();
            var expenses = transactions.Where(t => !t.IsIncome).ToList();

            if (version != _loadVersion)
                return;

            MonthlyIncome = transactions.Where(t => t.IsIncome).Sum(t => t.Amount);
            MonthlyExpenses = expenses.Sum(t => t.Amount);
            MonthlyBalance = MonthlyIncome - MonthlyExpenses;
            TransactionCount = transactions.Count;

            BuildCategoryExpenses(expenses);
            BuildBudgets(categories, expenses);
        }
        catch (Exception)
        {
            ErrorMessage = "Failed to load analytics";
        }
        finally
        {
            if (version == _loadVersion)
                IsBusy = false;
        }
    }

    private void BuildCategoryExpenses(IEnumerable<Transaction> expenses)
    {
        var grouped = expenses
            .GroupBy(t => new
            {
                t.CategoryId,
                CategoryName = t.Category?.Name ?? "Uncategorized",
                CategoryColor = t.Category?.Color ?? "#607D8B"
            })
            .Select(g => new
            {
                g.Key.CategoryName,
                g.Key.CategoryColor,
                Amount = g.Sum(t => t.Amount)
            })
            .OrderByDescending(x => x.Amount)
            .ToList();

        CategoryExpenses.Clear();

        foreach (var item in grouped)
        {
            CategoryExpenses.Add(new CategoryExpenseItem(
                item.CategoryName,
                item.CategoryColor,
                item.Amount,
                MonthlyExpenses));
        }

        TopExpenseCategory = grouped.FirstOrDefault()?.CategoryName ?? "No expenses";
        OnPropertyChanged(nameof(HasExpenses));
        OnPropertyChanged(nameof(HasNoExpenses));
    }

    private void BuildBudgets(IEnumerable<Category> categories, IEnumerable<Transaction> expenses)
    {
        var spentByCategory = expenses
            .GroupBy(t => t.CategoryId)
            .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

        CategoryBudgets.Clear();

        foreach (var category in categories.Where(c => !c.IsIncome).OrderBy(c => c.Name))
        {
            spentByCategory.TryGetValue(category.Id, out var spent);
            CategoryBudgets.Add(new CategoryBudgetItem(category, spent, SelectedMonth));
        }
    }

    private void LoadIncomePlan()
    {
        LoadIncomePlan(SelectedMonth);
    }

    private void LoadIncomePlan(DateTime month)
    {
        _plannedIncome = (decimal)Preferences.Get(GetMonthPreferenceKey("planned_income", month), 0d);
        _spendingPercent = (decimal)Preferences.Get(GetMonthPreferenceKey("spending_percent", month), 70d);
        _savingsPercent = (decimal)Preferences.Get(GetMonthPreferenceKey("savings_percent", month), 20d);
        _investmentPercent = (decimal)Preferences.Get(GetMonthPreferenceKey("investment_percent", month), 10d);
        NotifyIncomePlanProperties();
    }

    private string GetMonthPreferenceKey(string key) => $"{key}_{SelectedMonth:yyyy_MM}";
    private static string GetMonthPreferenceKey(string key, DateTime month) => $"{key}_{month:yyyy_MM}";

    private static decimal ClampPercent(decimal value) => Math.Max(0, Math.Min(100, value));

    private static void SetPercentText(string? value, Action<decimal> setter)
    {
        var normalized = value?.Replace(',', '.') ?? string.Empty;
        if (decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
            setter(parsed);
        else if (string.IsNullOrWhiteSpace(value))
            setter(0);
    }

    private void NotifyIncomePlanProperties()
    {
        OnPropertyChanged(nameof(PlannedIncome));
        OnPropertyChanged(nameof(PlannedIncomeText));
        OnPropertyChanged(nameof(SpendingPercent));
        OnPropertyChanged(nameof(SpendingPercentText));
        OnPropertyChanged(nameof(SavingsPercent));
        OnPropertyChanged(nameof(SavingsPercentText));
        OnPropertyChanged(nameof(InvestmentPercent));
        OnPropertyChanged(nameof(InvestmentPercentText));
        OnPropertyChanged(nameof(PlannedSpendAmount));
        OnPropertyChanged(nameof(PlannedSavingsAmount));
        OnPropertyChanged(nameof(PlannedInvestmentAmount));
        OnPropertyChanged(nameof(ActualSpendLimit));
        OnPropertyChanged(nameof(IncomeDifference));
        OnPropertyChanged(nameof(IsIncomeBelowPlan));
        OnPropertyChanged(nameof(PlannedSpendText));
        OnPropertyChanged(nameof(PlannedSavingsText));
        OnPropertyChanged(nameof(PlannedInvestmentText));
        OnPropertyChanged(nameof(ActualSpendLimitText));
        OnPropertyChanged(nameof(IncomePlanStatusText));
    }

    private string FormatCurrency(decimal value) => $"₴{value:N0}";
}

public class CategoryExpenseItem
{
    public string Name { get; }
    public string Color { get; }
    public decimal Amount { get; }
    public double Progress { get; }
    public string AmountText => $"₴{Amount:N0}";
    public string PercentText => $"{Progress:P0}";

    public CategoryExpenseItem(string name, string color, decimal amount, decimal totalExpenses)
    {
        Name = name;
        Color = color;
        Amount = amount;
        Progress = totalExpenses <= 0 ? 0 : (double)(amount / totalExpenses);
    }
}

public class CategoryBudgetItem : INotifyPropertyChanged
{
    private readonly string _preferenceKey;
    private decimal _budget;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Name { get; }
    public string Color { get; }
    public decimal Spent { get; }

    public decimal Budget
    {
        get => _budget;
        set
        {
            if (_budget == value) return;
            _budget = value;
            Preferences.Set(_preferenceKey, (double)_budget);
            NotifyBudgetProperties();
        }
    }

    public string BudgetText
    {
        get => Budget <= 0 ? string.Empty : Budget.ToString("0.##", CultureInfo.InvariantCulture);
        set
        {
            var normalized = value?.Replace(',', '.') ?? string.Empty;
            if (decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed) && parsed >= 0)
                Budget = parsed;
            else if (string.IsNullOrWhiteSpace(value))
                Budget = 0;
        }
    }

    public decimal Remaining => Budget - Spent;
    public double Progress => Budget <= 0 ? 0 : Math.Min((double)(Spent / Budget), 1);
    public bool HasBudget => Budget > 0;
    public bool IsOverBudget => HasBudget && Spent > Budget;
    public string SpentText => $"Spent ₴{Spent:N0}";
    public string RemainingText => HasBudget
        ? IsOverBudget ? $"Over by ₴{Math.Abs(Remaining):N0}" : $"Left ₴{Remaining:N0}"
        : "Set a budget";
    public string ProgressText => HasBudget ? $"{Spent / Budget:P0}" : "0%";
    public Color ProgressColor => IsOverBudget
        ? Microsoft.Maui.Graphics.Color.FromArgb("#E24B4A")
        : Microsoft.Maui.Graphics.Color.FromArgb("#1D9E75");
    public Color StatusColor => IsOverBudget
        ? Microsoft.Maui.Graphics.Color.FromArgb("#E24B4A")
        : Microsoft.Maui.Graphics.Color.FromArgb("#9CA3AF");

    public CategoryBudgetItem(Category category, decimal spent, DateTime month)
    {
        _preferenceKey = GetPreferenceKey(category.Id, month);
        Name = category.Name;
        Color = category.Color;
        Spent = spent;
        _budget = (decimal)Preferences.Get(_preferenceKey, 0d);
    }

    private static string GetPreferenceKey(int categoryId, DateTime month) => $"category_budget_{categoryId}_{month:yyyy_MM}";

    private void NotifyBudgetProperties()
    {
        OnPropertyChanged(nameof(Budget));
        OnPropertyChanged(nameof(BudgetText));
        OnPropertyChanged(nameof(Remaining));
        OnPropertyChanged(nameof(Progress));
        OnPropertyChanged(nameof(HasBudget));
        OnPropertyChanged(nameof(IsOverBudget));
        OnPropertyChanged(nameof(RemainingText));
        OnPropertyChanged(nameof(ProgressText));
        OnPropertyChanged(nameof(ProgressColor));
        OnPropertyChanged(nameof(StatusColor));
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
