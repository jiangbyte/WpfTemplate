using System.Text.Json;
using System.Text.Json.Nodes;

namespace WpfTemplate.Helpers;

/// <summary>
/// 对齐 web/portal <c>src/utils/wire.ts</c> 的 <c>stringifyScalars</c>：
/// HTTP JSON 请求体中布尔/数字标量一律写成字符串。
/// </summary>
public static class WireJson
{
    public static bool ReadBool(object? value, bool defaultValue = false)
    {
        return value switch
        {
            null => defaultValue,
            bool b => b,
            string s when bool.TryParse(s, out var parsed) => parsed,
            string s when s == "1" => true,
            string s when s == "0" => false,
            JsonElement { ValueKind: JsonValueKind.True } => true,
            JsonElement { ValueKind: JsonValueKind.False } => false,
            JsonElement { ValueKind: JsonValueKind.String } el => ReadBool(el.GetString(), defaultValue),
            _ => defaultValue,
        };
    }

    public static string Serialize(object? value, JsonSerializerOptions? options = null)
    {
        if (value is null)
        {
            return "null";
        }

        using var document = JsonSerializer.SerializeToDocument(value, options);
        var node = StringifyElement(document.RootElement);
        return node?.ToJsonString() ?? "null";
    }

    private static JsonNode? StringifyElement(JsonElement element) =>
        element.ValueKind switch
        {
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            JsonValueKind.True => JsonValue.Create("true"),
            JsonValueKind.False => JsonValue.Create("false"),
            JsonValueKind.Number => JsonValue.Create(element.GetRawText()),
            JsonValueKind.String => JsonValue.Create(element.GetString()),
            JsonValueKind.Array => StringifyArray(element),
            JsonValueKind.Object => StringifyObject(element),
            _ => JsonValue.Create(element.ToString()),
        };

    private static JsonArray StringifyArray(JsonElement element)
    {
        var array = new JsonArray();
        foreach (var item in element.EnumerateArray())
        {
            array.Add(StringifyElement(item));
        }

        return array;
    }

    private static JsonObject StringifyObject(JsonElement element)
    {
        var obj = new JsonObject();
        foreach (var property in element.EnumerateObject())
        {
            obj[property.Name] = StringifyElement(property.Value);
        }

        return obj;
    }
}
