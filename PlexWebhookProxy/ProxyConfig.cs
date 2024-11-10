namespace PlexWebhookProxy;

public class ProxyConfig
{
    public ProxyRule[] Rules { get; init; } = [];
}

public class ProxyRule
{
    public uint UserId { get; init; }
    public string? UserName { get; init; }
    public required string WebHookUrl { get; init; }
    public string[] Events { get; init; } = [];
    public string? Sanitizer { get; init; }
}