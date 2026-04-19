using FinanceTracker.MAUI.ViewModels;

namespace FinanceTracker.MAUI.Views;

public partial class TransactionDetailPage : ContentPage
{
    public TransactionDetailPage(TransactionDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}