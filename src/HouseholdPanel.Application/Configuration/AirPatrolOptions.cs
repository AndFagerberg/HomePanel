namespace HouseholdPanel.Application.Configuration;

public sealed class AirPatrolOptions
{
    public const string SectionName = "AirPatrol";

    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PairingId { get; set; } = string.Empty;
    public string Name { get; set; } = "Stugan";
}