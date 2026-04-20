using FinanceTracker.MAUI.Views;

namespace FinanceTracker.MAUI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("transactiondetail", typeof(TransactionDetailPage));
        Routing.RegisterRoute("transactionform", typeof(TransactionFormPage));
        Routing.RegisterRoute("categoryform", typeof(CategoryFormPage));
    }
}