-- Comprehensive analysis of VisaWizards database table - SQL Server Compatible
-- Law4Hire Database Analysis Script

-- 1. Total count of records
SELECT 'Total Records' AS Analysis, COUNT(*) AS Count
FROM VisaWizards;

-- 2. Records by country
SELECT 'Records by Country' AS Analysis, Country, COUNT(*) AS Count
FROM VisaWizards
GROUP BY Country
ORDER BY COUNT(*) DESC;

-- 3. Records by purpose (top 10)
SELECT TOP 10 'Records by Purpose (Top 10)' AS Analysis, Purpose, COUNT(*) AS Count
FROM VisaWizards
GROUP BY Purpose
ORDER BY COUNT(*) DESC;

-- 4. Data quality - Question/Answer field population
SELECT 
    'Data Quality Summary' AS Analysis,
    COUNT(*) AS TotalRecords,
    SUM(CASE WHEN Question1 IS NOT NULL AND Question1 != '' THEN 1 ELSE 0 END) AS Question1_Populated,
    SUM(CASE WHEN Answer1 IS NOT NULL AND Answer1 != '' THEN 1 ELSE 0 END) AS Answer1_Populated,
    SUM(CASE WHEN Question2 IS NOT NULL AND Question2 != '' THEN 1 ELSE 0 END) AS Question2_Populated,
    SUM(CASE WHEN Answer2 IS NOT NULL AND Answer2 != '' THEN 1 ELSE 0 END) AS Answer2_Populated,
    SUM(CASE WHEN OutcomeDisplayContent IS NOT NULL AND OutcomeDisplayContent != '' THEN 1 ELSE 0 END) AS OutcomeContent_Populated,
    SUM(CASE WHEN VisaRecommendations IS NOT NULL AND VisaRecommendations != '' THEN 1 ELSE 0 END) AS VisaRecommendations_Populated
FROM VisaWizards;

-- 5. Session completion analysis
SELECT 
    'Session Completion Analysis' AS Analysis,
    COUNT(DISTINCT SessionId) AS TotalSessions,
    SUM(CASE WHEN IsCompleteSession = 1 THEN 1 ELSE 0 END) AS CompletedSessions,
    ROUND(
        (SUM(CASE WHEN IsCompleteSession = 1 THEN 1 ELSE 0 END) * 100.0) / COUNT(DISTINCT SessionId), 
        2
    ) AS CompletionRate_Percent
FROM VisaWizards;

-- 6. Step distribution
SELECT 'Step Distribution' AS Analysis, StepNumber, COUNT(*) AS Count
FROM VisaWizards
GROUP BY StepNumber
ORDER BY StepNumber;

-- 7. Timestamp analysis - when were records created
SELECT 
    'Creation Timeline' AS Analysis,
    CAST(CreatedAt AS DATE) AS CreationDate,
    COUNT(*) AS RecordsCreated,
    MIN(CreatedAt) AS FirstRecord,
    MAX(CreatedAt) AS LastRecord
FROM VisaWizards
GROUP BY CAST(CreatedAt AS DATE)
ORDER BY CreationDate DESC;

-- 8. Recent session analysis (last 24 hours)
SELECT 
    'Recent Sessions (24h)' AS Analysis,
    COUNT(*) AS TotalRecords,
    COUNT(DISTINCT SessionId) AS UniqueSessions,
    AVG(CAST(StepNumber AS FLOAT)) AS AvgStepNumber,
    MAX(StepNumber) AS MaxStepNumber
FROM VisaWizards
WHERE CreatedAt >= DATEADD(day, -1, GETDATE());

-- 9. Longest running sessions
SELECT TOP 10
    'Longest Sessions' AS Analysis,
    SessionId,
    COUNT(*) AS StepCount,
    MIN(CreatedAt) AS SessionStart,
    MAX(CreatedAt) AS SessionEnd,
    DATEDIFF(minute, MIN(CreatedAt), MAX(CreatedAt)) AS DurationMinutes,
    MAX(CAST(IsCompleteSession AS INT)) AS WasCompleted
FROM VisaWizards
GROUP BY SessionId
HAVING COUNT(*) > 1
ORDER BY COUNT(*) DESC;

-- 10. Failed sessions analysis
SELECT TOP 10
    'Failed Sessions Analysis' AS Analysis,
    Country,
    Purpose,
    COUNT(*) AS IncompleteRecords,
    AVG(CAST(StepNumber AS FLOAT)) AS AvgStepsReached,
    MAX(StepNumber) AS MaxStepsReached
FROM VisaWizards
WHERE IsCompleteSession = 0
GROUP BY Country, Purpose
HAVING COUNT(*) > 1
ORDER BY COUNT(*) DESC;

-- 11. Sample of incomplete records to see patterns
SELECT TOP 20
    'Sample Incomplete Records' AS Analysis,
    Country,
    Purpose,
    StepNumber,
    LEFT(Question1, 100) AS Question1_Sample,
    LEFT(Answer1, 50) AS Answer1_Sample,
    LEFT(Question2, 100) AS Question2_Sample,
    LEFT(Answer2, 50) AS Answer2_Sample,
    HasFollowUp,
    CreatedAt
FROM VisaWizards
WHERE IsCompleteSession = 0
ORDER BY CreatedAt DESC;

-- 12. Sample of complete records for comparison
SELECT TOP 10
    'Sample Complete Records' AS Analysis,
    Country,
    Purpose,
    StepNumber,
    LEN(OutcomeDisplayContent) AS OutcomeLength,
    LEN(VisaRecommendations) AS RecommendationsLength,
    HasFollowUp,
    CreatedAt
FROM VisaWizards
WHERE IsCompleteSession = 1
ORDER BY CreatedAt DESC;

-- 13. Error pattern analysis - look for empty or problematic data
SELECT 
    'Error Patterns' AS Analysis,
    'Empty Questions' AS ErrorType,
    COUNT(*) AS Count
FROM VisaWizards
WHERE (Question1 IS NULL OR Question1 = '') AND StepNumber > 0

UNION ALL

SELECT 
    'Error Patterns' AS Analysis,
    'Empty Answers' AS ErrorType,
    COUNT(*) AS Count
FROM VisaWizards
WHERE (Answer1 IS NULL OR Answer1 = '') AND Question1 IS NOT NULL AND Question1 != ''

UNION ALL

SELECT 
    'Error Patterns' AS Analysis,
    'Missing Follow-up Logic' AS ErrorType,
    COUNT(*) AS Count
FROM VisaWizards
WHERE HasFollowUp = 1 AND (Question2 IS NULL OR Question2 = '')

UNION ALL

SELECT 
    'Error Patterns' AS Analysis,
    'Incomplete Outcomes' AS ErrorType,
    COUNT(*) AS Count
FROM VisaWizards
WHERE IsCompleteSession = 1 AND (OutcomeDisplayContent IS NULL OR OutcomeDisplayContent = '');

-- 14. Most recent records to check current state
SELECT TOP 50
    'Most Recent Records' AS Analysis,
    CreatedAt,
    Country,
    Purpose,
    StepNumber,
    IsCompleteSession,
    HasFollowUp,
    CASE WHEN Question1 IS NOT NULL AND Question1 != '' THEN 'Yes' ELSE 'No' END AS HasQuestion1,
    CASE WHEN Answer1 IS NOT NULL AND Answer1 != '' THEN 'Yes' ELSE 'No' END AS HasAnswer1,
    SessionId
FROM VisaWizards
ORDER BY CreatedAt DESC;