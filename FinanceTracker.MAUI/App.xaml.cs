using FinanceTracker.DAL;

namespace FinanceTracker.MAUI;

public partial class App : Application
{
    public App(AppDbContext dbContext)
    {
        InitializeComponent();
        dbContext.Database.EnsureCreated();
        MainPage = new AppShell();
    }
}