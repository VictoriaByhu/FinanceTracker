using FinanceTracker.MAUI.ViewModels;

namespace FinanceTracker.MAUI.Views;

public partial class CategoryFormPage : ContentPage
{
    public CategoryFormPage(CategoryFormViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}