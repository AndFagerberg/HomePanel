namespace HouseholdPanel.Domain.AirPatrol;

public sealed record AirPatrolStatus(
    string Name,
    decimal Temperature,
    int? Humidity,
    bool Power,
    string Mode,
    decimal? TargetTemperature,
    string FanSpeed,
    bool Swing,
    DateTimeOffset UpdatedAt);