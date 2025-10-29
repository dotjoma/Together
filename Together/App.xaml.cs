using System.Windows;
using System.Windows.Threading;
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
using Together.Application.Common;
using Serilog;
using Together.Services;

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
            // Configure Serilog first
            Log.Logger = LoggingConfiguration.ConfigureSerilog();
            Log.Information("Together application starting...");

            // Configure TLS 1.2+ enforcement
            TlsConfigurationService.ConfigureTls();

            // Set up global exception handlers
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

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

            // Database Context with performance optimizations
            services.AddDbContext<TogetherDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("SupabaseConnection"));
                
                // Performance optimizations
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking); // Default to no tracking
            });

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
            
            // Security Services
            services.AddSingleton<IInputValidator, InputValidator>();
            services.AddSingleton<ISecureTokenStorage, WindowsCredentialTokenStorage>();
            services.AddSingleton<IAuditLogger, AuditLogger>();
            services.AddScoped<IPrivacyService, PrivacyService>();
            services.AddScoped<ILocationPermissionService, LocationPermissionService>();
            
            // Navigation Service
            services.AddSingleton<Together.Services.INavigationService, Together.Services.NavigationService>();
            
            // Logging with Serilog
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog(dispose: true);
            });
            
            // Performance Optimization Services
            services.AddMemoryCache();
            services.AddSingleton<IMemoryCacheService, MemoryCacheService>();
            services.AddSingleton<IImageCacheService, ImageCacheService>();
            services.AddSingleton<IOfflineSyncManager, OfflineSyncManager>();
            
            // Update Service
            services.AddSingleton<IUpdateService, UpdateService>();

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
            services.AddTransient<UpdateViewModel>();
            
            // Windows
            services.AddTransient<MainWindow>();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            Log.Information("Application startup initiated");
            
            // Check for updates on startup
            try
            {
                var updateService = _serviceProvider?.GetRequiredService<IUpdateService>();
                if (updateService != null)
                {
                    await updateService.CheckForUpdateOnStartupAsync();
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to check for updates on startup");
            }
            
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
            Log.Information("Login window displayed");
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
            Log.Information("Application shutting down");
            _serviceProvider?.Dispose();
            Log.CloseAndFlush();
            base.OnExit(e);
        }

        #region Global Exception Handlers

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            HandleException(e.Exception);
            e.Handled = true;
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
            {
                HandleException(exception);
            }
        }

        private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            HandleException(e.Exception);
            e.SetObserved();
        }

        private void HandleException(Exception exception)
        {
            try
            {
                // Generate correlation ID for this error
                var correlationId = CorrelationContext.GenerateCorrelationId();

                // Log the exception with correlation ID
                Log.Error(exception, 
                    "[CorrelationId: {CorrelationId}] Unhandled exception occurred: {ExceptionType} - {Message}", 
                    correlationId,
                    exception.GetType().Name,
                    LoggingConfiguration.SanitizeLogMessage(ErrorMessageMapper.GetDetailedMessage(exception)));

                // Get user-friendly message
                var userMessage = ErrorMessageMapper.GetUserFriendlyMessage(exception);

                // Show error dialog to user with correlation ID for support
                MessageBox.Show(
                    $"{userMessage}\n\nError ID: {correlationId}\n\nPlease provide this ID if you contact support.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                // Fallback if error handling itself fails
                MessageBox.Show(
                    "A critical error occurred. Please restart the application.",
                    "Critical Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                // Try to log this as well
                try
                {
                    Log.Fatal(ex, "Critical error in exception handler");
                }
                catch
                {
                    // Nothing we can do at this point
                }
            }
        }

        #endregion
    }
}
