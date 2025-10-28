using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace Together.Presentation
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Register services here
            // Domain layer services will be registered here
            // Application layer services will be registered here
            // Infrastructure layer services will be registered here
            // Presentation layer ViewModels will be registered here
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // MainWindow will be resolved from DI container once ViewModels are set up
            // var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            // mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}
