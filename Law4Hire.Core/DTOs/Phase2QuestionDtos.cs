// Add these DTOs to your Law4Hire.Web project (create a DTOs folder if needed)

namespace Law4Hire.Web.DTOs
{
    public class Phase2QuestionDto
    {
        public string Question { get; set; } = string.Empty;
        public int Step { get; set; }
        public bool IsComplete { get; set; }
    }

    public class Phase2StepDto
    {
        public Guid UserId { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public string? Answer { get; set; }
    }
}