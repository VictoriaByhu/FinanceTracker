using FinanceTracker.MAUI.ViewModels;

namespace FinanceTracker.MAUI.Views;

public partial class TransactionListPage : ContentPage
{
    public TransactionListPage(TransactionListViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ((TransactionListViewModel)BindingContext).LoadTransactionsCommand.Execute(null);
    }
}