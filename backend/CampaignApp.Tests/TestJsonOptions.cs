using System.Text.Json;
using System.Text.Json.Serialization;

namespace CampaignApp.Tests;

/// <summary>
/// The server (Program.cs) serializes enums as strings via a
/// JsonStringEnumConverter registered on MVC's JSON options; a test's
/// HttpClient uses System.Text.Json defaults for its own
/// ReadFromJsonAsync/GetFromJsonAsync calls, which doesn't know about that
/// converter, so any DTO with an enum field needs this passed explicitly
/// when deserializing a response.
/// </summary>
internal static class TestJsonOptions
{
    public static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };
}
