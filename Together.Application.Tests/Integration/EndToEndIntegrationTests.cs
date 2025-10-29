using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Together.Application.Interfaces;
using Together.Application.Services;
using Together.Application.DTOs;
using Together.Domain.Interfaces;
using Together.Domain.Enums;
using Together.Infrastructure.Data;
using Together.Infrastructure.Repositories;
using Together.Infrastructure.Services;

namespace Together.Application.Tests.Integration
{
    /// <summary>
    /// Integration tests for end-to-end user journeys
    /// Tests complete flows from registration to couple features
    /// </summary>
    public class EndToEndIntegrationTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly TogetherDbContext _dbContext;

        public EndToEndIntegrationTests()
        {
            var services = new ServiceCollection();
            ConfigureTestServices(services);
            _serviceProvider = services.BuildServiceProvider();
            _dbContext = _serviceProvider.GetRequiredService<TogetherDbContext>();
        }

        private void ConfigureTestServices(IServiceCollection services)
        {
            // Configuration
            var configDict = new Dictionary<string, string?>
            {
                ["ConnectionStrings:SupabaseConnection"] = "Host=localhost;Database=together_test;Username=test;Password=test",
                ["Supabase:Url"] = "https://test.supabase.co",
                ["Supabase:Key"] = "test-key",
                ["Jwt:Secret"] = "test-secret-key-for-jwt-token-generation-minimum-32-chars",
                ["Jwt:Issuer"] = "together-test",
                ["Jwt:Audience"] = "together-test-users"
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();
            services.AddSingleton<IConfiguration>(configuration);

            // Use in-memory database for testing
            services.AddDbContext<TogetherDbContext>(options =>
            {
                options.UseInMemoryDatabase("TogetherTestDb_" + Guid.NewGuid());
            });

            // Register all repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICoupleConnectionRepository, CoupleConnectionRepository>();
            services.AddScoped<IConnectionRequestRepository, ConnectionRequestRepository>();
            services.AddScoped<IPostRepository, PostRepository>();
            services.AddScoped<IJournalEntryRepository, JournalEntryRepository>();
            services.AddScoped<IMoodEntryRepository, MoodEntryRepository>();
            services.AddScoped<ITodoItemRepository, TodoItemRepository>();
            services.AddScoped<ISharedEventRepository, SharedEventRepository>();
            services.AddScoped<IVirtualPetRepository, VirtualPetRepository>();
            services.AddScoped<IChallengeRepository, ChallengeRepository>();
            services.AddScoped<IFollowRelationshipRepository, FollowRelationshipRepository>();
            services.AddScoped<ILikeRepository, LikeRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();

            // Add logging
            services.AddLogging();

            // Register all services
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<ICoupleConnectionService, CoupleConnectionService>();
            services.AddScoped<IJournalService, JournalService>();
            services.AddScoped<IMoodTrackingService, MoodTrackingService>();
            services.AddScoped<IMoodAnalysisService, MoodAnalysisService>();
            services.AddScoped<ITodoService, TodoService>();
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<ILoveStreakService, LoveStreakService>();
            services.AddScoped<IChallengeService, ChallengeService>();
            services.AddScoped<IVirtualPetService, VirtualPetService>();
            services.AddScoped<IFollowService, FollowService>();
            services.AddScoped<IPostService, PostService>();
            services.AddScoped<ISocialFeedService, SocialFeedService>();
            services.AddScoped<ILikeService, LikeService>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<ILongDistanceService, LongDistanceService>();
            services.AddScoped<IDashboardService, DashboardService>();

            // Mock storage service for testing
            services.AddScoped<IStorageService, MockStorageService>();
        }

        [Fact]
        public async Task CompleteUserJourney_FromRegistrationToCoupleFeatures_Success()
        {
            // Arrange
            var authService = _serviceProvider.GetRequiredService<IAuthenticationService>();
            var connectionService = _serviceProvider.GetRequiredService<ICoupleConnectionService>();
            var journalService = _serviceProvider.GetRequiredService<IJournalService>();
            var moodService = _serviceProvider.GetRequiredService<IMoodTrackingService>();

            // Act & Assert - Step 1: Register two users
            var user1Result = await authService.RegisterAsync(new RegisterDto(
                "testuser1",
                "user1@test.com",
                "Password123"
            ));
            Assert.True(user1Result.Success);
            Assert.NotNull(user1Result.User);

            var user2Result = await authService.RegisterAsync(new RegisterDto(
                "testuser2",
                "user2@test.com",
                "Password123"
            ));
            Assert.True(user2Result.Success);
            Assert.NotNull(user2Result.User);

            // Step 2: Login both users
            var login1 = await authService.LoginAsync(new LoginDto("user1@test.com", "Password123"));
            Assert.True(login1.Success);
            Assert.NotEmpty(login1.Token);

            var login2 = await authService.LoginAsync(new LoginDto("user2@test.com", "Password123"));
            Assert.True(login2.Success);
            Assert.NotEmpty(login2.Token);

            // Step 3: Establish couple connection
            var connectionRequest = await connectionService.SendConnectionRequestAsync(
                user1Result.User.Id,
                user2Result.User.Id
            );
            Assert.NotNull(connectionRequest);

            var connection = await connectionService.AcceptConnectionRequestAsync(
                connectionRequest.Id,
                user2Result.User.Id
            );
            Assert.NotNull(connection);

            // Step 4: Create journal entry
            var journalEntry = await journalService.CreateJournalEntryAsync(new CreateJournalEntryDto(
                connection.Id,
                user1Result.User.Id,
                "Our first journal entry together!"
            ));
            Assert.NotNull(journalEntry);
            Assert.Equal("Our first journal entry together!", journalEntry.Content);

            // Step 5: Log mood
            var moodEntry = await moodService.CreateMoodEntryAsync(user1Result.User.Id, new CreateMoodEntryDto(
                "Happy",
                "Feeling great today!"
            ));
            Assert.NotNull(moodEntry);
            Assert.Equal("Happy", moodEntry.Mood);

            // Step 6: Verify couple connection exists
            var retrievedConnection = await connectionService.GetUserConnectionAsync(user1Result.User.Id);
            Assert.NotNull(retrievedConnection);
            Assert.Equal(connection.Id, retrievedConnection.Id);
        }

        [Fact]
        public async Task SocialFeatures_CompleteFlow_Success()
        {
            // Arrange
            var authService = _serviceProvider.GetRequiredService<IAuthenticationService>();
            var followService = _serviceProvider.GetRequiredService<IFollowService>();
            var postService = _serviceProvider.GetRequiredService<IPostService>();
            var likeService = _serviceProvider.GetRequiredService<ILikeService>();
            var commentService = _serviceProvider.GetRequiredService<ICommentService>();
            var feedService = _serviceProvider.GetRequiredService<ISocialFeedService>();

            // Register users
            var user1 = await authService.RegisterAsync(new RegisterDto("social1", "social1@test.com", "Password123"));
            var user2 = await authService.RegisterAsync(new RegisterDto("social2", "social2@test.com", "Password123"));

            // Follow relationship
            var followRequest = await followService.SendFollowRequestAsync(user1.User.Id, user2.User.Id);
            await followService.AcceptFollowRequestAsync(followRequest.Id);

            // Create post
            var post = await postService.CreatePostAsync(user2.User.Id, new CreatePostDto(
                "Hello from my first post!",
                new List<string>()
            ));
            Assert.NotNull(post);

            // Like post
            var liked = await likeService.ToggleLikeAsync(post.Id, user1.User.Id);
            Assert.True(liked);

            // Comment on post
            var comment = await commentService.AddCommentAsync(user1.User.Id, new CreateCommentDto(
                post.Id,
                "Great post!"
            ));
            Assert.NotNull(comment);

            // Get feed
            var feed = await feedService.GetFeedAsync(user1.User.Id);
            Assert.NotNull(feed);
            Assert.NotEmpty(feed.Posts);
        }

        [Fact]
        public async Task CoupleEngagementFeatures_CompleteFlow_Success()
        {
            // Arrange
            var authService = _serviceProvider.GetRequiredService<IAuthenticationService>();
            var connectionService = _serviceProvider.GetRequiredService<ICoupleConnectionService>();
            var todoService = _serviceProvider.GetRequiredService<ITodoService>();
            var eventService = _serviceProvider.GetRequiredService<IEventService>();
            var streakService = _serviceProvider.GetRequiredService<ILoveStreakService>();
            var challengeService = _serviceProvider.GetRequiredService<IChallengeService>();
            var petService = _serviceProvider.GetRequiredService<IVirtualPetService>();

            // Setup couple
            var user1 = await authService.RegisterAsync(new RegisterDto("couple1", "couple1@test.com", "Password123"));
            var user2 = await authService.RegisterAsync(new RegisterDto("couple2", "couple2@test.com", "Password123"));
            
            var request = await connectionService.SendConnectionRequestAsync(user1.User.Id, user2.User.Id);
            var connection = await connectionService.AcceptConnectionRequestAsync(request.Id, user2.User.Id);

            // Create todo
            var todo = await todoService.CreateTodoItemAsync(user1.User.Id, new CreateTodoItemDto(
                "Plan date night",
                "Find a nice restaurant",
                user1.User.Id,
                DateTime.UtcNow.AddDays(7),
                new List<string> { "date", "planning" }
            ));
            Assert.NotNull(todo);

            // Create event
            var eventDto = await eventService.CreateEventAsync(user1.User.Id, new CreateEventDto(
                "Anniversary Dinner",
                DateTime.UtcNow.AddMonths(1),
                "Celebrate our anniversary",
                "none"
            ));
            Assert.NotNull(eventDto);

            // Record interaction for streak
            await streakService.RecordInteractionAsync(connection.Id, InteractionType.JournalEntry);
            var streak = await streakService.GetCurrentStreakAsync(connection.Id);
            Assert.True(streak >= 1);

            // Generate challenge
            var challenge = await challengeService.GenerateDailyChallengeAsync(connection.Id);
            Assert.NotNull(challenge);

            // Get virtual pet
            var pet = await petService.GetPetAsync(connection.Id);
            Assert.NotNull(pet);
        }

        [Fact]
        public async Task DashboardAggregation_RetrievesAllData_Success()
        {
            // Arrange
            var authService = _serviceProvider.GetRequiredService<IAuthenticationService>();
            var connectionService = _serviceProvider.GetRequiredService<ICoupleConnectionService>();
            var moodService = _serviceProvider.GetRequiredService<IMoodTrackingService>();
            var dashboardService = _serviceProvider.GetRequiredService<IDashboardService>();

            // Setup
            var user1 = await authService.RegisterAsync(new RegisterDto("dash1", "dash1@test.com", "Password123"));
            var user2 = await authService.RegisterAsync(new RegisterDto("dash2", "dash2@test.com", "Password123"));
            
            var request = await connectionService.SendConnectionRequestAsync(user1.User.Id, user2.User.Id);
            await connectionService.AcceptConnectionRequestAsync(request.Id, user2.User.Id);

            // Add mood
            await moodService.CreateMoodEntryAsync(user2.User.Id, new CreateMoodEntryDto(
                "Happy",
                "Great day!"
            ));

            // Get dashboard
            var dashboard = await dashboardService.GetDashboardSummaryAsync(user1.User.Id);
            Assert.NotNull(dashboard);
            Assert.NotNull(dashboard.PartnerMood);
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
            _serviceProvider?.Dispose();
        }

        // Mock storage service for testing
        private class MockStorageService : IStorageService
        {
            public Task<string> UploadProfilePictureAsync(byte[] imageData, string fileName, Guid userId)
            {
                return Task.FromResult($"https://mock-storage.com/{userId}/{fileName}");
            }

            public Task<bool> DeleteProfilePictureAsync(string fileUrl)
            {
                return Task.FromResult(true);
            }

            public Task<byte[]> CompressImageAsync(byte[] imageData, int maxSizeInBytes)
            {
                return Task.FromResult(imageData);
            }

            public Task<string> UploadImageAsync(string filePath, string folder)
            {
                return Task.FromResult($"https://mock-storage.com/{folder}/{Path.GetFileName(filePath)}");
            }

            public Task<bool> DeleteImageAsync(string fileUrl)
            {
                return Task.FromResult(true);
            }

            public Task<string> UploadFileAsync(Stream fileStream, string filePath, string contentType)
            {
                return Task.FromResult($"https://mock-storage.com/{filePath}");
            }

            public Task<bool> DeleteFileAsync(string fileUrl)
            {
                return Task.FromResult(true);
            }
        }
    }
}
