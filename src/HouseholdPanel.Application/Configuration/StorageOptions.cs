namespace HouseholdPanel.Application.Configuration;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public string DatabasePath { get; set; } = "data/homepanel.db";
}