using Together.Domain.Entities;

namespace Together.Domain.Interfaces;

public interface INotificationRepository
{
    Task AddAsync(Notification notification);
    Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId, int skip, int take);
    Task<int> GetUnreadCountAsync(Guid userId);
}
