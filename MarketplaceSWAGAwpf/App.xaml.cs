using System.Windows;

namespace MarketplaceSWAGAwpf
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            await Lists.LoadAllAsync();
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}