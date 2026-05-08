using System.Collections.ObjectModel;
using System.Windows.Input;
using FinanceTracker.DAL.Entities;
using FinanceTracker.MAUI.Services;

namespace FinanceTracker.MAUI.ViewModels;

public class CategoryListViewModel : BaseViewModel
{
    private readonly IDataService _dataService;

    public ObservableCollection<Category> Categories { get; } = new();
    public bool IsEmpty => !Categories.Any();

    public ICommand LoadCategoriesCommand { get; }
    public ICommand GoToAddCommand { get; }
    public ICommand DeleteCommand { get; }

    public CategoryListViewModel(IDataService dataService)
    {
        _dataService = dataService;
        Title = "Category";

        LoadCategoriesCommand = new Command(async () => await LoadCategoriesAsync());
        GoToAddCommand = new Command(async () =>
            await Shell.Current.GoToAsync("categoryform"));
        DeleteCommand = new Command<Category>(async (category) =>
        {
            if (category is null) return;
            bool confirmed = await Shell.Current.DisplayAlert(
                "Warning", $"Delete category '{category.Name}'?", "Yes", "No");
            if (!confirmed) return;
            try
            {
                await _dataService.DeleteCategoryAsync(category.Id);
                await LoadCategoriesAsync();
            }
            catch (Exception)
            {
                await Shell.Current.DisplayAlert("Error", "Failed to delete category", "OK");
            }
        });
    }

    private async Task LoadCategoriesAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            var categories = await _dataService.GetAllCategoriesAsync();
            Categories.Clear();
            foreach (var c in categories)
                Categories.Add(c);

            OnPropertyChanged(nameof(IsEmpty));
        }
        catch (Exception)
        {
            await Shell.Current.DisplayAlert("Error", "Failed to load categories", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
