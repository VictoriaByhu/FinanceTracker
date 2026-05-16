using Android.App;
using Android.Content.PM;
using Android.Graphics.Drawables;
using Android.OS;
using AndroidX.Core.View;

namespace FinanceTracker.MAUI
{
    [Activity(Theme = "@style/AppTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Window is null)
            {
                return;
            }

            WindowCompat.SetDecorFitsSystemWindows(Window, true);

            var systemBarsColor = Android.Graphics.Color.ParseColor("#0F2F28");
            Window.SetBackgroundDrawable(new ColorDrawable(systemBarsColor));
            Window.DecorView?.SetBackgroundColor(systemBarsColor);
            Window.SetStatusBarColor(systemBarsColor);
            Window.SetNavigationBarColor(systemBarsColor);

            var controller = WindowCompat.GetInsetsController(Window, Window.DecorView);
            if (controller is not null)
            {
                controller.AppearanceLightStatusBars = false;
                controller.AppearanceLightNavigationBars = false;
            }
        }
    }
}
