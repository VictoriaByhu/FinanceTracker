using FinanceTracker.MAUI.ViewModels;

namespace FinanceTracker.MAUI.Views;

public partial class CategoryFormPage : ContentPage
{
    public CategoryFormPage(CategoryFormViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = ((CategoryFormViewModel)BindingContext).InitializeAsync();
    }
}
