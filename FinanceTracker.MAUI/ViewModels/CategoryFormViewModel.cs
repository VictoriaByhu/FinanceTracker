using System.Windows.Input;
using FinanceTracker.DAL.Entities;
using FinanceTracker.MAUI.Services;

namespace FinanceTracker.MAUI.ViewModels;

[QueryProperty(nameof(CategoryId), "id")]
public class CategoryFormViewModel : BaseViewModel
{
    private readonly IDataService _dataService;

    private int _categoryId;
    public int CategoryId
    {
        get => _categoryId;
        set
        {
            _categoryId = value;
            OnPropertyChanged();
        }
    }

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    private string _selectedColor = "#607D8B";
    public string SelectedColor
    {
        get => _selectedColor;
        set { _selectedColor = value; OnPropertyChanged(); }
    }

    private bool _isIncome;
    public bool IsIncome
    {
        get => _isIncome;
        set
        {
            if (_isIncome == value) return;
            _isIncome = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsExpense));
            OnPropertyChanged(nameof(TypePreview));
        }
    }

    public bool IsExpense
    {
        get => !IsIncome;
        set
        {
            if (value)
                IsIncome = false;
        }
    }

    public string TypePreview => IsIncome ? "Income" : "Expense";

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand SelectColorCommand { get; }

    public CategoryFormViewModel(IDataService dataService)
    {
        _dataService = dataService;
        Title = "New category";

        SaveCommand = new Command(async () =>
        {
            ErrorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(Name))
            {
                ErrorMessage = "Enter a category name";
                return;
            }
            try
            {
                IsBusy = true;
                if (CategoryId == 0)
                {
                    var category = new Category
                    {
                        Name = Name,
                        Color = SelectedColor,
                        IsIncome = IsIncome
                    };
                    await _dataService.CreateCategoryAsync(category);
                }
                else
                {
                    var category = await _dataService.GetCategoryByIdAsync(CategoryId);
                    if (category is not null)
                    {
                        category.Name = Name;
                        category.Color = SelectedColor;
                        category.IsIncome = IsIncome;
                        await _dataService.UpdateCategoryAsync(category);
                    }
                }
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception)
            {
                ErrorMessage = "Failed to save category";
            }
            finally
            {
                IsBusy = false;
            }
        });

        CancelCommand = new Command(async () =>
            await Shell.Current.GoToAsync(".."));

        SelectColorCommand = new Command<string>(color =>
        {
            if (!string.IsNullOrWhiteSpace(color))
                SelectedColor = color;
        });
    }

    public async Task InitializeAsync()
    {
        if (CategoryId <= 0)
            return;

        try
        {
            IsBusy = true;
            Title = "Edit category";
            var category = await _dataService.GetCategoryByIdAsync(CategoryId);
            if (category is null)
                return;

            Name = category.Name;
            SelectedColor = category.Color;
            IsIncome = category.IsIncome;
        }
        catch (Exception)
        {
            ErrorMessage = "Failed to load category";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
