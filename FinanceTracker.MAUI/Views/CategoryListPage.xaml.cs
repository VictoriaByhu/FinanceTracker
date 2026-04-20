using FinanceTracker.MAUI.ViewModels;

namespace FinanceTracker.MAUI.Views;

public partial class CategoryListPage : ContentPage
{
    public CategoryListPage(CategoryListViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ((CategoryListViewModel)BindingContext).LoadCategoriesCommand.Execute(null);
    }
}