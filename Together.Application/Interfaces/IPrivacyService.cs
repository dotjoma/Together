namespace Together.Application.Interfaces;

/// <summary>
/// Service for enforcing privacy controls and data isolation
/// </summary>
public interface IPrivacyService
{
    /// <summary>
    /// Checks if a user has access to couple-specific data
    /// </summary>
    Task<bool> HasCoupleDataAccessAsync(Guid userId, Guid connectionId);

    /// <summary>
    /// Checks if a user can view another user's profile based on visibility settings
    /// </summary>
    Task<bool> CanViewProfileAsync(Guid viewerId, Guid profileOwnerId);

    /// <summary>
    /// Checks if a user can view a post based on privacy settings
    /// </summary>
    Task<bool> CanViewPostAsync(Guid viewerId, Guid postId);

    /// <summary>
    /// Filters a list of user IDs based on profile visibility settings
    /// </summary>
    Task<IEnumerable<Guid>> FilterVisibleUsersAsync(Guid viewerId, IEnumerable<Guid> userIds);

    /// <summary>
    /// Validates that a user is part of a couple connection
    /// </summary>
    Task<bool> IsPartOfConnectionAsync(Guid userId, Guid connectionId);

    /// <summary>
    /// Gets the partner ID for a user if they have a couple connection
    /// </summary>
    Task<Guid?> GetPartnerIdAsync(Guid userId);
}
