namespace HouseholdPanel.Application.Configuration;

public sealed class NewsOptions
{
    public const string SectionName = "News";

    public int ArticlesPerSection { get; init; } = 8;
}