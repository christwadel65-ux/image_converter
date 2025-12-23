
using System.Windows;
using System.Windows.Threading;

namespace ImageConvertResize.WPF
{
    public partial class App : System.Windows.Application 
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur au démarrage:\n\n{ex.GetType().Name}\n{ex.Message}\n\n{ex.StackTrace}", 
                    "Erreur de démarrage", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Shutdown(1);
            }
        }

        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            System.Windows.MessageBox.Show($"Exception non gérée:\n\n{e.Exception.GetType().Name}\n{e.Exception.Message}\n\n{e.Exception.StackTrace}", 
                "Exception non gérée", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = false;
        }
    }
}
