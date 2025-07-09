using Law4Hire.Core.Enums;
using System.Collections.Generic;

namespace Law4Hire.Core.DTOs;

public record IntakeSessionDto(
    Guid Id,
    Guid UserId,
    IntakeStatus Status,
    DateTime StartedAt,
    DateTime? CompletedAt,
    string Language,
    string? SessionData
);

public record CreateIntakeSessionDto(
    Guid UserId,
    string Language
);

public record IntakeQuestionDto(
    int Id,
    string Category,
    string QuestionKey,
    string QuestionText,
    QuestionType Type,
    int Order,
    string? Conditions,
    bool IsRequired,
    string? ValidationRules
);

public record UpdateSessionProgressDto(
    int CurrentStep,
    Dictionary<string, string> Answers);