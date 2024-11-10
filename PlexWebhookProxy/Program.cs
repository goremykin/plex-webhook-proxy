using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PlexWebhookProxy;
using PlexWebhookProxy.Sanitizers;

var builder = WebApplication.CreateSlimBuilder(args);
var rulesJson = Environment.GetEnvironmentVariable("PROXY_RULES");

if (string.IsNullOrEmpty(rulesJson))
{
    throw new ApplicationException("PROXY_RULES environment variable is missing");
}

var rules = JsonSerializer.Deserialize(rulesJson, SourceGenerationContext.Default.ProxyRuleArray);

if (rules == null || rules.Length == 0)
{
    throw new AggregateException("No rules found");
}

var config = new ProxyConfig { Rules = rules };

builder.Services.AddHealthChecks();

builder.Services
    .AddLogging(configure => configure.AddConsole())
    .AddTransient<ProxyService>()
    .AddSingleton<SimklSanitizer>()
    .AddSingleton(config)
    .AddHttpClient<ProxyService>();

var app = builder.Build();

app.MapHealthChecks("/healthz");

app.MapPost("/", async ([FromForm] string payload, [FromServices] ProxyService proxyService) =>
    {
        await proxyService.HandleAsync(payload);
    })
    .DisableAntiforgery();

app.Run();