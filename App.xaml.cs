using System.Configuration;
using System.Data;
using System.Windows;

namespace rms_gui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Show login window as a dialog
            LoginWindow loginWindow = new LoginWindow();

            if (loginWindow.ShowDialog() == true)
            {
                // Login successful, show main window
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            }
            else
            {
                // Login cancelled or failed, exit application
                Shutdown();
            }
        }
    }
}
