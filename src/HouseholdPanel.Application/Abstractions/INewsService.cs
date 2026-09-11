using HouseholdPanel.Domain.News;

namespace HouseholdPanel.Application.Abstractions;

public interface INewsService
{
    Task<IReadOnlyList<NewsArticle>> GetNationalAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<NewsArticle>> GetLocalAsync(CancellationToken cancellationToken);
}