-- Insert basic visa data for testing
INSERT INTO BaseVisaTypes (Id, Code, Name, Description, Status, CreatedAt, UpdatedAt, ConfidenceScore, Question1, Question2, Question3, VisaName, VisaDescription, VisaAppropriateFor)
VALUES 
    (NEWID(), 'H-1B', 'H-1B Specialty Occupation', 'For professionals with specialized knowledge and a bachelor degree or higher', 'Active', GETDATE(), GETDATE(), 0.85, 'Do you have a job offer requiring specialized knowledge?', 'Do you have a bachelor degree or equivalent?', 'Is your degree related to the job position?', 'H-1B', 'For professionals with specialized knowledge and a bachelor degree or higher', 'Professionals with specialized skills'),
    (NEWID(), 'F-1', 'F-1 Student Visa', 'For academic study at approved institutions', 'Active', GETDATE(), GETDATE(), 0.90, 'Are you enrolled in a SEVP-certified school?', 'Do you plan to study full-time?', 'Do you have sufficient funds for education?', 'F-1', 'For academic study at approved institutions', 'Students pursuing academic education'),
    (NEWID(), 'B-1/B-2', 'B-1/B-2 Visitor', 'For business or tourism purposes', 'Active', GETDATE(), GETDATE(), 0.75, 'Are you visiting for business or tourism?', 'Do you plan to return to your home country?', 'Do you have ties to your home country?', 'B-1/B-2', 'For business or tourism purposes', 'Business visitors and tourists'),
    (NEWID(), 'L-1', 'L-1 Intracompany Transfer', 'For employees of multinational companies', 'Active', GETDATE(), GETDATE(), 0.80, 'Do you work for a multinational company?', 'Have you worked for the company for at least 1 year?', 'Are you being transferred to a US office?', 'L-1', 'For employees of multinational companies', 'Intracompany transferees'),
    (NEWID(), 'O-1', 'O-1 Extraordinary Ability', 'For individuals with extraordinary ability', 'Active', GETDATE(), GETDATE(), 0.70, 'Do you have extraordinary ability in your field?', 'Do you have national or international recognition?', 'Do you have awards or published works?', 'O-1', 'For individuals with extraordinary ability', 'Individuals with extraordinary abilities');

-- Insert basic category data  
INSERT INTO VisaCategories (Id, Name, VisaTypesJson, CreatedAt, UpdatedAt)
VALUES 
    (NEWID(), 'Work', '["H-1B", "L-1", "O-1"]', GETDATE(), GETDATE()),
    (NEWID(), 'Study', '["F-1"]', GETDATE(), GETDATE()),
    (NEWID(), 'Visit', '["B-1/B-2"]', GETDATE(), GETDATE());

-- Show results
SELECT Code, Name, Description FROM BaseVisaTypes;
SELECT Name FROM VisaCategories;