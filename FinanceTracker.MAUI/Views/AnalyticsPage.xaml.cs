using FinanceTracker.MAUI.ViewModels;

namespace FinanceTracker.MAUI.Views;

public partial class AnalyticsPage : ContentPage
{
    public AnalyticsPage(AnalyticsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = ((AnalyticsViewModel)BindingContext).LoadAnalyticsAsync();
    }
}
