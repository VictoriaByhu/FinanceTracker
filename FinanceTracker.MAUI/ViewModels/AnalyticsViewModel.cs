using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FinanceTracker.DAL.Entities;
using FinanceTracker.MAUI.Services;

namespace FinanceTracker.MAUI.ViewModels;

public class AnalyticsViewModel : BaseViewModel
{
    private readonly IDataService _dataService;
    private readonly CultureInfo _culture = CultureInfo.GetCultureInfo("uk-UA");

    public ObservableCollection<CategoryExpenseItem> CategoryExpenses { get; } = new();
    public ObservableCollection<CategoryBudgetItem> CategoryBudgets { get; } = new();

    private decimal _monthlyIncome;
    public decimal MonthlyIncome
    {
        get => _monthlyIncome;
        set { _monthlyIncome = value; OnPropertyChanged(); OnPropertyChanged(nameof(MonthlyIncomeText)); }
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

    private string _topExpenseCategory = "Немає витрат";
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

    public string MonthTitle => DateTime.Today.ToString("MMMM yyyy", _culture);
    public string MonthlyIncomeText => FormatCurrency(MonthlyIncome);
    public string MonthlyExpensesText => FormatCurrency(MonthlyExpenses);
    public string MonthlyBalanceText => FormatCurrency(MonthlyBalance);
    public string TransactionCountText => $"{TransactionCount} транз.";
    public bool HasExpenses => CategoryExpenses.Any();
    public bool HasNoExpenses => !HasExpenses;
    public ICommand LoadAnalyticsCommand { get; }

    public AnalyticsViewModel(IDataService dataService)
    {
        _dataService = dataService;
        Title = "Аналітика";
        LoadAnalyticsCommand = new Command(async () => await LoadAnalyticsAsync());
    }

    public async Task LoadAnalyticsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var nextMonth = monthStart.AddMonths(1);

            var transactions = (await _dataService.GetAllTransactionsAsync())
                .Where(t => t.Date >= monthStart && t.Date < nextMonth)
                .ToList();

            var categories = (await _dataService.GetAllCategoriesAsync()).ToList();
            var expenses = transactions.Where(t => !t.IsIncome).ToList();

            MonthlyIncome = transactions.Where(t => t.IsIncome).Sum(t => t.Amount);
            MonthlyExpenses = expenses.Sum(t => t.Amount);
            MonthlyBalance = MonthlyIncome - MonthlyExpenses;
            TransactionCount = transactions.Count;

            BuildCategoryExpenses(expenses);
            BuildBudgets(categories, expenses);
        }
        catch (Exception)
        {
            await Shell.Current.DisplayAlert("Помилка", "Не вдалося завантажити аналітику", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void BuildCategoryExpenses(IEnumerable<Transaction> expenses)
    {
        var grouped = expenses
            .GroupBy(t => new
            {
                t.CategoryId,
                CategoryName = t.Category?.Name ?? "Без категорії",
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

        TopExpenseCategory = grouped.FirstOrDefault()?.CategoryName ?? "Немає витрат";
        OnPropertyChanged(nameof(HasExpenses));
        OnPropertyChanged(nameof(HasNoExpenses));
    }

    private void BuildBudgets(IEnumerable<Category> categories, IEnumerable<Transaction> expenses)
    {
        var spentByCategory = expenses
            .GroupBy(t => t.CategoryId)
            .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

        CategoryBudgets.Clear();

        foreach (var category in categories.OrderBy(c => c.Name))
        {
            spentByCategory.TryGetValue(category.Id, out var spent);
            CategoryBudgets.Add(new CategoryBudgetItem(category, spent));
        }
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
    private readonly int _categoryId;
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
            Preferences.Set(GetPreferenceKey(_categoryId), (double)_budget);
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
    public string SpentText => $"Витрачено ₴{Spent:N0}";
    public string RemainingText => HasBudget
        ? IsOverBudget ? $"Перевищено на ₴{Math.Abs(Remaining):N0}" : $"Залишилось ₴{Remaining:N0}"
        : "Встановіть бюджет";
    public string ProgressText => HasBudget ? $"{Spent / Budget:P0}" : "0%";
    public Color ProgressColor => IsOverBudget
        ? Microsoft.Maui.Graphics.Color.FromArgb("#E24B4A")
        : Microsoft.Maui.Graphics.Color.FromArgb("#1D9E75");
    public Color StatusColor => IsOverBudget
        ? Microsoft.Maui.Graphics.Color.FromArgb("#E24B4A")
        : Microsoft.Maui.Graphics.Color.FromArgb("#9CA3AF");

    public CategoryBudgetItem(Category category, decimal spent)
    {
        _categoryId = category.Id;
        Name = category.Name;
        Color = category.Color;
        Spent = spent;
        _budget = (decimal)Preferences.Get(GetPreferenceKey(_categoryId), 0d);
    }

    private static string GetPreferenceKey(int categoryId) => $"category_budget_{categoryId}";

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
