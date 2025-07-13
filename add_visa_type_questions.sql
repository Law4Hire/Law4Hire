CREATE TABLE IF NOT EXISTS VisaTypeQuestions (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    VisaTypeId UNIQUEIDENTIFIER NOT NULL,
    QuestionKey NVARCHAR(100) NOT NULL,
    QuestionText NVARCHAR(500) NOT NULL,
    Type INT NOT NULL,
    [Order] INT NOT NULL,
    IsRequired BIT NOT NULL,
    ValidationRules NVARCHAR(1000) NULL,
    CONSTRAINT FK_VisaTypeQuestions_VisaTypes FOREIGN KEY (VisaTypeId) REFERENCES VisaTypes(Id)
);

-- Sample question for B1/B2 visitor visa
INSERT INTO VisaTypeQuestions (Id, VisaTypeId, QuestionKey, QuestionText, Type, [Order], IsRequired, ValidationRules)
VALUES ('11111111-2222-3333-4444-555555555555', '1aa2bf5e-7242-49c2-9303-04ddef44e8b1', 'has_us_sponsor', 'Do you have a U.S. sponsor?', 3, 1, 1, '{"required": true, "options": ["Yes","No"]}');
