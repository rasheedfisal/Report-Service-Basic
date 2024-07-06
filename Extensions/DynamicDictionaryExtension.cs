using System.Text.Json;

namespace Basic_Report.Extensions;

public static class DynamicDictionaryExtension
{
    public static Dictionary<string, object> ConvertToDictionary(JsonElement jsonElement)
    {
        var dictionary = new Dictionary<string, object>();

        // Iterate through properties and add key-value pairs to dictionary
        foreach (JsonProperty property in jsonElement.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.Null)
            {
                dictionary.Add(property.Name, string.Empty);
            }
            else if (property.Value.ValueKind == JsonValueKind.String)
            {
                dictionary.Add(property.Name, property.Value.GetString() ?? string.Empty);
            }
            else if (property.Value.ValueKind == JsonValueKind.Number)
            {
                dictionary.Add(property.Name, property.Value.GetInt32());
            }
            // Add handling for other value types as needed (Boolean, Array, Object, etc.)
        }

        return dictionary;
    }

    public static List<Dictionary<string, object>> ConvertToListOfDictionaries(JsonElement jsonArray)
    {
        return jsonArray.EnumerateArray().Select(ConvertToDictionary).ToList();
    }

    public static bool IsJsonArray(string jsonString)
    {
        // Parse JSON string into JsonDocument
        using JsonDocument jsonDocument = JsonDocument.Parse(jsonString);

        // Check if it's an array
        return jsonDocument.RootElement.ValueKind == JsonValueKind.Array;
    }
}