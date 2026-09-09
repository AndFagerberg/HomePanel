using System.Security.Cryptography;
using System.Text;
using HouseholdPanel.Application.Configuration;
using Microsoft.Extensions.Options;

namespace HouseholdPanel.Api.Security;

// Guards /api endpoints with a shared-secret header. No-op when Security:ApiKey is unset.
public sealed class ApiKeyMiddleware(RequestDelegate next, ILogger<ApiKeyMiddleware> logger)
{
    private const string HeaderName = "X-Api-Key";

    public async Task InvokeAsync(HttpContext context, IOptions<SecurityOptions> options)
    {
        var apiKey = options.Value.ApiKey;

        if (string.IsNullOrEmpty(apiKey) ||
            !context.Request.Path.StartsWithSegments("/api") ||
            HttpMethods.IsOptions(context.Request.Method))
        {
            await next(context);
            return;
        }

        var providedKey = context.Request.Headers[HeaderName].ToString();

        if (!KeysMatch(providedKey, apiKey))
        {
            logger.LogWarning("Rejected request to {Path}: missing or invalid {Header}", context.Request.Path, HeaderName);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized");
            return;
        }

        await next(context);
    }

    private static bool KeysMatch(string provided, string expected)
    {
        var providedBytes = Encoding.UTF8.GetBytes(provided);
        var expectedBytes = Encoding.UTF8.GetBytes(expected);

        return providedBytes.Length == expectedBytes.Length &&
            CryptographicOperations.FixedTimeEquals(providedBytes, expectedBytes);
    }
}
