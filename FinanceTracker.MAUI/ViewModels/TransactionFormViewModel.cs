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
        set
        {
            _selectedCategory = value;
            if (_selectedCategory is not null)
                IsIncome = _selectedCategory.IsIncome;
            OnPropertyChanged();
        }
    }

    private int _transactionId;
    public int TransactionId
    {
        get => _transactionId;
        set
        {
            _transactionId = value;
            OnPropertyChanged();
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
        set
        {
            _isIncome = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TypeLabel));
        }
    }

    public string TypeLabel => IsIncome ? "Income" : "Expense";

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public TransactionFormViewModel(IDataService dataService)
    {
        _dataService = dataService;
        Title = "New transaction";

        SaveCommand = new Command(async () =>
        {
            ErrorMessage = string.Empty;
            if (Amount <= 0)
            {
                ErrorMessage = "Enter an amount greater than zero";
                return;
            }
            if (SelectedCategory is null)
            {
                ErrorMessage = "Choose a category";
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
                        IsIncome = SelectedCategory.IsIncome,
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
                        existing.IsIncome = SelectedCategory.IsIncome;
                        existing.CategoryId = SelectedCategory.Id;
                        await _dataService.UpdateTransactionAsync(existing);
                    }
                }

                await Shell.Current.GoToAsync("..");
            }
            catch (Exception)
            {
                ErrorMessage = "Failed to save transaction";
            }
            finally
            {
                IsBusy = false;
            }
        });

        CancelCommand = new Command(async () =>
            await Shell.Current.GoToAsync(".."));
    }

    // This method is called when the page appears
    public async Task InitializeAsync()
    {
        await LoadCategoriesAsync();
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
                ErrorMessage = "Create a category in the Categories tab first";
                await Shell.Current.GoToAsync("..");
                return;
            }

            // This is called only after loading categories
            if (TransactionId > 0)
                await LoadTransactionAsync(TransactionId);
        }
        catch (Exception) { }
    }

    private async Task LoadTransactionAsync(int id)
    {
        try
        {
            IsBusy = true;
            Title = "Edit transaction";
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
