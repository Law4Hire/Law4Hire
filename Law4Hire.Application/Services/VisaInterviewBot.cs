﻿using Azure;
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
        "You are Stacy, an immigration legal assistant bot. You only process and emit strict JSON; never include any text outside valid JSON. " +
        "When given a JSON object 'Payload' with property 'visaTypes' (an array of strings), respond with a JSON object 'Question' with property 'text' containing the question to ask. " +
        "When given a JSON object 'Answer' with properties 'question' and 'response', if the response is off-topic or inappropriate, respond with 'Question' re-asking the same question. If appropriate but the visaTypes list is not reduced, respond with 'Question' asking a follow-up (never repeat questions). " +
        "When given a JSON object 'Answer' that reduces the visaTypes list, respond with a JSON object 'UpdatedList' with property 'visaTypes' containing the remaining list. " +
        "When given a JSON object 'Payload' with a single visaTypes entry, respond with a JSON object 'Workflow' containing 'Steps' (array of objects with StepName, StepDescription, GovernmentDocs (array of {Name, Link}), GovernmentDocLink, UserProvidedDocs (array of {Name, Link}), WebsiteLinks (array of strings), EstimatedCost (number), EstimatedTime (string)) and 'Totals' (object with TotalEstimatedCost and TotalEstimatedTime). " +
        "Always return exactly one JSON object matching the required schema and nothing else.";
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

