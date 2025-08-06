-- Comprehensive analysis of VisaWizards database table
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
SELECT 'Records by Purpose (Top 10)' AS Analysis, Purpose, COUNT(*) AS Count
FROM VisaWizards
GROUP BY Purpose
ORDER BY COUNT(*) DESC
LIMIT 10;

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
    DATE(CreatedAt) AS CreationDate,
    COUNT(*) AS RecordsCreated,
    MIN(CreatedAt) AS FirstRecord,
    MAX(CreatedAt) AS LastRecord
FROM VisaWizards
GROUP BY DATE(CreatedAt)
ORDER BY CreationDate DESC;

-- 8. Recent session analysis (last 24 hours)
SELECT 
    'Recent Sessions (24h)' AS Analysis,
    COUNT(*) AS TotalRecords,
    COUNT(DISTINCT SessionId) AS UniqueSessions,
    AVG(StepNumber) AS AvgStepNumber,
    MAX(StepNumber) AS MaxStepNumber
FROM VisaWizards
WHERE CreatedAt >= datetime('now', '-1 day');

-- 9. Longest running sessions
SELECT 
    'Longest Sessions' AS Analysis,
    SessionId,
    COUNT(*) AS StepCount,
    MIN(CreatedAt) AS SessionStart,
    MAX(CreatedAt) AS SessionEnd,
    ROUND((julianday(MAX(CreatedAt)) - julianday(MIN(CreatedAt))) * 24 * 60, 2) AS DurationMinutes,
    MAX(IsCompleteSession) AS WasCompleted
FROM VisaWizards
GROUP BY SessionId
HAVING COUNT(*) > 1
ORDER BY COUNT(*) DESC
LIMIT 10;

-- 10. Failed sessions analysis
SELECT 
    'Failed Sessions Analysis' AS Analysis,
    Country,
    Purpose,
    COUNT(*) AS IncompleteRecords,
    AVG(StepNumber) AS AvgStepsReached,
    MAX(StepNumber) AS MaxStepsReached
FROM VisaWizards
WHERE IsCompleteSession = 0
GROUP BY Country, Purpose
HAVING COUNT(*) > 1
ORDER BY COUNT(*) DESC
LIMIT 10;

-- 11. Sample of incomplete records to see patterns
SELECT 
    'Sample Incomplete Records' AS Analysis,
    Country,
    Purpose,
    StepNumber,
    Question1,
    Answer1,
    Question2,
    Answer2,
    HasFollowUp,
    CreatedAt
FROM VisaWizards
WHERE IsCompleteSession = 0
ORDER BY CreatedAt DESC
LIMIT 20;

-- 12. Sample of complete records for comparison
SELECT 
    'Sample Complete Records' AS Analysis,
    Country,
    Purpose,
    StepNumber,
    LENGTH(OutcomeDisplayContent) AS OutcomeLength,
    LENGTH(VisaRecommendations) AS RecommendationsLength,
    HasFollowUp,
    CreatedAt
FROM VisaWizards
WHERE IsCompleteSession = 1
ORDER BY CreatedAt DESC
LIMIT 10;

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