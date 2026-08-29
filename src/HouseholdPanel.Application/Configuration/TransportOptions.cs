namespace HouseholdPanel.Application.Configuration;

public sealed class TransportOptions
{
    public const string SectionName = "Transport";

    public string StopId { get; init; } = string.Empty;
    public string StopName { get; init; } = string.Empty;
    public string Direction { get; init; } = string.Empty;
}
