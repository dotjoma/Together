namespace Together.Application.Interfaces;

/// <summary>
/// Service for managing location permissions and privacy
/// </summary>
public interface ILocationPermissionService
{
    /// <summary>
    /// Checks if user has granted location permission
    /// </summary>
    Task<bool> HasLocationPermissionAsync(Guid userId);

    /// <summary>
    /// Requests location permission from user
    /// </summary>
    Task<bool> RequestLocationPermissionAsync(Guid userId);

    /// <summary>
    /// Revokes location permission
    /// </summary>
    Task RevokeLocationPermissionAsync(Guid userId);

    /// <summary>
    /// Gets location sharing status with partner
    /// </summary>
    Task<bool> IsLocationSharedWithPartnerAsync(Guid userId);

    /// <summary>
    /// Updates location sharing preference
    /// </summary>
    Task UpdateLocationSharingAsync(Guid userId, bool shareWithPartner);
}
