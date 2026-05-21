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
            RootWindow rootWindow = new RootWindow();
            rootWindow.Show();

            rootWindow.MainFrame.Navigate(new LoginPage());
        }
    }
}
