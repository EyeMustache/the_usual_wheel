using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TheUsualWheelProject.Models;

namespace TheUsualWheelProject.Services;

public class BreakService : LoggingBase<BreakService>
{
    private readonly HttpClient _httpClient;
    // Note: Gemini API endpoint format
    private const string ApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";

    public BreakService(HttpClient httpClient, ILogger<BreakService> logger) : base(logger)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Break>> GenerateBreaksForMovieAsync(Models.Movie movie)
    {
        Logger.LogInformation($"Generating breaks for {movie.Title} via Gemini API...");

        // The system prompt locks in the exact behavior and schema
        var systemInstructionText = @"
You are a break recommendation engine. Return ONLY valid JSON.

You will receive the movie details. Analyze the movie and choose the best break points for a refreshment pause.

Return a JSON array of objects. Each object must include exactly:
- BreakNumber
- Timestamp
- VibeCheck
- Recap

Timestamp should be an approximate pause time, such as '~1h 05m'.
VibeCheck must be spoiler free, very vague and athmospheric. Do not mention names, plot developments, or specific actions. Use only a brief scene cue like 'after the storm eases'.
Recap should summarize what happened, this has info from up to that point.

Break rules:
1. Choose 2 breaks for a 2h to 2h30m movie unless a third break can still be placed with at least ~40 minutes separation.
2. Breaks must be at least 40 minutes apart.
3. Prefer the first break after the first major act turn and the final break before the last act.
4. Do not include markdown fences, explanation text, analysis, or any extra fields.
";

        // Construct the movie metadata to prompt the user part.
        var userInstructionText = $@"
Movie: {movie.Title} ({movie.Year})
Director: {movie.Director}
Runtime: {movie.DurationInMinutes}m
Synopsis: {movie.Synopsis}
";

        // Gemini REST Payload format with system instructions
        var requestPayload = new
        {
            system_instruction = new
            {
                parts = new[] { new { text = systemInstructionText } }
            },
            contents = new[]
            {
                new { parts = new[] { new { text = userInstructionText } } }
            },
            generationConfig = new 
            { 
                responseMimeType = "application/json",
                temperature = 0.2 // Lower temp minimizes random rule breaking
            }
        };

        var requestUrl = $"{ApiUrl}?key={GeminiApiKey.APIKEY}";
        var response = await _httpClient.PostAsJsonAsync(requestUrl, requestPayload);

        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadFromJsonAsync<JsonDocument>();
        
        var textContent = jsonResponse?.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text").GetString();

        if (string.IsNullOrWhiteSpace(textContent))
            return new List<Break>();

        // Map the JSON directly back into your Break object
        var breaks = JsonSerializer.Deserialize<List<Break>>(textContent, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });

        // Loop over the list to set the foreign key
        if (breaks != null)
        {
            foreach (var b in breaks)
            {
                b.MovieId = movie.Id;
            }
        }

        return breaks ?? new List<Break>();
    }
}