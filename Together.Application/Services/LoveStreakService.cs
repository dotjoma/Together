using Together.Application.Exceptions;
using Together.Application.Interfaces;
using Together.Domain.Entities;
using Together.Domain.Enums;
using Together.Domain.Interfaces;

namespace Together.Application.Services;

public class LoveStreakService : ILoveStreakService
{
    private readonly ICoupleConnectionRepository _connectionRepository;
    private readonly INotificationRepository _notificationRepository;
    private static readonly int[] MilestoneValues = { 7, 30, 100, 365 };

    public LoveStreakService(
        ICoupleConnectionRepository connectionRepository,
        INotificationRepository notificationRepository)
    {
        _connectionRepository = connectionRepository;
        _notificationRepository = notificationRepository;
    }

    public async Task RecordInteractionAsync(Guid connectionId, InteractionType interactionType)
    {
        var connection = await _connectionRepository.GetByIdAsync(connectionId);
        if (connection == null)
            throw new NotFoundException(nameof(CoupleConnection), connectionId);

        var today = DateTime.UtcNow.Date;
        var lastInteractionDate = connection.LastInteractionDate?.Date;

        // Check if this is the first interaction today
        if (lastInteractionDate == null || lastInteractionDate < today)
        {
            // Check if streak should continue (interaction within 24 hours)
            if (lastInteractionDate.HasValue && (today - lastInteractionDate.Value).TotalDays == 1)
            {
                // Increment streak for consecutive day
                var previousStreak = connection.LoveStreak;
                connection.IncrementStreak();

                // Check for milestone achievement
                await CheckAndNotifyMilestoneAsync(connection, previousStreak);
            }
            else if (lastInteractionDate.HasValue && (today - lastInteractionDate.Value).TotalDays > 1)
            {
                // Reset streak if more than 24 hours passed
                connection.ResetStreak();
                connection.IncrementStreak(); // Start new streak at 1
            }
            else if (!lastInteractionDate.HasValue)
            {
                // First ever interaction
                connection.IncrementStreak();
            }
        }
        else
        {
            // Same day interaction - just update timestamp
            connection.RecordInteraction();
        }

        await _connectionRepository.UpdateAsync(connection);
    }

    public async Task<int> GetCurrentStreakAsync(Guid connectionId)
    {
        var connection = await _connectionRepository.GetByIdAsync(connectionId);
        if (connection == null)
            throw new NotFoundException(nameof(CoupleConnection), connectionId);

        return connection.LoveStreak;
    }

    public async Task CheckAndResetStreakAsync(Guid connectionId)
    {
        var connection = await _connectionRepository.GetByIdAsync(connectionId);
        if (connection == null)
            throw new NotFoundException(nameof(CoupleConnection), connectionId);

        if (connection.LastInteractionDate.HasValue)
        {
            var hoursSinceLastInteraction = (DateTime.UtcNow - connection.LastInteractionDate.Value).TotalHours;

            // Reset if more than 24 hours of inactivity
            if (hoursSinceLastInteraction > 24)
            {
                connection.ResetStreak();
                await _connectionRepository.UpdateAsync(connection);

                // Notify both partners about streak loss
                await NotifyStreakLostAsync(connection);
            }
        }
    }

    public async Task<IEnumerable<int>> GetStreakMilestonesAsync(Guid connectionId)
    {
        var connection = await _connectionRepository.GetByIdAsync(connectionId);
        if (connection == null)
            throw new NotFoundException(nameof(CoupleConnection), connectionId);

        // Return milestones that have been achieved
        return MilestoneValues.Where(m => connection.LoveStreak >= m).ToList();
    }

    private async Task CheckAndNotifyMilestoneAsync(CoupleConnection connection, int previousStreak)
    {
        var currentStreak = connection.LoveStreak;

        // Check if a milestone was just reached
        var milestoneReached = MilestoneValues.FirstOrDefault(m => m == currentStreak && m > previousStreak);

        if (milestoneReached > 0)
        {
            await NotifyMilestoneAchievedAsync(connection, milestoneReached);
        }
    }

    private async Task NotifyMilestoneAchievedAsync(CoupleConnection connection, int milestone)
    {
        var message = milestone switch
        {
            7 => "🎉 Amazing! You've reached a 7-day love streak!",
            30 => "🌟 Incredible! 30 days of staying connected!",
            100 => "💯 Wow! 100 days of love and dedication!",
            365 => "🏆 Legendary! A full year of daily connection!",
            _ => $"🎊 Milestone reached: {milestone} days!"
        };

        // Create notifications for both partners
        var notification1 = new Notification(
            connection.User1Id,
            "Love Streak Milestone",
            message,
            "LoveStreak"
        );

        var notification2 = new Notification(
            connection.User2Id,
            "Love Streak Milestone",
            message,
            "LoveStreak"
        );

        await _notificationRepository.AddAsync(notification1);
        await _notificationRepository.AddAsync(notification2);
    }

    private async Task NotifyStreakLostAsync(CoupleConnection connection)
    {
        var message = "Your love streak has been reset. Start a new one today! 💪";

        var notification1 = new Notification(
            connection.User1Id,
            "Love Streak Reset",
            message,
            "LoveStreak"
        );

        var notification2 = new Notification(
            connection.User2Id,
            "Love Streak Reset",
            message,
            "LoveStreak"
        );

        await _notificationRepository.AddAsync(notification1);
        await _notificationRepository.AddAsync(notification2);
    }
}
