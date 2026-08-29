namespace HouseholdPanel.Domain.Transport;

public sealed record Departure(
    DateTimeOffset DepartureTime,
    string Destination,
    string Line,
    int MinutesUntilDeparture);
