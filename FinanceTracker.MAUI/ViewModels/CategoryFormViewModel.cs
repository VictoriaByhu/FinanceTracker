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

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public CategoryFormViewModel(IDataService dataService)
    {
        _dataService = dataService;
        Title = "Нова категорія";

        SaveCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                await Shell.Current.DisplayAlert("Помилка", "Введіть назву категорії", "OK");
                return;
            }
            try
            {
                IsBusy = true;
                var category = new Category
                {
                    Name = Name,
                    Color = "#607D8B",
                    IsIncome = false
                };
                await _dataService.CreateCategoryAsync(category);
                await Shell.Current.DisplayAlert("Успіх", "Категорію збережено!", "OK");
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
}