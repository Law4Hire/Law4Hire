Create a new C# console application in my Law4Hire solution called Law4Hire.VisaWorkflowScraper.

Use Entity Framework to access the existing SQL Server database via the Law4HireDbContext. Assume it already includes tables for:
- Countries (Id, Name, ISOCode)
- BaseVisaTypes (Id, VisaTypeCode, Description)

Add a new table called VisaWorkflows with the following columns:
- Id (Guid)
- CountryId (Guid, FK to Countries)
- VisaTypeId (Guid, FK to BaseVisaTypes)
- WorkflowJson (nvarchar(max))
- CreatedAt (datetime)
- UpdatedAt (datetime)
- Hash (string, optional, for change detection)

In Program.cs:
1. Inject the DbContext.
2. Use environment variable "OpenAIKey" to create an HttpClient for OpenAI.
3. Loop through every country and visa combination.
4. For each, send a prompt to OpenAI using the Chat Completions API asking for the structured workflow in JSON for the given visa type and country.

Use this system message:
"You are a U.S. immigration expert. Given a visa type and a country of citizenship, return a step-by-step workflow in JSON. Include name, description, government forms, estimated cost (or null), URLs, required documents, medical requirements, and any consulate-specific instructions. Do not guess. If the information is unavailable, say so clearly in the response and mark the status as 'incomplete'."

Use this user message format:
{
  "visa_type": "<VisaTypeCode>",
  "country": "<CountryName>",
  "citizenship_country_code": "<ISOCode>",
  "format": "json"
}

5. Parse the JSON response. If status is "complete", store it in the VisaWorkflows table with timestamp and country/visa keys. Skip if already exists.
6. Add basic logging to show progress (country name, visa type, success/failure).

Ensure the app runs from command line and is idempotent — don’t reprocess existing records. Add retry logic and basic exponential backoff if OpenAI call fails.

Add EF migration and update the database automatically on run.
