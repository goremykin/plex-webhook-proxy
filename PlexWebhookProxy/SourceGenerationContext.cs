using System.Text.Json.Serialization;

namespace PlexWebhookProxy;

[JsonSerializable(typeof(ProxyRule[]))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}