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
        }
    }

    public ICommand DeleteCommand { get; }
    public ICommand GoToEditCommand { get; }

    public TransactionDetailViewModel(IDataService dataService)
    {
        _dataService = dataService;
        Title = "Деталі транзакції";

        DeleteCommand = new Command(async () =>
        {
            bool confirmed = await Shell.Current.DisplayAlert(
                "Увага", "Видалити транзакцію?", "Так", "Ні");
            if (!confirmed) return;

            try
            {
                await _dataService.DeleteTransactionAsync(TransactionId);
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception)
            {
                await Shell.Current.DisplayAlert("Помилка", "Не вдалося видалити", "OK");
            }
        });

        GoToEditCommand = new Command(async () =>
            await Shell.Current.GoToAsync($"transactionform?id={TransactionId}"));
    }

    private async Task LoadTransactionAsync(int id)
    {
        try
        {
            IsBusy = true;
            Transaction = await _dataService.GetTransactionByIdAsync(id);
            if (Transaction is not null)
                Title = Transaction.Category?.Name ?? Transaction.Description ?? "Деталі";
        }
        catch (Exception)
        {
            await Shell.Current.DisplayAlert("Помилка", "Не вдалося завантажити дані", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}