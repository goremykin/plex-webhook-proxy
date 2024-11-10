using System.Text.Json;

namespace PlexWebhookProxy.Sanitizers;

public interface ISanitizer
{
    JsonDocument Sanitize(JsonDocument jsonDocument);
}