namespace HouseholdPanel.Application.Configuration;

public sealed class TransportOptions
{
    public const string SectionName = "Transport";

    // Trafiklab Realtime API area id for the stop, e.g. from https://www.trafiklab.se/api/our-apis/trafiklab-realtime-apis/stop-lookup/
    public string StopId { get; init; } = string.Empty;
    public string StopName { get; init; } = string.Empty;

    // Substring matched (case-insensitive) against the departure's route direction text (the text shown on the
    // bus's front sign, e.g. "Högstorp via stationen"). This is NOT the final destination stop name - for loop
    // routes the direction may only mention an intermediate via-point. Check the actual API response for the
    // stop to find the right substring rather than assuming the target city/stop name appears literally.
    public string Direction { get; init; } = string.Empty;

    // Line/route designation to filter on, e.g. "4". Empty means all lines.
    public string Line { get; init; } = string.Empty;

    // Trafiklab API key, see https://www.trafiklab.se/docs/using-trafiklab/getting-api-keys
    public string ApiKey { get; init; } = string.Empty;
}
