using FinanceTracker.DAL;

namespace FinanceTracker.MAUI;

public partial class App : Application
{
    public App(AppDbContext dbContext)
    {
        InitializeComponent();
        UserAppTheme = AppTheme.Dark;
        MainPage = new AppShell();
    }
}
