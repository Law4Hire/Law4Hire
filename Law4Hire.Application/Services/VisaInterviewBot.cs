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
        "You are Stacy, a Legal Assistant Bot specializing in Immigration, Asylum, and Refugee visa processes. Your ONLY job is to return valid JSON responses - NEVER return plain text or explanations. " +
        "INPUT FORMATS: " +
        "1. Payload: {\"VisaTypes\":[array], \"Language\":\"code\"} - Respond with {\"Question\":\"question text\"} " +
        "2. Answer: {\"Answer\":\"user response\", \"VisaTypes\":[current array]} - User responding to your question. Filter the VisaTypes based on their answer and return {\"Payload\":[filtered array]} or {\"Question\":\"follow-up question\"} " +
        "3. Single visa string (e.g. \"H-1B\") - Respond with {\"Workflow\":{\"Steps\":[...],\"Totals\":{...}}} " +
        "OUTPUT FORMATS (ALWAYS VALID JSON): " +
        "- {\"Payload\":[\"visa1\",\"visa2\"]} - Filtered visa list " +
        "- {\"Question\":\"question text\"} - Next question to narrow choices " +
        "- {\"Workflow\":{\"Steps\":[{\"StepName\":\"\",\"StepDescription\":\"\",\"GovernmentDocument\":\"\",\"GovernmentDocumentLink\":\"\",\"UserGeneratedDocuments\":\"\",\"WebsiteLink\":\"\",\"EstimatedTime\":0,\"EstimatedCost\":0}],\"Totals\":{\"TotalEstimatedTime\":0,\"TotalEstimatedCost\":0}}} " +
        "CRITICAL RULES: " +
        "1. ALWAYS return valid JSON - NEVER plain text " +
        "2. When you receive an Answer object, the user is responding to your previous question about narrowing down visa types " +
        "3. Use the answer to filter the visa list or ask follow-up questions " +
        "4. Ask specific, targeted questions that help eliminate visa options " +
        "5. Focus on family-based visas when user mentions family relationships " +
        "6. Return JSON only - no explanations, no apologies, no requests for more information";
    public VisaInterviewBot(HttpClient httpClient, string apiKey, IConfiguration _configuration, Law4HireDbContext context)
    {
        _httpClient = httpClient;
        _context = context; 
        _apiKey = "not-needed"; // Static implementation - no OpenAI API needed
    }

    public async Task<string> ProcessAsync(string inputJson)
    {
        // TESTING MODE: Use mock responses instead of OpenAI API
        // This prevents API key issues during development and testing
        Console.WriteLine($"[DEBUG] VisaInterviewBot input: {inputJson}");
        
        try
        {
            // Parse the input to determine what type of response to provide
            using var doc = JsonDocument.Parse(inputJson);
            
            // Case 1: Payload object with VisaTypes - return a question
            if (doc.RootElement.TryGetProperty("VisaTypes", out var visaTypesElement))
            {
                var visaTypes = JsonSerializer.Deserialize<List<string>>(visaTypesElement.GetRawText());
                var language = doc.RootElement.TryGetProperty("Language", out var langElement) ? langElement.GetString() : "en-US";
                var category = doc.RootElement.TryGetProperty("Category", out var catElement) ? catElement.GetString() : "";
                
                Console.WriteLine($"[DEBUG] Processing visa types for {category}: {visaTypes?.Count ?? 0} options");
                
                // Generate a category-appropriate question with separate options
                var (questionText, options) = GenerateQuestionForCategory(category, visaTypes?.Count ?? 0);
                
                var response = new {
                    Question = questionText,
                    Options = options.Select(o => new { Key = o.key, Text = o.text }).ToArray()
                };
                
                var jsonResponse = System.Text.Json.JsonSerializer.Serialize(response);
                Console.WriteLine($"[DEBUG] Returning question with options: {jsonResponse}");
                return jsonResponse;
            }
            
            // Case 2: Answer object with user response - return filtered payload
            if (doc.RootElement.TryGetProperty("Answer", out var answerElement))
            {
                var answer = answerElement.GetString();
                var currentVisaTypes = doc.RootElement.TryGetProperty("VisaTypes", out var currentTypes) 
                    ? JsonSerializer.Deserialize<List<string>>(currentTypes.GetRawText()) 
                    : new List<string>();
                
                Console.WriteLine($"[DEBUG] Processing answer '{answer}' for {currentVisaTypes?.Count ?? 0} visa types");
                
                // Simple filtering logic based on answer
                var filteredTypes = FilterVisaTypesByAnswer(answer, currentVisaTypes);
                Console.WriteLine($"[DEBUG] FilterVisaTypesByAnswer returned {filteredTypes.Count} results: {string.Join(", ", filteredTypes)}");
                
                // ALWAYS return a Payload for any valid filtering result
                if (filteredTypes.Count >= 1)
                {
                    var response = $"{{\"Payload\":{JsonSerializer.Serialize(filteredTypes)}}}";
                    Console.WriteLine($"[DEBUG] Returning filtered payload with {filteredTypes.Count} visas: {response}");
                    return response;
                }
                else
                {
                    // No matching visas - return a subset to keep progressing
                    var fallbackTypes = currentVisaTypes.Take(Math.Max(1, currentVisaTypes.Count / 2)).ToList();
                    var response = $"{{\"Payload\":{JsonSerializer.Serialize(fallbackTypes)}}}";
                    Console.WriteLine($"[DEBUG] No matches, returning fallback payload with {fallbackTypes.Count} visas: {response}");
                    return response;
                }
            }
            
            // Case 3: Single visa string - return workflow
            var singleVisaInput = inputJson.Trim('"');
            if (IsValidVisaCode(singleVisaInput))
            {
                Console.WriteLine($"[DEBUG] Generating workflow for visa: {singleVisaInput}");
                var workflow = GenerateWorkflowForVisa(singleVisaInput);
                var response = $"{{\"Workflow\":{workflow}}}";
                Console.WriteLine($"[DEBUG] Returning workflow: {response}");
                return response;
            }
            
            // Default fallback
            Console.WriteLine($"[DEBUG] Unrecognized input format, returning default question");
            return "{\"Question\":\"Tell me more about your immigration goals so I can help you find the right visa.\"}";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] VisaInterviewBot error: {ex.Message}");
            return "{\"Question\":\"I'm having trouble processing your request. Could you please try again?\"}";
        }
    }
    
    private (string question, List<(string key, string text)> options) GenerateQuestionForCategory(string category, int visaCount)
    {
        return category?.ToLower() switch
        {
            "immigrate" => (
                "What is the basis for your immigration to the United States?",
                new List<(string, string)> {
                    ("A", "Family relationships (spouse, parent, child)"),
                    ("B", "Employment opportunities (job offer, special skills)"),
                    ("C", "Investment in US business")
                }
            ),
            "work" => (
                "What best describes your work situation?",
                new List<(string, string)> {
                    ("A", "I have a specific job offer from a US employer"),
                    ("B", "I have special skills or talents"),
                    ("C", "I want to transfer from an international company")
                }
            ),
            "study" => (
                "What type of education are you pursuing?",
                new List<(string, string)> {
                    ("A", "University degree program (bachelor's/master's)"),
                    ("B", "Vocational or technical school"),
                    ("C", "Exchange program")
                }
            ),
            "family" => (
                "What is your relationship status?",
                new List<(string, string)> {
                    ("A", "Married to a US citizen"),
                    ("B", "Engaged to a US citizen"),
                    ("C", "Related to a US citizen/permanent resident (parent, child, sibling)")
                }
            ),
            "visit" => (
                "What is the purpose of your visit?",
                new List<(string, string)> {
                    ("A", "Tourism and leisure activities"),
                    ("B", "Business meetings or conferences"),
                    ("C", "Visiting family and friends")
                }
            ),
            "investment" => (
                "What type of business investment are you considering?",
                new List<(string, string)> {
                    ("A", "Starting a new business with significant investment ($500K+)"),
                    ("B", "Purchasing an existing business"),
                    ("C", "Working for a company I'm investing in")
                }
            ),
            "asylum" => (
                "What type of protection are you seeking?",
                new List<(string, string)> {
                    ("A", "Protection from political persecution"),
                    ("B", "Protection from religious persecution"),
                    ("C", "Protection from other forms of persecution")
                }
            ),
            _ => (
                $"I found {visaCount} visa options for you. Which category best describes your situation?",
                new List<(string, string)> {
                    ("A", "Work-related purposes"),
                    ("B", "Family-related purposes"),
                    ("C", "Other reasons")
                }
            )
        };
    }
    
    private List<string> FilterVisaTypesByAnswer(string answer, List<string> currentTypes)
    {
        if (currentTypes == null || !currentTypes.Any()) return new List<string>();
        
        var lowerAnswer = answer?.ToLower() ?? "";
        var filtered = new List<string>();
        
        Console.WriteLine($"[DEBUG] Filtering {currentTypes.Count} visa types with answer: '{answer}'");
        
        // Handle multiple choice answers (A, B, C)
        if (lowerAnswer == "a" || lowerAnswer.Contains("option a"))
        {
            Console.WriteLine("[DEBUG] User selected option A");
            // Filter based on first option - typically family/employment/primary category
            filtered = FilterByCategoryOption(currentTypes, "A");
        }
        else if (lowerAnswer == "b" || lowerAnswer.Contains("option b"))
        {
            Console.WriteLine("[DEBUG] User selected option B");
            // Filter based on second option - typically skills/special cases
            filtered = FilterByCategoryOption(currentTypes, "B");
        }
        else if (lowerAnswer == "c" || lowerAnswer.Contains("option c"))
        {
            Console.WriteLine("[DEBUG] User selected option C");
            // Filter based on third option - typically other/investment
            filtered = FilterByCategoryOption(currentTypes, "C");
        }
        // Handle simple yes/no answers by categorizing the current visa types
        else if (lowerAnswer == "yes" || lowerAnswer == "y" || lowerAnswer == "true")
        {
            // If user says yes, we need to determine what they're saying yes to
            Console.WriteLine("[DEBUG] User answered 'yes' - using smart filtering based on visa types");
            
            // If we have family visas in the current list, keep them
            var familyVisas = currentTypes.Where(v => 
                v.Contains("K-") || v.Contains("CR-") || v.Contains("IR-") || 
                v.Contains("F-") || v.ToLower().Contains("family")).ToList();
            
            // If we have work visas in the current list, keep them  
            var workVisas = currentTypes.Where(v => 
                v.Contains("H-") || v.Contains("L-") || v.Contains("O-") || 
                v.Contains("P-") || v.Contains("EB-") || v.Contains("TN")).ToList();
            
            // Return the most common category or a reasonable subset
            if (familyVisas.Any() && familyVisas.Count <= workVisas.Count)
            {
                filtered = familyVisas;
                Console.WriteLine($"[DEBUG] Filtered to {filtered.Count} family visas");
            }
            else if (workVisas.Any())
            {
                filtered = workVisas;
                Console.WriteLine($"[DEBUG] Filtered to {filtered.Count} work visas");
            }
            else
            {
                // Take a reasonable subset
                filtered = currentTypes.Take(Math.Min(3, currentTypes.Count)).ToList();
                Console.WriteLine($"[DEBUG] No clear category, taking first {filtered.Count} visas");
            }
        }
        else if (lowerAnswer == "no" || lowerAnswer == "n" || lowerAnswer == "false")
        {
            // If user says no, we need more information
            Console.WriteLine("[DEBUG] User answered 'no' - asking for clarification");
            return new List<string>(); // This will trigger a clarifying question
        }
        else
        {
            // Handle specific keyword answers
            foreach (var visa in currentTypes)
            {
                var visaLower = visa.ToLower();
                
                // Family-based filtering
                if (lowerAnswer.Contains("family") || lowerAnswer.Contains("married") || lowerAnswer.Contains("spouse"))
                {
                    if (visaLower.Contains("k-") || visaLower.Contains("cr-") || visaLower.Contains("ir-") || 
                        visaLower.Contains("f-") || visaLower.Contains("family"))
                        filtered.Add(visa);
                }
                // Employment-based filtering
                else if (lowerAnswer.Contains("work") || lowerAnswer.Contains("job") || lowerAnswer.Contains("employ"))
                {
                    if (visaLower.Contains("h-") || visaLower.Contains("l-") || visaLower.Contains("o-") || 
                        visaLower.Contains("p-") || visaLower.Contains("eb-") || visaLower.Contains("tn"))
                        filtered.Add(visa);
                }
                // Investment-based filtering
                else if (lowerAnswer.Contains("invest") || lowerAnswer.Contains("business"))
                {
                    if (visaLower.Contains("eb-5") || visaLower.Contains("e-2") || visaLower.Contains("invest"))
                        filtered.Add(visa);
                }
                // Study-based filtering
                else if (lowerAnswer.Contains("study") || lowerAnswer.Contains("school") || lowerAnswer.Contains("university"))
                {
                    if (visaLower.Contains("f-1") || visaLower.Contains("m-1") || visaLower.Contains("j-1"))
                        filtered.Add(visa);
                }
                // Visit-based filtering
                else if (lowerAnswer.Contains("visit") || lowerAnswer.Contains("tourism") || lowerAnswer.Contains("business trip"))
                {
                    if (visaLower.Contains("b-") || visaLower.Contains("esta"))
                        filtered.Add(visa);
                }
            }
            
            // If no specific matches with keywords, try to narrow down intelligently
            if (!filtered.Any())
            {
                Console.WriteLine("[DEBUG] No keyword matches, using intelligent narrowing");
                filtered = currentTypes.Take(Math.Max(1, Math.Min(3, currentTypes.Count))).ToList();
            }
        }
        
        Console.WriteLine($"[DEBUG] Filtered from {currentTypes.Count} to {filtered.Count} visa types: {string.Join(", ", filtered)}");
        return filtered;
    }
    
    private List<string> FilterByCategoryOption(List<string> currentTypes, string option)
    {
        var filtered = new List<string>();
        
        // Analyze the current visa types to determine what category they fall into
        var familyVisas = currentTypes.Where(v => 
            v.Contains("K-") || v.Contains("CR-") || v.Contains("IR-") || 
            v.Contains("F-") || v.ToLower().Contains("family")).ToList();
            
        var workVisas = currentTypes.Where(v => 
            v.Contains("H-") || v.Contains("L-") || v.Contains("O-") || 
            v.Contains("P-") || v.Contains("EB-") || v.Contains("TN")).ToList();
            
        var investmentVisas = currentTypes.Where(v => 
            v.Contains("EB-5") || v.Contains("E-2")).ToList();
            
        var studyVisas = currentTypes.Where(v => 
            v.Contains("F-1") || v.Contains("M-1") || v.Contains("J-1")).ToList();
            
        var visitVisas = currentTypes.Where(v => 
            v.Contains("B-") || v.Contains("ESTA")).ToList();
        
        // Map options based on the context
        switch (option)
        {
            case "A":
                // First option is usually primary category (family, employment, etc.)
                if (familyVisas.Any())
                    filtered = familyVisas;
                else if (workVisas.Any())
                    filtered = workVisas.Take(Math.Min(3, workVisas.Count)).ToList();
                else
                    filtered = currentTypes.Take(Math.Min(2, currentTypes.Count)).ToList();
                break;
                
            case "B":
                // Second option is usually secondary/specialized category
                if (workVisas.Any())
                    filtered = workVisas.Where(v => v.Contains("H-1B") || v.Contains("L-1") || v.Contains("O-1")).ToList();
                else if (studyVisas.Any())
                    filtered = studyVisas;
                else
                    filtered = currentTypes.Skip(1).Take(Math.Min(2, currentTypes.Count - 1)).ToList();
                break;
                
            case "C":
                // Third option is usually other/investment/special cases
                if (investmentVisas.Any())
                    filtered = investmentVisas;
                else if (visitVisas.Any())
                    filtered = visitVisas;
                else
                    filtered = currentTypes.Skip(2).Take(Math.Min(2, Math.Max(1, currentTypes.Count - 2))).ToList();
                break;
        }
        
        // Ensure we always return at least one visa if the input list wasn't empty
        if (!filtered.Any() && currentTypes.Any())
        {
            filtered.Add(currentTypes.First());
        }
        
        Console.WriteLine($"[DEBUG] FilterByCategoryOption({option}) returned {filtered.Count} visas: {string.Join(", ", filtered)}");
        return filtered;
    }
    
    private bool IsValidVisaCode(string input)
    {
        return !string.IsNullOrEmpty(input) && 
               (input.Contains("-") || input.Length <= 10) && 
               !input.Contains("{") && !input.Contains("[");
    }
    
    private string GenerateWorkflowForVisa(string visaCode)
    {
        // Generate a simple workflow structure for testing
        return $@"{{
            ""Steps"": [
                {{
                    ""StepName"": ""Prepare Documentation"",
                    ""StepDescription"": ""Gather required documents for {visaCode} visa application"",
                    ""GovernmentDocument"": ""Form I-130"",
                    ""GovernmentDocumentLink"": ""https://www.uscis.gov/i-130"",
                    ""UserGeneratedDocuments"": ""Personal statement, financial records"",
                    ""WebsiteLink"": ""https://www.uscis.gov"",
                    ""EstimatedTime"": 30,
                    ""EstimatedCost"": 535
                }},
                {{
                    ""StepName"": ""Submit Application"",
                    ""StepDescription"": ""File {visaCode} petition with USCIS"",
                    ""GovernmentDocument"": ""Supporting Documents"",
                    ""GovernmentDocumentLink"": ""https://www.uscis.gov"",
                    ""UserGeneratedDocuments"": ""Cover letter"",
                    ""WebsiteLink"": ""https://www.uscis.gov"",
                    ""EstimatedTime"": 7,
                    ""EstimatedCost"": 0
                }}
            ],
            ""Totals"": {{
                ""TotalEstimatedTime"": 37,
                ""TotalEstimatedCost"": 535
            }}
        }}";
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

