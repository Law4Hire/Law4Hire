using OpenAI;
using OpenAI.Chat;
using OpenAI.Models;
using System.Data;
using System.Text.Json;

namespace Law4Hire.Scraper
{
    public class BruceOpenAIAgent : IBruceOpenAIAgent
    {
        private readonly OpenAIClient _client;

        // STRICT PROMPT RULES FOR BRUCE
        private const string SUBCATEGORIES_PROMPT = @"
You are Bruce, a visa classification expert. Respond ONLY with a valid JSON array of strings.
RULES:
1. Return ONLY a JSON array - no explanations, no markdown, no extra text
2. Each item must be a distinct sub-category name (string)
3. Maximum 25 sub-categories per response
4. Focus on the most common and legally distinct sub-categories
5. Use official terminology when possible

Example format: [""Business Visitors"", ""Medical Treatment"", ""Tourism""]

Category: {0}
Response:";

        private const string VALIDATION_PROMPT = @"
You are Bruce, a visa classification expert. Respond ONLY with this exact JSON format:
{{ ""result"": ""Valid"" }} or {{ ""result"": ""Invalid"" }}

RULES:
1. Return ""Valid"" if the sub-category is legally distinct and adds value
2. Return ""Invalid"" if it's redundant, too broad, or already covered
3. No explanations - only the JSON response

Category: {0}
SubCategory to validate: {1}
Response:";

        private const string VISA_TYPES_PROMPT = @"
Return JSON array of visa codes for category: {0}

Rules:
- Max 25 visa types 
- Official codes only (H-1B, EB-2, F-1)
- Include cross-category types
- Examples: EB-2 fits Work+Immigration, H-1B fits Work+Immigration

SubCategories: {1}
JSON only:";

        public BruceOpenAIAgent()
        {
            var apiKey = Environment.GetEnvironmentVariable("OpenAIKey");
            if (string.IsNullOrEmpty(apiKey))
                throw new InvalidOperationException("Missing environment variable: OpenAIKey");

            _client = new OpenAIClient(new OpenAIAuthentication(apiKey));
        }

        public async Task<List<string>> GetSubCategoriesAsync(string category)
        {
            var prompt = string.Format(SUBCATEGORIES_PROMPT, category);
            var response = await GetRawResponseAsync(prompt);

            try
            {
                return JsonSerializer.Deserialize<List<string>>(response) ?? [];
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException($"Bruce returned invalid JSON for sub-categories: {response}", ex);
            }
        }

        public async Task<bool> ValidateSubCategoryAsync(string category, string subCategory)
        {
            var prompt = string.Format(VALIDATION_PROMPT, category, subCategory);
            var response = await GetRawResponseAsync(prompt);

            try
            {
                using var doc = JsonDocument.Parse(response);
                var result = doc.RootElement.GetProperty("result").GetString();
                return result == "Valid";
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException($"Bruce returned invalid validation JSON: {response}", ex);
            }
        }

        public async Task<List<string>> GetVisaTypesAsync(string category, List<string> subCategories)
        {
            var subCategoriesJson = JsonSerializer.Serialize(subCategories);
            var prompt = string.Format(VISA_TYPES_PROMPT, category, subCategoriesJson);
            
            Console.WriteLine($"[BRUCE] Requesting visa types for category: {category}");
            Console.WriteLine($"[BRUCE] SubCategories: {subCategoriesJson}");
            
            var response = await GetRawResponseAsync(prompt);
            
            Console.WriteLine($"[BRUCE] API Response for {category}: {response}");

            try
            {
                // Check if response is truncated
                if (response.EndsWith("\"") == false && response.Contains("\""))
                {
                    Console.WriteLine($"[BRUCE] WARNING: Response appears truncated, attempting to fix: {response}");
                    // Try to fix truncated JSON by finding the last complete entry
                    var lastCommaIndex = response.LastIndexOf(',');
                    if (lastCommaIndex > 0)
                    {
                        response = response.Substring(0, lastCommaIndex) + "]";
                        Console.WriteLine($"[BRUCE] Fixed response: {response}");
                    }
                }

                var visaTypes = JsonSerializer.Deserialize<List<string>>(response) ?? [];
                Console.WriteLine($"[BRUCE] Parsed {visaTypes.Count} visa types for {category}: {string.Join(", ", visaTypes)}");
                return visaTypes;
            }
            catch (JsonException)
            {
                Console.WriteLine($"[BRUCE] ERROR: Failed to parse JSON response: {response}");
                
                // Fallback: return a basic set of visa types for the category
                var fallbackTypes = GetFallbackVisaTypes(category);
                Console.WriteLine($"[BRUCE] Using fallback visa types for {category}: {string.Join(", ", fallbackTypes)}");
                return fallbackTypes;
            }
        }

        private async Task<string> GetRawResponseAsync(string prompt)
        {
            var request = new ChatRequest(
                new List<Message>
                {
                    new(Role.System, "You are Bruce, a visa expert. You ONLY respond with valid JSON. Never include explanations, markdown, or extra text."),
                    new(Role.User, prompt)
                },
                model: Model.GPT4o,
                temperature: 0.1,  // Lower temperature for more consistent responses
                maxTokens: 500     // Reduced tokens to prevent truncation
            );

            var result = await _client.ChatEndpoint.GetCompletionAsync(request);

            // Get the content as a string - this was the issue!
            var content = result?.FirstChoice?.Message?.Content?.ToString()?.Trim();

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidOperationException("OpenAI returned empty response");
            }

            // Clean up any markdown formatting that might slip through
            if (content?.StartsWith("```json") == true)
            {
                content = content.Replace("```json", "").Replace("```", "").Trim();
            }
            else if (content?.StartsWith("```") == true)
            {
                // Remove any other code block markers
                var firstNewline = content.IndexOf('\n');
                if (firstNewline != -1)
                    content = content.Substring(firstNewline + 1);

                var lastTriple = content.LastIndexOf("```");
                if (lastTriple != -1)
                    content = content.Substring(0, lastTriple);

                content = content.Trim();
            }

            return content ?? throw new InvalidOperationException("OpenAI returned null content after processing");
        }

        private static List<string> GetFallbackVisaTypes(string category)
        {
            return category.ToLower() switch
            {
                "visit" => ["B-1", "B-2", "J-1", "C-1", "D-1", "VWP", "ESTA"],
                "work" => ["H-1B", "L-1A", "L-1B", "O-1", "EB-2", "EB-3", "H-2A", "H-2B"],
                "immigrate" => ["EB-1", "EB-2", "EB-3", "EB-4", "EB-5", "IR-1", "IR-2", "IR-5"],
                "study" => ["F-1", "F-2", "M-1", "M-2", "J-1"],
                "family" => ["IR-1", "IR-2", "IR-5", "F-1", "F-2A", "F-2B", "F-3", "F-4", "K-1"],
                "investment" => ["EB-5", "E-2", "L-1A"],
                "asylum" => ["I-589", "T-1", "U-1", "VAWA"],
                _ => ["H-1B", "F-1", "B-1", "B-2"]
            };
        }
    }
}