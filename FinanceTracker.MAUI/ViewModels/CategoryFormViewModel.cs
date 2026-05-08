using System.Windows.Input;
using FinanceTracker.DAL.Entities;
using FinanceTracker.MAUI.Services;

namespace FinanceTracker.MAUI.ViewModels;

public class CategoryFormViewModel : BaseViewModel
{
    private readonly IDataService _dataService;

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

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand SelectColorCommand { get; }

    public CategoryFormViewModel(IDataService dataService)
    {
        _dataService = dataService;
        Title = "New Category";

        SaveCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                await Shell.Current.DisplayAlert("Error", "Please enter a category name", "OK");
                return;
            }
            try
            {
                IsBusy = true;
                var category = new Category
                {
                    Name = Name,
                    Color = SelectedColor,
                    IsIncome = false
                };
                await _dataService.CreateCategoryAsync(category);
                await Shell.Current.DisplayAlert("Success", "Category saved!", "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception)
            {
                await Shell.Current.DisplayAlert("Error", "Failed to save category", "OK");
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
}
