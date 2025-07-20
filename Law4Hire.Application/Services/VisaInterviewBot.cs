using Azure;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Law4Hire.Application.Services;

public class VisaInterviewBot
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    private const string SystemPrompt =
    "You are Stacy, an immigration legal assistant bot. Your job is to return a valid single-line JSON response only. " +
    "Never include markdown, comments, formatting, explanations, or text outside the JSON. " +
    "Rules: " +
    "1. When given a visa category (e.g., \\\"Tourism\\\"), respond with a comprehensive list of all of the U.S. visa types related to that category from outside sources such as the US State Department, the USCIS and the DHS or Department of Labor.. " +
    "This must include common, uncommon, electronic waivers, and special-case visas. " +
    "When determining visa types, reference the full list of valid U.S. visas published by USCIS, State Department, or DHS where relevant." +
    "2. Do not assume user location or country of origin. Return every possible applicable visa for any individual considering that category. " +
    "3. Respond only with a single-line JSON array, such as [\\\"B-2\\\", \\\"ESTA\\\", \\\"WT\\\", \\\"C-1/D\\\"]. " +
    "4. Do not filter or prioritize unless explicitly asked. " +
    "5. If given a JSON array of visa types, return a single-line JSON string with one question that helps eliminate at least one type. " +
    "6. If given an answer and a JSON object with 'visaOptions', return a single-line JSON array with remaining options. " +
    "7. If given a single visa type, return a single-line JSON object containing: " +
    "steps: array of steps, each with name (string), description (string), documents (string[]), estimatedCost (decimal), estimatedTimeDays (int); " +
    "and also include estimatedTotalCost (decimal) and estimatedTotalTimeDays (int). " +
    "Never explain your actions. Never include a greeting. Return valid JSON only on a single line.";


    public VisaInterviewBot(HttpClient httpClient, string apiKey)
    {
        _httpClient = httpClient;
        _apiKey = apiKey;
    }

    public async Task<string> ProcessAsync(string inputJson)
    {
        var requestBody = new
        {
            model = "gpt-4o",
            messages = new object[]
            {
                new { role = "system", content = SystemPrompt },
                new { role = "user", content = inputJson }
            },
            temperature = 0.2
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        string rawJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(rawJson);
        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrWhiteSpace(content))
            return "{}";

        // Sanitize triple-backtick code blocks
        content = content.Trim();
        if (content.StartsWith("```"))
        {
            int firstNewline = content.IndexOf('\n');
            if (firstNewline != -1)
                content = content.Substring(firstNewline + 1);

            int lastTriple = content.LastIndexOf("```");
            if (lastTriple != -1)
                content = content.Substring(0, lastTriple);
        }

        return content.Trim();
    }
}


public async Task FinalizeInterviewAsync(Guid userId, string finalVisa, WorkflowResult workflow)
{
    var user = await _context.Users.Include(u => u.VisaInterview).FirstOrDefaultAsync(u => u.Id == userId);
    if (user == null) throw new Exception("User not found");

    user.VisaType = finalVisa;
    user.WorkflowJson = JsonSerializer.Serialize(workflow);
    user.VisaInterview.IsCompleted = true;

    await _context.SaveChangesAsync();
}

public async Task ResetInterviewAsync(Guid userId)
{
    var user = await _context.Users.Include(u => u.VisaInterview).FirstOrDefaultAsync(u => u.Id == userId);
    if (user == null) throw new Exception("User not found");

    if (user.VisaInterview != null)
    {
        user.VisaInterview.IsReset = true;
        user.VisaInterview.IsCompleted = false;
        user.VisaInterview.CurrentStep = 0;
        user.WorkflowJson = null;
        user.Category = null;
        user.VisaType = null;
    }

    await _context.SaveChangesAsync();
}
