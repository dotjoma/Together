namespace Together.Application.DTOs;

public record LocationDto(
    double Latitude,
    double Longitude,
    string TimeZoneId
);
