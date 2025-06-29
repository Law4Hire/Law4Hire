using Law4Hire.Core.Enums;

namespace Law4Hire.Core.DTOs;

public record IntakeSessionDto(
    Guid Id,
    Guid UserId,
    IntakeStatus Status,
    DateTime StartedAt,
    DateTime? CompletedAt,
    string Language
);

public record CreateIntakeSessionDto(
    Guid UserId,
    string Language
);

public record IntakeQuestionDto(
    int Id,
    string QuestionKey,
    string QuestionText,
    QuestionType Type,
    int Order,
    string? Conditions,
    bool IsRequired,
    string? ValidationRules
);