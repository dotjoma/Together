using Together.Application.DTOs;
using Together.Application.Exceptions;
using Together.Application.Interfaces;
using Together.Domain.Interfaces;

namespace Together.Application.Services;

public class LongDistanceService : ILongDistanceService
{
    private readonly ICoupleConnectionRepository _connectionRepository;
    private readonly IUserRepository _userRepository;
    private const double EarthRadiusKm = 6371.0;
    private const int CommunicationWindowStartHour = 8;  // 8 AM
    private const int CommunicationWindowEndHour = 22;   // 10 PM

    public LongDistanceService(
        ICoupleConnectionRepository connectionRepository,
        IUserRepository userRepository)
    {
        _connectionRepository = connectionRepository;
        _userRepository = userRepository;
    }

    public async Task<LongDistanceInfoDto> GetLongDistanceInfoAsync(Guid connectionId)
    {
        var connection = await _connectionRepository.GetByIdAsync(connectionId);
        if (connection == null)
            throw new NotFoundException(nameof(connection), connectionId);

        var user1 = await _userRepository.GetByIdAsync(connection.User1Id);
        var user2 = await _userRepository.GetByIdAsync(connection.User2Id);

        if (user1 == null || user2 == null)
            throw new NotFoundException("User", "connection users");

        double? distanceKm = null;
        double? distanceMiles = null;

        // Calculate distance if both users have location data
        if (user1.Latitude.HasValue && user1.Longitude.HasValue &&
            user2.Latitude.HasValue && user2.Longitude.HasValue)
        {
            distanceKm = CalculateDistance(
                user1.Latitude.Value, user1.Longitude.Value,
                user2.Latitude.Value, user2.Longitude.Value);
            distanceMiles = distanceKm * 0.621371; // Convert to miles
        }

        // Get current times in each timezone
        DateTime? user1LocalTime = null;
        DateTime? user2LocalTime = null;
        TimeSpan? timeDifference = null;

        if (!string.IsNullOrEmpty(user1.TimeZoneId))
        {
            var tz1 = TimeZoneInfo.FindSystemTimeZoneById(user1.TimeZoneId);
            user1LocalTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz1);
        }

        if (!string.IsNullOrEmpty(user2.TimeZoneId))
        {
            var tz2 = TimeZoneInfo.FindSystemTimeZoneById(user2.TimeZoneId);
            user2LocalTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz2);
        }

        if (user1LocalTime.HasValue && user2LocalTime.HasValue)
        {
            timeDifference = user2LocalTime.Value - user1LocalTime.Value;
        }

        // Calculate optimal communication window
        CommunicationWindowDto? optimalWindow = null;
        if (!string.IsNullOrEmpty(user1.TimeZoneId) && !string.IsNullOrEmpty(user2.TimeZoneId))
        {
            optimalWindow = CalculateOptimalCommunicationWindow(user1.TimeZoneId, user2.TimeZoneId);
        }

        // Calculate time until next meeting
        TimeSpan? timeUntilMeeting = null;
        if (connection.NextMeetingDate.HasValue)
        {
            timeUntilMeeting = connection.NextMeetingDate.Value - DateTime.UtcNow;
            if (timeUntilMeeting.Value.TotalSeconds < 0)
                timeUntilMeeting = TimeSpan.Zero;
        }

        return new LongDistanceInfoDto(
            distanceKm,
            distanceMiles,
            user1.TimeZoneId,
            user2.TimeZoneId,
            user1LocalTime,
            user2LocalTime,
            timeDifference,
            optimalWindow,
            connection.NextMeetingDate,
            timeUntilMeeting
        );
    }

    public async Task UpdateUserLocationAsync(Guid userId, double latitude, double longitude, string timeZoneId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new NotFoundException(nameof(user), userId);

        // Validate latitude and longitude
        if (latitude < -90 || latitude > 90)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { nameof(latitude), new[] { "Latitude must be between -90 and 90 degrees" } }
            });

        if (longitude < -180 || longitude > 180)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { nameof(longitude), new[] { "Longitude must be between -180 and 180 degrees" } }
            });

        // Validate timezone
        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { nameof(timeZoneId), new[] { "Invalid timezone identifier" } }
            });
        }

        user.UpdateLocation(latitude, longitude, timeZoneId);
        await _userRepository.UpdateAsync(user);
    }

    public async Task SetNextMeetingDateAsync(Guid connectionId, DateTime? nextMeetingDate)
    {
        var connection = await _connectionRepository.GetByIdAsync(connectionId);
        if (connection == null)
            throw new NotFoundException(nameof(connection), connectionId);

        if (nextMeetingDate.HasValue && nextMeetingDate.Value < DateTime.UtcNow)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { nameof(nextMeetingDate), new[] { "Next meeting date must be in the future" } }
            });
        }

        connection.SetNextMeetingDate(nextMeetingDate);
        await _connectionRepository.UpdateAsync(connection);
    }

    public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        // Haversine formula
        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusKm * c;
    }

    public CommunicationWindowDto? CalculateOptimalCommunicationWindow(string timeZone1, string timeZone2)
    {
        try
        {
            var tz1 = TimeZoneInfo.FindSystemTimeZoneById(timeZone1);
            var tz2 = TimeZoneInfo.FindSystemTimeZoneById(timeZone2);

            var now = DateTime.UtcNow;
            var today = now.Date;

            // Create communication windows (8 AM - 10 PM) for each timezone
            var user1WindowStart = new DateTime(today.Year, today.Month, today.Day, CommunicationWindowStartHour, 0, 0);
            var user1WindowEnd = new DateTime(today.Year, today.Month, today.Day, CommunicationWindowEndHour, 0, 0);

            var user2WindowStart = new DateTime(today.Year, today.Month, today.Day, CommunicationWindowStartHour, 0, 0);
            var user2WindowEnd = new DateTime(today.Year, today.Month, today.Day, CommunicationWindowEndHour, 0, 0);

            // Convert to UTC for comparison
            var user1StartUtc = TimeZoneInfo.ConvertTimeToUtc(user1WindowStart, tz1);
            var user1EndUtc = TimeZoneInfo.ConvertTimeToUtc(user1WindowEnd, tz1);
            var user2StartUtc = TimeZoneInfo.ConvertTimeToUtc(user2WindowStart, tz2);
            var user2EndUtc = TimeZoneInfo.ConvertTimeToUtc(user2WindowEnd, tz2);

            // Find overlap
            var overlapStart = user1StartUtc > user2StartUtc ? user1StartUtc : user2StartUtc;
            var overlapEnd = user1EndUtc < user2EndUtc ? user1EndUtc : user2EndUtc;

            // If there's no overlap, return null
            if (overlapStart >= overlapEnd)
                return null;

            // Convert back to local times for display
            var user1OverlapStart = TimeZoneInfo.ConvertTimeFromUtc(overlapStart, tz1);
            var user1OverlapEnd = TimeZoneInfo.ConvertTimeFromUtc(overlapEnd, tz1);
            var user2OverlapStart = TimeZoneInfo.ConvertTimeFromUtc(overlapStart, tz2);
            var user2OverlapEnd = TimeZoneInfo.ConvertTimeFromUtc(overlapEnd, tz2);

            return new CommunicationWindowDto(
                user1WindowStart,
                user1WindowEnd,
                user2WindowStart,
                user2WindowEnd,
                user1OverlapStart,
                user1OverlapEnd
            );
        }
        catch (TimeZoneNotFoundException)
        {
            return null;
        }
    }

    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
}
