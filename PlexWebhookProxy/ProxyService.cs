using System.Text;
using System.Text.Json;
using PlexWebhookProxy.Sanitizers;

namespace PlexWebhookProxy;

public class ProxyService
{
    private readonly ILogger<ProxyService> logger;
    private readonly HttpClient httpClient;
    private readonly IServiceProvider serviceProvider;
    private readonly ProxyConfig config;

    public ProxyService(
        ILogger<ProxyService> logger,
        HttpClient httpClient,
        IServiceProvider serviceProvider,
        ProxyConfig config)
    {
        this.logger = logger;
        this.httpClient = httpClient;
        this.serviceProvider = serviceProvider;
        this.config = config;
    }

    public async Task HandleAsync(string payloadJson)
    {
        using var payload = JsonDocument.Parse(payloadJson);
        var root = payload.RootElement;
        var eventType = root.GetProperty("event").GetString();
        var account = root.GetProperty("Account");
        var userId = account.GetProperty("id").GetUInt32();
        var userName = account.GetProperty("title").GetString();

        var appropriateRules = config.Rules
            .Where(rule => rule.Events.Length == 0 || rule.Events.Contains(eventType))
            .Where(rule => rule.UserId == default && string.IsNullOrEmpty(rule.UserName) ||
                           rule.UserId != default && rule.UserId == userId ||
                           rule.UserName?.Length > 0 && rule.UserName == userName)
            .ToArray();

        if (appropriateRules.Length == 0)
        {
            logger.LogInformation("No rules found for user {UserId} and event \"{Event}\". Skipping.", userId, eventType);
            return;
        }

        var tasks = appropriateRules.Select(rule => TryRouteAsync(payload, rule));
        await Task.WhenAll(tasks);
    }

    private async Task TryRouteAsync(JsonDocument payload, ProxyRule rule)
    {
        try
        {
            ISanitizer? sanitizer = rule.Sanitizer switch
            {
                "simkl" => serviceProvider.GetRequiredService<SimklSanitizer>(),
                _ => null
            };

            if (sanitizer != null)
            {
                payload = sanitizer.Sanitize(payload);
            }

            var payloadJson = payload.RootElement.GetRawText();
            var payloadHttpContent = new StringContent(payloadJson, Encoding.UTF8, "application/json");
            var form = new MultipartFormDataContent();
            form.Add(payloadHttpContent, "payload");

            var response = await httpClient.PostAsync(rule.WebHookUrl, form);

            response.EnsureSuccessStatusCode();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to route to {Url}", rule.WebHookUrl);
        }
    }
}