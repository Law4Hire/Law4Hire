namespace Law4Hire.Core.DTOs
{
    public class Phase2QuestionDto
    {
        public string Question { get; set; } = string.Empty;
        public int Step { get; set; }
        public bool IsComplete { get; set; }
        public List<QuestionOptionDto> Options { get; set; } = new();
    }

    public class QuestionOptionDto
    {
        public string Key { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }

    public class Phase2StepDto
    {
        public Guid UserId { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public string? Answer { get; set; }
    }
}