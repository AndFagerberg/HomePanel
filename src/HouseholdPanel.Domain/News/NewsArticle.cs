namespace HouseholdPanel.Domain.News;

public sealed record NewsArticle(
    string Title,
    string Summary,
    string Source,
    DateTimeOffset PublishedAt,
    string Url);