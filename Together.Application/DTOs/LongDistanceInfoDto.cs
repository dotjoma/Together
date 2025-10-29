namespace Together.Application.DTOs;

public record LongDistanceInfoDto(
    double? DistanceInKilometers,
    double? DistanceInMiles,
    string? User1TimeZone,
    string? User2TimeZone,
    DateTime? User1LocalTime,
    DateTime? User2LocalTime,
    TimeSpan? TimeDifference,
    CommunicationWindowDto? OptimalWindow,
    DateTime? NextMeetingDate,
    TimeSpan? TimeUntilMeeting
);

public record CommunicationWindowDto(
    DateTime User1WindowStart,
    DateTime User1WindowEnd,
    DateTime User2WindowStart,
    DateTime User2WindowEnd,
    DateTime OverlapStart,
    DateTime OverlapEnd
);
