namespace HouseholdPanel.Application.Configuration;

// Shared-secret key required on /api requests when configured; empty disables the check.
public sealed class SecurityOptions
{
    public const string SectionName = "Security";

    public string ApiKey { get; init; } = string.Empty;
}
