using Azure;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.Extensions.Configuration;

namespace Law4Hire.Application.Services;

public class VisaInterviewBot
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly Law4HireDbContext _context;

    private const string SystemPrompt =
        "You are Stacy, an immigration legal assistant bot. Your job is to return a valid single-line JSON response only. " +
        "Never include markdown, comments, formatting, explanations, or text outside the JSON. " +
        "CRITICAL RESPONSE FORMATS: " +
        "1. When given an object with 'category' property, respond with ONLY a JSON array of visa types. " +
        "2. When given a JSON array of visa types, respond with ONLY a JSON string containing a question. " +
        "3. When given an object with 'visaTypes' and 'answer', respond with ONLY a JSON array of filtered visa types. " +
        "4. When given a single visa type string, respond with a complete workflow JSON object in this EXACT format: " +
        "{\\\"steps\\\":[" +
        "{\\\"name\\\":\\\"Complete DS-160 Form\\\",\\\"description\\\":\\\"Fill out the online nonimmigrant visa application form\\\",\\\"documents\\\":[{\\\"name\\\":\\\"DS-160 Form\\\",\\\"isGovernmentProvided\\\":false,\\\"downloadLink\\\":null,\\\"isRequired\\\":true},{\\\"name\\\":\\\"Passport Photo\\\",\\\"isGovernmentProvided\\\":false,\\\"downloadLink\\\":null,\\\"isRequired\\\":true}],\\\"estimatedCost\\\":0.00,\\\"estimatedTimeDays\\\":1,\\\"websiteLink\\\":\\\"https://ceac.state.gov/genniv/\\\"}," +
        "{\\\"name\\\":\\\"Pay Visa Fee\\\",\\\"description\\\":\\\"Pay the non-refundable visa application fee\\\",\\\"documents\\\":[{\\\"name\\\":\\\"Payment Receipt\\\",\\\"isGovernmentProvided\\\":true,\\\"downloadLink\\\":\\\"https://www.ustraveldocs.com/\\\",\\\"isRequired\\\":true}],\\\"estimatedCost\\\":185.00,\\\"estimatedTimeDays\\\":1,\\\"websiteLink\\\":\\\"https://www.ustraveldocs.com/\\\"}]," +
        "\\\"estimatedTotalCost\\\":185.00,\\\"estimatedTotalTimeDays\\\":15} " +
        "Each step MUST include: name, description, documents array (with name, isGovernmentProvided, downloadLink, isRequired), estimatedCost, estimatedTimeDays, websiteLink (if applicable). " +
        "Documents array items must have: name (string), isGovernmentProvided (boolean), downloadLink (string or null), isRequired (boolean). " +
        "Include realistic government links for forms and fee payments. " +
        "NEVER return text descriptions - only the structured JSON workflow object.";
    public VisaInterviewBot(HttpClient httpClient, string apiKey, IConfiguration _configuration, Law4HireDbContext context)
    {
        _httpClient = httpClient;
        _context = context; 
        _apiKey = Environment.GetEnvironmentVariable("OpenAIKey")
                 ?? _configuration["OpenAI:ApiKey"]
                 ?? throw new InvalidOperationException("OpenAI API key not found");
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

    public async Task FinalizeInterviewAsync(Guid userId, string finalVisa, WorkflowResult workflow)
    {
        var user = await _context.Users.Include(u => u.VisaInterview).FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) throw new Exception("User not found");

        user.VisaType = finalVisa;
        user.WorkflowJson = JsonSerializer.Serialize(workflow);
        if (user.VisaInterview != null)
        {
            user.VisaInterview.IsCompleted = true;
        }

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
}

