using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Together.Application.Interfaces;
using Together.Application.Services;
using Together.Domain.Interfaces;
using Together.Infrastructure.Data;
using Together.Infrastructure.Repositories;
using Together.Infrastructure.Services;

namespace Together.Presentation
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
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
            // Configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            services.AddSingleton<IConfiguration>(configuration);

            // Database Context
            services.AddDbContext<TogetherDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("SupabaseConnection")));

            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IFollowRelationshipRepository, FollowRelationshipRepository>();
            services.AddScoped<IPostRepository, PostRepository>();
            services.AddScoped<IJournalEntryRepository, JournalEntryRepository>();
            services.AddScoped<IMoodEntryRepository, MoodEntryRepository>();
            services.AddScoped<ICoupleConnectionRepository, CoupleConnectionRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<ISharedEventRepository, SharedEventRepository>();

            // Application Services
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<IFollowService, FollowService>();
            services.AddScoped<IPostService, PostService>();
            services.AddScoped<ISocialFeedService, SocialFeedService>();
            services.AddScoped<IJournalService, JournalService>();
            services.AddScoped<IMoodTrackingService, MoodTrackingService>();
            services.AddScoped<IMoodAnalysisService, MoodAnalysisService>();
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<ILoveStreakService, LoveStreakService>();

            // Infrastructure Services
            services.AddScoped<IStorageService, SupabaseStorageService>();
            
            // Caching
            services.AddMemoryCache();

            // ViewModels will be registered here as they are created
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
