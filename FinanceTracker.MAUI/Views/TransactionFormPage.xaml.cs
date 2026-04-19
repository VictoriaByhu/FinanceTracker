using FinanceTracker.MAUI.ViewModels;

namespace FinanceTracker.MAUI.Views;

public partial class TransactionFormPage : ContentPage
{
    public TransactionFormPage(TransactionFormViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}