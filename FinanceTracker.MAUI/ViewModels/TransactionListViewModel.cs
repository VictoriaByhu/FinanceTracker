using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using FinanceTracker.DAL.Entities;
using FinanceTracker.MAUI.Services;

namespace FinanceTracker.MAUI.ViewModels;

public class TransactionListViewModel : BaseViewModel
{
    private readonly IDataService _dataService;
    private readonly CultureInfo _culture = CultureInfo.GetCultureInfo("en-US");
    private readonly List<Transaction> _allTransactions = new();

    public ObservableCollection<Transaction> Transactions { get; } = new();

    public bool IsEmpty => !Transactions.Any();
    public bool IsAllSelected => SelectedFilter == TransactionFilter.All;
    public bool IsIncomeSelected => SelectedFilter == TransactionFilter.Income;
    public bool IsExpenseSelected => SelectedFilter == TransactionFilter.Expense;
    public string MonthTitle => SelectedMonth.ToString("MMMM yyyy", _culture);
    public bool IsCurrentMonth => SelectedMonth.Year == DateTime.Today.Year && SelectedMonth.Month == DateTime.Today.Month;
    public bool CanGoNextMonth => SelectedMonth < CurrentMonthStart;
    private static DateTime CurrentMonthStart => new(DateTime.Today.Year, DateTime.Today.Month, 1);

    private DateTime _selectedMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    public DateTime SelectedMonth
    {
        get => _selectedMonth;
        set
        {
            var normalizedMonth = new DateTime(value.Year, value.Month, 1);
            _selectedMonth = normalizedMonth > CurrentMonthStart ? CurrentMonthStart : normalizedMonth;
            ApplyFilter();
            OnPropertyChanged();
            OnPropertyChanged(nameof(MonthTitle));
            OnPropertyChanged(nameof(IsCurrentMonth));
            OnPropertyChanged(nameof(CanGoNextMonth));
        }
    }

    private TransactionFilter _selectedFilter = TransactionFilter.All;
    public TransactionFilter SelectedFilter
    {
        get => _selectedFilter;
        set
        {
            if (_selectedFilter == value) return;
            _selectedFilter = value;
            ApplyFilter();
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsAllSelected));
            OnPropertyChanged(nameof(IsIncomeSelected));
            OnPropertyChanged(nameof(IsExpenseSelected));
        }
    }

    private Transaction? _selectedTransaction;
    public Transaction? SelectedTransaction
    {
        get => _selectedTransaction;
        set { _selectedTransaction = value; OnPropertyChanged(); }
    }

    private decimal _totalBalance;
    public decimal TotalBalance
    {
        get => _totalBalance;
        set { _totalBalance = value; OnPropertyChanged(); }
    }

    private decimal _totalIncome;
    public decimal TotalIncome
    {
        get => _totalIncome;
        set { _totalIncome = value; OnPropertyChanged(); }
    }

    private decimal _totalExpenses;
    public decimal TotalExpenses
    {
        get => _totalExpenses;
        set { _totalExpenses = value; OnPropertyChanged(); }
    }

    public ICommand LoadTransactionsCommand { get; }
    public ICommand GoToAddCommand { get; }
    public ICommand GoToDetailCommand { get; }
    public ICommand ShowAllCommand { get; }
    public ICommand ShowIncomeCommand { get; }
    public ICommand ShowExpensesCommand { get; }
    public ICommand PreviousMonthCommand { get; }
    public ICommand NextMonthCommand { get; }
    public ICommand CurrentMonthCommand { get; }

    public TransactionListViewModel(IDataService dataService)
    {
        _dataService = dataService;
        Title = "Transactions";

        LoadTransactionsCommand = new Command(async () => await LoadTransactionsAsync());
        GoToAddCommand = new Command(async () =>
            await Shell.Current.GoToAsync("transactionform"));
        GoToDetailCommand = new Command<Transaction>(async (transaction) =>
        {
            if (transaction is null) return;
            await Shell.Current.GoToAsync($"transactiondetail?id={transaction.Id}");
        });
        ShowAllCommand = new Command(() => SelectedFilter = TransactionFilter.All);
        ShowIncomeCommand = new Command(() => SelectedFilter = TransactionFilter.Income);
        ShowExpensesCommand = new Command(() => SelectedFilter = TransactionFilter.Expense);
        PreviousMonthCommand = new Command(() => SelectedMonth = SelectedMonth.AddMonths(-1));
        NextMonthCommand = new Command(() =>
        {
            if (CanGoNextMonth)
                SelectedMonth = SelectedMonth.AddMonths(1);
        });
        CurrentMonthCommand = new Command(() => SelectedMonth = DateTime.Today);
    }

    private async Task LoadTransactionsAsync()
    {
        if (IsBusy) return;
        try
        {
            ErrorMessage = string.Empty;
            IsBusy = true;
            var transactions = await _dataService.GetAllTransactionsAsync();
            _allTransactions.Clear();
            _allTransactions.AddRange(transactions.OrderByDescending(t => t.Date));

            ApplyFilter();
        }
        catch (Exception)
        {
            ErrorMessage = "Failed to load data";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyFilter()
    {
        var monthStart = SelectedMonth;
        var nextMonth = monthStart.AddMonths(1);
        var monthTransactions = _allTransactions
            .Where(t => t.Date >= monthStart && t.Date < nextMonth)
            .ToList();

        TotalIncome = monthTransactions.Where(t => t.IsIncome).Sum(t => t.Amount);
        TotalExpenses = monthTransactions.Where(t => !t.IsIncome).Sum(t => t.Amount);
        TotalBalance = TotalIncome - TotalExpenses;

        var filtered = SelectedFilter switch
        {
            TransactionFilter.Income => monthTransactions.Where(t => t.IsIncome),
            TransactionFilter.Expense => monthTransactions.Where(t => !t.IsIncome),
            _ => monthTransactions
        };

        Transactions.Clear();
        foreach (var transaction in filtered)
            Transactions.Add(transaction);

        OnPropertyChanged(nameof(IsEmpty));
    }
}

public enum TransactionFilter
{
    All,
    Income,
    Expense
}
