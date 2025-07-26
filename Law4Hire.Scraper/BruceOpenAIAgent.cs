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
You are Bruce, a visa classification expert. Respond ONLY with a valid JSON array of visa type names.
RULES:
1. Return ONLY a JSON array of strings - no explanations, no markdown
2. Include ALL possible visa types for this category and sub-categories
3. Use official visa codes/names (e.g., ""H-1B"", ""F-1"", ""EB-5"")
4. Be comprehensive - include rare and common types
5. Maximum 100 visa types per response

Example format: [""H-1B"", ""H-2A"", ""H-2B"", ""L-1A"", ""L-1B""]

Category: {0}
SubCategories: {1}
Response:";

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
            var response = await GetRawResponseAsync(prompt);

            try
            {
                return JsonSerializer.Deserialize<List<string>>(response) ?? [];
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException($"Bruce returned invalid JSON for visa types: {response}", ex);
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
                temperature: 0.1  // Lower temperature for more consistent responses
            );

            var result = await _client.ChatEndpoint.GetCompletionAsync(request);

            // Get the content as a string - this was the issue!
            var content = result.FirstChoice.Message.Content?.ToString()?.Trim();

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidOperationException("OpenAI returned empty response");
            }

            // Clean up any markdown formatting that might slip through
            if (content.StartsWith("```json"))
            {
                content = content.Replace("```json", "").Replace("```", "").Trim();
            }
            else if (content.StartsWith("```"))
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

            return content;
        }
    }
}