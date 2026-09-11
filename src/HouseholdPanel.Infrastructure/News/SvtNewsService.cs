using System.Globalization;
using System.Xml.Linq;
using HouseholdPanel.Application.Abstractions;
using HouseholdPanel.Application.Configuration;
using HouseholdPanel.Domain.News;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace HouseholdPanel.Infrastructure.News;

public sealed class SvtNewsService(
    HttpClient httpClient,
    IMemoryCache cache,
    IOptions<NewsOptions> newsOptions) : INewsService
{
    private const string NationalFeed = "nyheter/rss.xml";
    private const string LocalFeed = "nyheter/lokalt/smaland/rss.xml";

    public Task<IReadOnlyList<NewsArticle>> GetNationalAsync(CancellationToken cancellationToken) =>
        GetArticlesAsync(NationalFeed, "SVT Nyheter", cancellationToken);

    public Task<IReadOnlyList<NewsArticle>> GetLocalAsync(CancellationToken cancellationToken) =>
        GetArticlesAsync(LocalFeed, "SVT Nyheter Småland", cancellationToken);

    private async Task<IReadOnlyList<NewsArticle>> GetArticlesAsync(
        string feedPath,
        string source,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"svt-news:{feedPath}";
                if (cache.TryGetValue(cacheKey, out IReadOnlyList<NewsArticle>? cachedArticles)
                        && cachedArticles is not null)
        {
            return cachedArticles;
        }

        var feed = await httpClient.GetStringAsync(feedPath, cancellationToken);
        var articles = XDocument.Parse(feed)
            .Descendants("item")
            .Select(item => new NewsArticle(
                Title: GetValue(item, "title"),
                Summary: GetValue(item, "description"),
                Source: source,
                PublishedAt: DateTimeOffset.TryParse(
                    GetValue(item, "pubDate"),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal,
                    out var publishedAt) ? publishedAt : DateTimeOffset.MinValue,
                Url: GetValue(item, "link")))
            .Where(article => !string.IsNullOrWhiteSpace(article.Title) && !string.IsNullOrWhiteSpace(article.Url))
            .OrderByDescending(article => article.PublishedAt)
            .Take(Math.Clamp(newsOptions.Value.ArticlesPerSection, 1, 20))
            .ToList();

        cache.Set(cacheKey, articles, TimeSpan.FromMinutes(5));
        return articles;
    }

    private static string GetValue(XElement item, string name) =>
        item.Element(name)?.Value.Trim() ?? string.Empty;
}