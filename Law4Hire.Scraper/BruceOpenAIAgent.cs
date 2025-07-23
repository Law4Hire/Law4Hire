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

        public BruceOpenAIAgent()
        {
            var apiKey = Environment.GetEnvironmentVariable("OpenAIKey");
            if (string.IsNullOrEmpty(apiKey))
                throw new InvalidOperationException("Missing environment variable: OpenAIKey");

            _client = new OpenAIClient(new OpenAIAuthentication(apiKey));
        }

        public async Task<List<string>> GetSubCategoriesAsync(string category)
        {
            var prompt = $"You are Bruce. Respond only in strict single-line JSON arrays.\n" +
                         $"Function: GetSubCategories\n" +
                         $"Category: {category}\n" +
                         $"Return an array of the most common and specific sub-categories for this visa category. No commentary.";

            return await GetJsonArrayAsync(prompt);
        }

        public async Task<bool> ValidateSubCategoryAsync(string category, string subCategory)
        {
            var prompt = $"You are Bruce. Respond only in this format: {{ \"result\": \"Valid\" }} or {{ \"result\": \"Invalid\" }}.\n" +
                         $"Function: ValidateSubCategory\n" +
                         $"Category: {category}\n" +
                         $"SubCategory: {subCategory}\n" +
                         $"Return Valid if the sub-category is new and distinct. Return Invalid if it is just a rewording or already exists.";

            var response = await GetRawResponseAsync(prompt);
            var doc = JsonDocument.Parse(response);
            var result = doc.RootElement.GetProperty("result").GetString();
            return result == "Valid";
        }

        public async Task<List<string>> GetVisaTypesAsync(string category, List<string> subCategories)
        {
            var prompt = $"You are Bruce. Respond only in a strict single-line JSON array of visa types.\n" +
                         $"Function: GetVisaTypes\n" +
                         $"Category: {category}\n" +
                         $"SubCategories: {JsonSerializer.Serialize(subCategories)}\n" +
                         $"Return a comprehensive list of visa types. No commentary.";

            return await GetJsonArrayAsync(prompt);
        }

        private async Task<List<string>> GetJsonArrayAsync(string prompt)
        {
            var response = await GetRawResponseAsync(prompt);
            try
            {
                return JsonSerializer.Deserialize<List<string>>(response) ?? [];
            }
            catch
            {
                throw new InvalidDataException("Bruce did not return a valid JSON array.");
            }
        }

        private async Task<string> GetRawResponseAsync(string prompt)
        {
            var request = new ChatRequest(
                new List<Message>
                {
                    new(Role.System, "You are Bruce, an AI that always responds with clean JSON only."),
                    new(Role.User, prompt)
                },
                model: Model.GPT4o,
                temperature: 0.2
            );

            var result = await _client.ChatEndpoint.GetCompletionAsync(request);
            return result.FirstChoice.Message.Content.Trim();
        }
    }
}
