-- Detailed analysis to understand the wizard failure patterns

-- Check actual content of Question1/Answer1 for populated records
SELECT TOP 5
    'Sample Questions and Answers' AS Analysis,
    Country,
    Purpose,
    Question1,
    Answer1,
    Question2,
    Answer2,
    HasFollowUp,
    IsCompleteSession
FROM VisaWizards
WHERE Question1 IS NOT NULL AND Question1 != ''
ORDER BY CreatedAt DESC;

-- Check records that have follow-up set but don't have Question2
SELECT 
    'Follow-up Logic Issues' AS Analysis,
    Country,
    Purpose,
    HasFollowUp,
    CASE WHEN Question2 IS NULL OR Question2 = '' THEN 'Missing Q2' ELSE 'Has Q2' END AS Question2Status,
    CASE WHEN Answer2 IS NULL OR Answer2 = '' THEN 'Missing A2' ELSE 'Has A2' END AS Answer2Status,
    CreatedAt
FROM VisaWizards
WHERE HasFollowUp = 1;

-- Check the records marked as complete but missing outcome content
SELECT 
    'Complete But Missing Outcomes' AS Analysis,
    Country,
    Purpose,
    IsCompleteSession,
    CASE WHEN OutcomeDisplayContent IS NULL OR OutcomeDisplayContent = '' THEN 'Missing Outcome' ELSE 'Has Outcome' END AS OutcomeStatus,
    CASE WHEN VisaRecommendations IS NULL OR VisaRecommendations = '' THEN 'Missing Recs' ELSE 'Has Recs' END AS RecommendationStatus,
    CreatedAt
FROM VisaWizards
WHERE IsCompleteSession = 1;

-- Get session timing details
SELECT 
    'Session Timing Analysis' AS Analysis,
    SessionId,
    Country,
    Purpose,
    CreatedAt,
    DATEDIFF(second, LAG(CreatedAt) OVER (ORDER BY CreatedAt), CreatedAt) AS SecondsSinceLastRecord
FROM VisaWizards
ORDER BY CreatedAt;

-- Check for any error messages or unusual content in the text fields
SELECT 
    'Content Analysis' AS Analysis,
    Country,
    Purpose,
    CASE 
        WHEN Question1 LIKE '%error%' OR Question1 LIKE '%fail%' OR Question1 LIKE '%exception%' THEN 'Potential Error in Q1'
        WHEN Answer1 LIKE '%error%' OR Answer1 LIKE '%fail%' OR Answer1 LIKE '%exception%' THEN 'Potential Error in A1'
        ELSE 'Normal Content'
    END AS ContentType,
    LEFT(Question1, 200) AS Question1_Snippet,
    LEFT(Answer1, 100) AS Answer1_Snippet
FROM VisaWizards
WHERE Question1 IS NOT NULL AND Question1 != '';