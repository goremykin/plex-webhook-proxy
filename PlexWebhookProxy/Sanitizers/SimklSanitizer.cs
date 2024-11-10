using System.Text.Json;
using System.Text.Json.Nodes;

namespace PlexWebhookProxy.Sanitizers;

public class SimklSanitizer : ISanitizer
{
    private readonly IReadOnlyCollection<string> metadataStringProps =
    [
        "guid",
        "parentGuid",
        "grandparentGuid",
        "grandparentSlug",
        "title",
        "parentTitle",
        "grandparentTitle",
        "originalTitle",
        "type",
        "slug"
    ];
    private readonly IReadOnlyCollection<string> metadataIntProps =
    [
        "year",
        "index",
        "parentIndex"
    ];
    private readonly IReadOnlyCollection<string> metadataArrayProps = ["Guid"];

    public JsonDocument Sanitize(JsonDocument jsonDocument)
    {
        var sourceRoot = jsonDocument.RootElement;
        var resultObj = new JsonObject
        {
            ["event"] = sourceRoot.GetProperty("event").GetString(),
            ["user"] = "anonymous",
            ["Metadata"] = ExtractMetadata(sourceRoot)
        };
        var resultJson = resultObj.ToJsonString();

        return JsonDocument.Parse(resultJson);
    }

    private JsonObject ExtractMetadata(JsonElement root)
    {
        var sourceMetadata = root.GetProperty("Metadata");
        var result = new JsonObject();

        foreach (var propertyName in metadataStringProps)
        {
            if (sourceMetadata.TryGetProperty(propertyName, out var property))
            {
                result[propertyName] = property.GetString();
            }
        }

        foreach (var propertyName in metadataIntProps)
        {
            if (sourceMetadata.TryGetProperty(propertyName, out var property))
            {
                result[propertyName] = property.GetInt32();
            }
        }

        foreach (var propertyName in metadataArrayProps)
        {
            if (sourceMetadata.TryGetProperty(propertyName, out var property))
            {
                result[propertyName] = JsonArray.Create(property);
            }
        }

        return result;
    }
}