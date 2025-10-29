using Together.Application.DTOs;

namespace Together.Application.Interfaces;

public interface ILongDistanceService
{
    Task<LongDistanceInfoDto> GetLongDistanceInfoAsync(Guid connectionId);
    Task UpdateUserLocationAsync(Guid userId, double latitude, double longitude, string timeZoneId);
    Task SetNextMeetingDateAsync(Guid connectionId, DateTime? nextMeetingDate);
    double CalculateDistance(double lat1, double lon1, double lat2, double lon2);
    CommunicationWindowDto? CalculateOptimalCommunicationWindow(string timeZone1, string timeZone2);
}
