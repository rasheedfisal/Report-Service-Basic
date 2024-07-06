using System.Text.Json;

namespace Basic_Report.Extensions;

public static class Response {
    public static IResult Send<T>(T response, int statusCode)
    {
        return Results.Json(response, new JsonSerializerOptions{ WriteIndented = true }, "application/json", statusCode);
    }
}