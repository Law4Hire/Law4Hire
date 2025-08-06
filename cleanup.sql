-- Law4Hire Database Cleanup Script
-- This script will delete all user-generated data while preserving BaseVisaType and other seed data
-- Execute with caution - this will permanently delete user data

-- Disable foreign key checks to avoid constraint issues during deletion
-- (This syntax may vary depending on your database system)
-- For SQL Server: EXEC sp_MSforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all"
-- For MySQL: SET FOREIGN_KEY_CHECKS = 0;
-- For PostgreSQL: This script handles foreign keys by deleting in correct order

-- Begin transaction for safety
BEGIN TRANSACTION;

-- Delete user-related workflow and interview data (most dependent first)
DELETE FROM UserDocumentStatuses;
DELETE FROM WorkflowStepDocuments; 
DELETE FROM WorkflowSteps;
DELETE FROM VisaInterviewStates;

-- Delete session and intake data
DELETE FROM IntakeSessionQuestions;
DELETE FROM IntakeSessions;

-- Delete user authentication and profile data
DELETE FROM Users;

-- Delete any audit/log tables if they exist (uncomment if present)
-- DELETE FROM AuditLogs WHERE EntityType IN ('User', 'VisaInterviewState', 'WorkflowStep', 'IntakeSession');

-- Reset identity columns to start from 1 (if using IDENTITY columns)
-- This syntax varies by database system:

-- For SQL Server:
-- DBCC CHECKIDENT ('Users', RESEED, 0);
-- DBCC CHECKIDENT ('VisaInterviewStates', RESEED, 0);
-- DBCC CHECKIDENT ('WorkflowSteps', RESEED, 0);
-- DBCC CHECKIDENT ('IntakeSessions', RESEED, 0);

-- For MySQL:
-- ALTER TABLE Users AUTO_INCREMENT = 1;
-- ALTER TABLE VisaInterviewStates AUTO_INCREMENT = 1;
-- ALTER TABLE WorkflowSteps AUTO_INCREMENT = 1;
-- ALTER TABLE IntakeSessions AUTO_INCREMENT = 1;

-- For PostgreSQL:
-- ALTER SEQUENCE users_id_seq RESTART WITH 1;
-- ALTER SEQUENCE visainterviewstates_id_seq RESTART WITH 1;
-- ALTER SEQUENCE workflowsteps_id_seq RESTART WITH 1;
-- ALTER SEQUENCE intakesessions_id_seq RESTART WITH 1;

-- Re-enable foreign key checks
-- For SQL Server: EXEC sp_MSforeachtable "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all"
-- For MySQL: SET FOREIGN_KEY_CHECKS = 1;

-- Commit the transaction
COMMIT;

-- Verify cleanup by checking row counts
SELECT 'Users' as TableName, COUNT(*) as RowCount FROM Users
UNION ALL
SELECT 'VisaInterviewStates', COUNT(*) FROM VisaInterviewStates
UNION ALL  
SELECT 'WorkflowSteps', COUNT(*) FROM WorkflowSteps
UNION ALL
SELECT 'IntakeSessions', COUNT(*) FROM IntakeSessions
UNION ALL
SELECT 'UserDocumentStatuses', COUNT(*) FROM UserDocumentStatuses
UNION ALL
SELECT 'BaseVisaTypes (preserved)', COUNT(*) FROM BaseVisaTypes
UNION ALL
SELECT 'VisaCategories (preserved)', COUNT(*) FROM VisaCategories
UNION ALL
SELECT 'ServicePackages (preserved)', COUNT(*) FROM ServicePackages
UNION ALL
SELECT 'LocalizedContents (preserved)', COUNT(*) FROM LocalizedContents;

-- The following tables should still have data (preserved):
-- - BaseVisaTypes (seed data)
-- - VisaCategories (seed data) 
-- - ServicePackages (seed data)
-- - LocalizedContents (seed data)
-- - DocumentTypes (if any exist)

PRINT 'Cleanup completed successfully. User data has been wiped while preserving seed data.';