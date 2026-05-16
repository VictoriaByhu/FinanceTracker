using System.Windows.Input;
using FinanceTracker.DAL.Entities;
using FinanceTracker.MAUI.Services;

namespace FinanceTracker.MAUI.ViewModels;

[QueryProperty(nameof(TransactionId), "id")]
public class TransactionDetailViewModel : BaseViewModel
{
    private readonly IDataService _dataService;

    private int _transactionId;
    public int TransactionId
    {
        get => _transactionId;
        set
        {
            _transactionId = value;
            OnPropertyChanged();
            LoadTransactionAsync(value).ConfigureAwait(false);
        }
    }

    private Transaction? _transaction;
    public Transaction? Transaction
    {
        get => _transaction;
        set
        {
            _transaction = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(AmountFormatted));
            OnPropertyChanged(nameof(TypeLabel));
        }
    }

    public string AmountFormatted
    {
        get
        {
            if (Transaction is null) return "?0";
            var sign = Transaction.IsIncome ? "+" : "?";
            return $"{sign}?{Transaction.Amount:N0}";
        }
    }

    public string TypeLabel => Transaction?.IsIncome == true ? "Income" : "Expense";

    private bool _isDeleteDialogVisible;
    public bool IsDeleteDialogVisible
    {
        get => _isDeleteDialogVisible;
        set { _isDeleteDialogVisible = value; OnPropertyChanged(); }
    }

    public ICommand DeleteCommand { get; }
    public ICommand ConfirmDeleteCommand { get; }
    public ICommand CancelDeleteCommand { get; }
    public ICommand GoToEditCommand { get; }
    public ICommand GoBackCommand { get; }

    public TransactionDetailViewModel(IDataService dataService)
    {
        _dataService = dataService;
        Title = "Transaction Details";

        DeleteCommand = new Command(() => IsDeleteDialogVisible = true);
        CancelDeleteCommand = new Command(() => IsDeleteDialogVisible = false);

        ConfirmDeleteCommand = new Command(async () =>
        {
            try
            {
                IsDeleteDialogVisible = false;
                await _dataService.DeleteTransactionAsync(TransactionId);
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception)
            {
                ErrorMessage = "Failed to delete transaction";
            }
        });

        GoToEditCommand = new Command(async () =>
            await Shell.Current.GoToAsync($"transactionform?id={TransactionId}"));

        GoBackCommand = new Command(async () =>
            await Shell.Current.GoToAsync(".."));
    }

    private async Task LoadTransactionAsync(int id)
    {
        try
        {
            IsBusy = true;
            Transaction = await _dataService.GetTransactionByIdAsync(id);
            if (Transaction is not null)
                Title = Transaction.Category?.Name ?? Transaction.Description ?? "Transaction Details";
        }
        catch (Exception)
        {
            ErrorMessage = "Failed to load transaction data";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
