using Microsoft.Extensions.Logging;
using Together.Application.Interfaces;
using Together.Domain.Interfaces;

namespace Together.Application.Services;

/// <summary>
/// Implementation of location permission service for privacy-compliant location handling
/// </summary>
public class LocationPermissionService : ILocationPermissionService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditLogger _auditLogger;
    private readonly ILogger<LocationPermissionService> _logger;
    private readonly Dictionary<Guid, LocationPermissionState> _permissionCache;

    public LocationPermissionService(
        IUserRepository userRepository,
        IAuditLogger auditLogger,
        ILogger<LocationPermissionService> logger)
    {
        _userRepository = userRepository;
        _auditLogger = auditLogger;
        _logger = logger;
        _permissionCache = new Dictionary<Guid, LocationPermissionState>();
    }

    public Task<bool> HasLocationPermissionAsync(Guid userId)
    {
        if (_permissionCache.TryGetValue(userId, out var state))
        {
            return Task.FromResult(state.HasPermission);
        }

        // Default to no permission
        return Task.FromResult(false);
    }

    public async Task<bool> RequestLocationPermissionAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("Location permission requested for user: {UserId}", userId);

            // In a real implementation, this would show a system dialog
            // For now, we'll simulate user granting permission
            var granted = true; // This would come from actual user interaction

            if (granted)
            {
                _permissionCache[userId] = new LocationPermissionState
                {
                    HasPermission = true,
                    GrantedAt = DateTime.UtcNow,
                    ShareWithPartner = false
                };

                await _auditLogger.LogPrivacyEventAsync(
                    userId,
                    "LocationPermissionGranted",
                    "User granted location access permission");

                _logger.LogInformation("Location permission granted for user: {UserId}", userId);
            }
            else
            {
                await _auditLogger.LogPrivacyEventAsync(
                    userId,
                    "LocationPermissionDenied",
                    "User denied location access permission");

                _logger.LogInformation("Location permission denied for user: {UserId}", userId);
            }

            return granted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting location permission for user: {UserId}", userId);
            return false;
        }
    }

    public async Task RevokeLocationPermissionAsync(Guid userId)
    {
        try
        {
            if (_permissionCache.ContainsKey(userId))
            {
                _permissionCache.Remove(userId);
            }

            await _auditLogger.LogPrivacyEventAsync(
                userId,
                "LocationPermissionRevoked",
                "User revoked location access permission");

            _logger.LogInformation("Location permission revoked for user: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking location permission for user: {UserId}", userId);
        }
    }

    public Task<bool> IsLocationSharedWithPartnerAsync(Guid userId)
    {
        if (_permissionCache.TryGetValue(userId, out var state))
        {
            return Task.FromResult(state.ShareWithPartner);
        }

        return Task.FromResult(false);
    }

    public async Task UpdateLocationSharingAsync(Guid userId, bool shareWithPartner)
    {
        try
        {
            if (!_permissionCache.TryGetValue(userId, out var state))
            {
                // User hasn't granted location permission yet
                _logger.LogWarning("Attempted to update location sharing without permission for user: {UserId}", userId);
                return;
            }

            state.ShareWithPartner = shareWithPartner;
            _permissionCache[userId] = state;

            await _auditLogger.LogPrivacyEventAsync(
                userId,
                "LocationSharingUpdated",
                $"Location sharing with partner set to: {shareWithPartner}");

            _logger.LogInformation("Location sharing updated for user: {UserId}, ShareWithPartner: {ShareWithPartner}",
                userId, shareWithPartner);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating location sharing for user: {UserId}", userId);
        }
    }

    private class LocationPermissionState
    {
        public bool HasPermission { get; set; }
        public DateTime GrantedAt { get; set; }
        public bool ShareWithPartner { get; set; }
    }
}
