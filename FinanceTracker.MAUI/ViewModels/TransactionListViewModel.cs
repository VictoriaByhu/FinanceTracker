using System.Collections.ObjectModel;
using System.Windows.Input;
using FinanceTracker.DAL.Entities;
using FinanceTracker.MAUI.Services;

namespace FinanceTracker.MAUI.ViewModels;

public class TransactionListViewModel : BaseViewModel
{
    private readonly IDataService _dataService;

    public ObservableCollection<Transaction> Transactions { get; } = new();

    public bool IsEmpty => !Transactions.Any();

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

    public TransactionListViewModel(IDataService dataService)
    {
        _dataService = dataService;
        Title = "Finance Tracker";

        LoadTransactionsCommand = new Command(async () => await LoadTransactionsAsync());
        GoToAddCommand = new Command(async () =>
            await Shell.Current.GoToAsync("transactionform"));
        GoToDetailCommand = new Command<Transaction>(async (transaction) =>
        {
            if (transaction is null) return;
            await Shell.Current.GoToAsync($"transactiondetail?id={transaction.Id}");
        });
    }

    private async Task LoadTransactionsAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            var transactions = await _dataService.GetAllTransactionsAsync();
            Transactions.Clear();
            foreach (var t in transactions)
                Transactions.Add(t);

            TotalIncome = Transactions.Where(t => t.IsIncome).Sum(t => t.Amount);
            TotalExpenses = Transactions.Where(t => !t.IsIncome).Sum(t => t.Amount);
            TotalBalance = TotalIncome - TotalExpenses;

            OnPropertyChanged(nameof(IsEmpty));
        }
        catch (Exception)
        {
            await Shell.Current.DisplayAlert("Error", "Failed to load data", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}