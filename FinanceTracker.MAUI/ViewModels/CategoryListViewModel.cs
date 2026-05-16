using System.Collections.ObjectModel;
using System.Windows.Input;
using FinanceTracker.DAL.Entities;
using FinanceTracker.MAUI.Services;

namespace FinanceTracker.MAUI.ViewModels;

public class CategoryListViewModel : BaseViewModel
{
    private readonly IDataService _dataService;

    public ObservableCollection<Category> Categories { get; } = new();
    public ObservableCollection<Category> ExpenseCategories { get; } = new();
    public ObservableCollection<Category> IncomeCategories { get; } = new();
    public bool IsEmpty => !Categories.Any();
    public bool HasExpenseCategories => ExpenseCategories.Any();
    public bool HasIncomeCategories => IncomeCategories.Any();

    private Category? _categoryPendingDelete;
    public Category? CategoryPendingDelete
    {
        get => _categoryPendingDelete;
        set
        {
            _categoryPendingDelete = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsDeleteDialogVisible));
            OnPropertyChanged(nameof(DeleteDialogMessage));
        }
    }

    public bool IsDeleteDialogVisible => CategoryPendingDelete is not null;
    public string DeleteDialogMessage => CategoryPendingDelete is null
        ? string.Empty
        : $"Delete category \"{CategoryPendingDelete.Name}\"?";

    public ICommand LoadCategoriesCommand { get; }
    public ICommand GoToAddCommand { get; }
    public ICommand GoToEditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand ConfirmDeleteCommand { get; }
    public ICommand CancelDeleteCommand { get; }

    public CategoryListViewModel(IDataService dataService)
    {
        _dataService = dataService;
        Title = "Categories";

        LoadCategoriesCommand = new Command(async () => await LoadCategoriesAsync());
        GoToAddCommand = new Command(async () =>
            await Shell.Current.GoToAsync("categoryform"));
        GoToEditCommand = new Command<Category>(async (category) =>
        {
            if (category is null) return;
            await Shell.Current.GoToAsync($"categoryform?id={category.Id}");
        });
        DeleteCommand = new Command<Category>(async (category) =>
        {
            if (category is null) return;
            CategoryPendingDelete = category;
            await Task.CompletedTask;
        });
        ConfirmDeleteCommand = new Command(async () =>
        {
            if (CategoryPendingDelete is null) return;
            try
            {
                ErrorMessage = string.Empty;
                var category = CategoryPendingDelete;
                CategoryPendingDelete = null;
                await _dataService.DeleteCategoryAsync(category.Id);
                await LoadCategoriesAsync();
            }
            catch (Exception)
            {
                ErrorMessage = "Failed to delete category";
            }
        });
        CancelDeleteCommand = new Command(() => CategoryPendingDelete = null);
    }

    private async Task LoadCategoriesAsync()
    {
        if (IsBusy) return;
        try
        {
            ErrorMessage = string.Empty;
            IsBusy = true;
            var categories = await _dataService.GetAllCategoriesAsync();
            Categories.Clear();
            ExpenseCategories.Clear();
            IncomeCategories.Clear();
            foreach (var c in categories)
            {
                Categories.Add(c);
                if (c.IsIncome)
                    IncomeCategories.Add(c);
                else
                    ExpenseCategories.Add(c);
            }

            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(nameof(HasExpenseCategories));
            OnPropertyChanged(nameof(HasIncomeCategories));
        }
        catch (Exception)
        {
            ErrorMessage = "Failed to load categories";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
