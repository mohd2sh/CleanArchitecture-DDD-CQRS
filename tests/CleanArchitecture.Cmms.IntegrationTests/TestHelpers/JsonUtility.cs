using System.Text.Json;

namespace CleanArchitecture.Cmms.IntegrationTests.TestHelpers;

public static class JsonUtility
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        IncludeFields = false
    };

    /// <summary>
    /// Deserialize JSON string to specified type.
    /// </summary>
    public static T Deserialize<T>(string json) where T : class
    {
        return JsonSerializer.Deserialize<T>(json, DefaultOptions);
    }

    /// <summary>
    /// Deserialize HTTP response content to specified type.
    /// </summary>
    public static async Task<T> DeserializeAsync<T>(HttpContent content) where T : class
    {
        var json = await content.ReadAsStringAsync();
        return Deserialize<T>(json);
    }
}

