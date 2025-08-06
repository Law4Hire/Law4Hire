-- Clear existing BaseVisaType data
DELETE FROM BaseVisaTypes;

-- Insert a few sample visa records to test the structure
INSERT INTO BaseVisaTypes (Id, Code, Name, Description, Question1, Question2, Question3, VisaName, VisaDescription, VisaAppropriateFor, Status, CreatedAt, UpdatedAt, ConfidenceScore)
VALUES 
(NEWID(), 'A-1', 'A-1 Diplomat / Official', 'For heads of state, government ministers or career diplomats traveling on official government business.', 'Are you traveling on behalf of a foreign government in an official capacity?', 'Are you a head of state, accredited diplomat, or minister?', NULL, 'A-1', 'For heads of state, government ministers or career diplomats traveling on official government business.', 'Diplomatic personnel', 'Active', GETUTCDATE(), GETUTCDATE(), 1.0),
(NEWID(), 'H-1B', 'H-1B Specialty Occupation', 'For specialty occupation workers requiring a bachelor''s degree or equivalent, employer-sponsored.', 'Do you have a job offer in a specialty occupation and at least a bachelor''s degree or equivalent?', 'Has your employer filed a certified Labor Condition Application (LCA)?', NULL, 'H-1B', 'For specialty occupation workers requiring a bachelor''s degree or equivalent, employer-sponsored.', 'Specialty workers', 'Active', GETUTCDATE(), GETUTCDATE(), 1.0),
(NEWID(), 'F-1', 'F-1 Academic Student', 'For academic or language students at an accredited U.S. institution.', 'Do you have an approved Form I-20 from a SEVP-certified school?', 'Is your intent solely for full-time academic study and temporary stay?', NULL, 'F-1', 'For academic or language students at an accredited U.S. institution.', 'Students', 'Active', GETUTCDATE(), GETUTCDATE(), 1.0);

-- Verify the data was loaded
SELECT COUNT(*) AS TotalVisaTypes FROM BaseVisaTypes;
SELECT Code, Name, Question1 FROM BaseVisaTypes ORDER BY Code;