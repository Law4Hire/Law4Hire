-- Final verification of the wizard data patterns

-- Show a few sample Question1/Answer1 patterns to understand the data structure
SELECT TOP 5
    'Sample Q1/A1 patterns' AS Analysis,
    Country,
    Purpose,
    LEFT(Question1, 150) AS Question1_Full,
    LEFT(Answer1, 50) AS Answer1_Full,
    LEFT(Question2, 100) AS Question2_Sample,
    LEFT(Answer2, 50) AS Answer2_Sample,
    IsCompleteSession,
    HasFollowUp
FROM VisaWizards
WHERE Question1 IS NOT NULL AND Question1 != ''
ORDER BY CreatedAt DESC;

-- Check what happens when we look at the Question1 content more carefully
SELECT 
    'Question1 content analysis' AS Analysis,
    Country,
    Purpose,
    CASE 
        WHEN Question1 LIKE '%passport%' THEN 'Passport question'
        WHEN Question1 LIKE '%travel%' THEN 'Travel question'
        WHEN Question1 LIKE '%country%' THEN 'Country question'
        ELSE 'Other question type'
    END AS QuestionType,
    CASE
        WHEN LEN(Question1) > 200 THEN 'Very long (possible combined text)'
        WHEN LEN(Question1) > 100 THEN 'Long'
        WHEN LEN(Question1) < 50 THEN 'Short'
        ELSE 'Normal'
    END AS QuestionLength,
    LEN(Question1) AS ActualLength
FROM VisaWizards
WHERE Question1 IS NOT NULL AND Question1 != '';