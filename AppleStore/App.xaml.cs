using System.Windows;

namespace AppleStore
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SimpleHttpServer.Start();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            SimpleHttpServer.Stop();
            base.OnExit(e);
        }
    }
}