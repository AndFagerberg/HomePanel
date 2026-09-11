using System.Net;
using System.Text;
using HouseholdPanel.Application.Configuration;
using HouseholdPanel.Infrastructure.News;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace HouseholdPanel.UnitTests.News;

public sealed class SvtNewsServiceTests
{
    [Fact]
    public async Task GetNationalAsync_MapsRssArticlesAndUsesCachedResult()
    {
        var handler = new StubHttpMessageHandler("""
            <rss><channel><item>
              <title>En rubrik</title>
              <description>En längre ingress.</description>
              <pubDate>Fri, 11 Sep 2026 14:49:58 +0200</pubDate>
              <link>https://www.svt.se/nyheter/en-rubrik</link>
            </item></channel></rss>
            """);
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://www.svt.se/") };
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var sut = new SvtNewsService(httpClient, cache, Options.Create(new NewsOptions()));

        var firstResult = await sut.GetNationalAsync(CancellationToken.None);
        var secondResult = await sut.GetNationalAsync(CancellationToken.None);

        var article = Assert.Single(firstResult);
        Assert.Equal("En rubrik", article.Title);
        Assert.Equal("En längre ingress.", article.Summary);
        Assert.Equal("SVT Nyheter", article.Source);
        Assert.Equal("https://www.svt.se/nyheter/en-rubrik", article.Url);
        Assert.Same(firstResult, secondResult);
        Assert.Equal(1, handler.RequestCount);
    }

    private sealed class StubHttpMessageHandler(string content) : HttpMessageHandler
    {
        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestCount++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/xml"),
            });
        }
    }
}