using System.Collections.ObjectModel;
using System.Windows.Input;
using FinanceTracker.DAL.Entities;
using FinanceTracker.MAUI.Services;

namespace FinanceTracker.MAUI.ViewModels;

[QueryProperty(nameof(TransactionId), "id")]
public class TransactionFormViewModel : BaseViewModel
{
    private readonly IDataService _dataService;

    public ObservableCollection<Category> Categories { get; } = new();

    private Category? _selectedCategory;
    public Category? SelectedCategory
    {
        get => _selectedCategory;
        set { _selectedCategory = value; OnPropertyChanged(); }
    }

    private int _transactionId;
    public int TransactionId
    {
        get => _transactionId;
        set
        {
            _transactionId = value;
            OnPropertyChanged();
            if (value > 0)
                LoadTransactionAsync(value).ConfigureAwait(false);
        }
    }

    private string _description = string.Empty;
    public string Description
    {
        get => _description;
        set { _description = value; OnPropertyChanged(); }
    }

    private decimal _amount;
    public decimal Amount
    {
        get => _amount;
        set { _amount = value; OnPropertyChanged(); }
    }

    private DateTime _date = DateTime.Today;
    public DateTime Date
    {
        get => _date;
        set { _date = value; OnPropertyChanged(); }
    }

    private bool _isIncome;
    public bool IsIncome
    {
        get => _isIncome;
        set { _isIncome = value; OnPropertyChanged(); }
    }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public TransactionFormViewModel(IDataService dataService)
    {
        _dataService = dataService;
        Title = "Нова транзакція";
        LoadCategoriesAsync().ConfigureAwait(false);

        SaveCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(Description))
            {
                await Shell.Current.DisplayAlert("Помилка", "Введіть опис транзакції", "OK");
                return;
            }
            if (Amount <= 0)
            {
                await Shell.Current.DisplayAlert("Помилка", "Введіть суму більше нуля", "OK");
                return;
            }
            if (SelectedCategory is null)
            {
                await Shell.Current.DisplayAlert("Помилка", "Оберіть категорію", "OK");
                return;
            }

            try
            {
                IsBusy = true;
                if (TransactionId == 0)
                {
                    var newTransaction = new Transaction
                    {
                        Description = Description,
                        Amount = Amount,
                        Date = Date,
                        IsIncome = IsIncome,
                        CategoryId = SelectedCategory.Id
                    };
                    await _dataService.CreateTransactionAsync(newTransaction);
                }
                else
                {
                    var existing = await _dataService.GetTransactionByIdAsync(TransactionId);
                    if (existing is not null)
                    {
                        existing.Description = Description;
                        existing.Amount = Amount;
                        existing.Date = Date;
                        existing.IsIncome = IsIncome;
                        existing.CategoryId = SelectedCategory.Id;
                        await _dataService.UpdateTransactionAsync(existing);
                    }
                }

                await Shell.Current.DisplayAlert("Успіх", "Збережено!", "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception)
            {
                await Shell.Current.DisplayAlert("Помилка", "Не вдалося зберегти", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        });

        CancelCommand = new Command(async () =>
            await Shell.Current.GoToAsync(".."));
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            var categories = await _dataService.GetAllCategoriesAsync();
            Categories.Clear();
            foreach (var c in categories)
                Categories.Add(c);

            if (!Categories.Any())
            {
                await Shell.Current.DisplayAlert(
                    "Увага",
                    "Спочатку створіть категорію у вкладці Категорії",
                    "OK");
                await Shell.Current.GoToAsync("..");
            }
        }
        catch (Exception) { }
    }

    private async Task LoadTransactionAsync(int id)
    {
        try
        {
            IsBusy = true;
            Title = "Редагування";
            var transaction = await _dataService.GetTransactionByIdAsync(id);
            if (transaction is not null)
            {
                Description = transaction.Description;
                Amount = transaction.Amount;
                Date = transaction.Date;
                IsIncome = transaction.IsIncome;
                SelectedCategory = Categories.FirstOrDefault(c => c.Id == transaction.CategoryId);
            }
        }
        catch (Exception) { }
        finally
        {
            IsBusy = false;
        }
    }
}