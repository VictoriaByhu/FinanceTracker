using FinanceTracker.DAL;
using FinanceTracker.DAL.Repositories;
using FinanceTracker.MAUI.Services;
using FinanceTracker.MAUI.ViewModels;
using FinanceTracker.MAUI.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinanceTracker.MAUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Database
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "finance.db");
        builder.Services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlite($"Data Source={dbPath}"));

        // Generic Repository
        builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        builder.Services.AddScoped<IDataService, LocalDataService>();
        // ViewModels
        builder.Services.AddTransient<TransactionListViewModel>();
        builder.Services.AddTransient<TransactionDetailViewModel>();
        builder.Services.AddTransient<TransactionFormViewModel>();

        // Views
        builder.Services.AddTransient<TransactionListPage>();
        builder.Services.AddTransient<TransactionDetailPage>();
        builder.Services.AddTransient<TransactionFormPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}