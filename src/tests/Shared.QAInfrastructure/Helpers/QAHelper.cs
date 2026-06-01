namespace Shared.QAInfrastructure.Helpers;

public static class QAHelper
{
    public static async Task<JsonDocument> ParseAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(json);
    }

    public static async Task AssertOkAsync(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            because: await response.Content.ReadAsStringAsync());
    }

    public static async Task Assert400Async(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            because: await response.Content.ReadAsStringAsync());
    }

    public static async Task Assert401Async(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            because: await response.Content.ReadAsStringAsync());
    }

    public static async Task Assert404Async(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.NotFound,
            because: await response.Content.ReadAsStringAsync());
    }

    public static async Task<int> GetCreatedIdAsync(HttpResponseMessage response)
    {
        var doc = await ParseAsync(response);
        var id  = doc.RootElement.CreatedId();
        id.Should().BeGreaterThan(0, "created ID must be > 0");
        return id;
    }

    public static async Task AssertBodyContainsAsync(HttpResponseMessage response, string keyword)
    {
        var body = await response.Content.ReadAsStringAsync();
        body.ToLower().Should().Contain(keyword.ToLower(),
            because: $"response body should contain '{keyword}'");
    }

    public static string LongString(int length) => new('A', length);
}

/// <summary>
/// Robust extraction of the newly-created entity id from a create response's `data` element.
/// Create endpoints across modules are NOT uniform — `data` may be:
///   • a bare integer id (handlers returning ApiResponseDTO&lt;int&gt;), or
///   • an object carrying the id under "id" (e.g. CountryDto), or
///   • an object whose primary key is named "{Entity}Id" (e.g. UserRoleDto.UserRoleId).
/// This centralises that handling so every test's id-capture tolerates all three shapes.
/// </summary>
public static class QAJsonExtensions
{
    public static int CreatedId(this JsonDocument doc) => doc.RootElement.CreatedId();

    public static int CreatedId(this JsonElement root)
    {
        var data = root.GetProperty("data");

        return data.ValueKind switch
        {
            JsonValueKind.Number => data.GetInt32(),
            JsonValueKind.Object => ExtractIdFromObject(data),
            _ => throw new InvalidOperationException(
                $"Create response 'data' had unexpected kind '{data.ValueKind}'.")
        };
    }

    private static int ExtractIdFromObject(JsonElement data)
    {
        // Prefer an exact "id" (case-insensitive). DTOs declare the primary key first by
        // convention, so otherwise fall back to the first numeric "{Entity}Id" property.
        int? firstEntityId = null;

        foreach (var prop in data.EnumerateObject())
        {
            if (prop.Value.ValueKind != JsonValueKind.Number)
                continue;

            if (prop.Name.Equals("id", StringComparison.OrdinalIgnoreCase))
                return prop.Value.GetInt32();

            if (firstEntityId is null && prop.Name.EndsWith("id", StringComparison.OrdinalIgnoreCase))
                firstEntityId = prop.Value.GetInt32();
        }

        return firstEntityId
            ?? throw new InvalidOperationException(
                "Create response 'data' was an object with no numeric id property.");
    }
}
