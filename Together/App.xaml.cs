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
using Together.Infrastructure.SignalR;
using Microsoft.Extensions.Logging;
using Together.Presentation.ViewModels;

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
            services.AddScoped<IVirtualPetRepository, VirtualPetRepository>();
            services.AddScoped<ITodoItemRepository, TodoItemRepository>();
            services.AddScoped<IChallengeRepository, ChallengeRepository>();
            services.AddScoped<IConnectionRequestRepository, ConnectionRequestRepository>();
            services.AddScoped<ILikeRepository, LikeRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();

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
            services.AddScoped<IVirtualPetService, VirtualPetService>();
            services.AddScoped<ILongDistanceService, LongDistanceService>();
            services.AddScoped<ITodoService, TodoService>();
            services.AddScoped<IChallengeService, ChallengeService>();
            services.AddScoped<ICoupleConnectionService, CoupleConnectionService>();
            services.AddScoped<ILikeService, LikeService>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<IDashboardService, DashboardService>();

            // Infrastructure Services
            services.AddScoped<IStorageService, SupabaseStorageService>();
            services.AddSingleton<IRealTimeSyncService, TogetherHub>();
            
            // Navigation Service
            services.AddSingleton<Together.Services.INavigationService, Together.Services.NavigationService>();
            
            // Logging
            services.AddLogging();
            
            // Caching
            services.AddMemoryCache();

            // ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<RegisterViewModel>();
            services.AddTransient<CoupleHubViewModel>();
            services.AddTransient<JournalViewModel>();
            services.AddTransient<MoodTrackerViewModel>();
            services.AddTransient<SocialFeedViewModel>();
            services.AddTransient<UserProfileViewModel>();
            services.AddTransient<CalendarViewModel>();
            services.AddTransient<TodoListViewModel>();
            services.AddTransient<ChallengeViewModel>();
            services.AddTransient<VirtualPetViewModel>();
            services.AddTransient<LongDistanceViewModel>();
            
            // Windows
            services.AddTransient<MainWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Show login window initially
            var loginWindow = new Window
            {
                Title = "Together - Login",
                Width = 400,
                Height = 500,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Content = new Views.LoginView
                {
                    DataContext = _serviceProvider?.GetRequiredService<LoginViewModel>()
                }
            };
            
            loginWindow.Show();
        }
        
        public void ShowMainWindow(Application.DTOs.UserDto user)
        {
            var mainWindow = _serviceProvider?.GetRequiredService<MainWindow>();
            if (mainWindow != null)
            {
                var mainViewModel = mainWindow.DataContext as MainViewModel;
                mainViewModel?.Initialize(user);
                mainWindow.Show();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}
