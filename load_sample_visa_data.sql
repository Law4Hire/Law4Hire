-- Load sample visa data from Updated.json format
-- This demonstrates the new structure with Code, Name, Description, and Questions

INSERT INTO [BaseVisaTypes] ([Id], [Code], [Name], [Description], [Question1], [Question2], [Question3], [Status], [CreatedAt], [UpdatedAt], [ConfidenceScore])
VALUES 
(NEWID(), 'A-1', 'A-1 Diplomat / Official', 'For heads of state, government ministers or career diplomats traveling on official government business.', 'Are you traveling on behalf of a foreign government in an official capacity?', 'Are you a head of state, accredited diplomat, or minister?', NULL, 'Active', GETUTCDATE(), GETUTCDATE(), 1.0),

(NEWID(), 'A-2', 'A-2 Foreign Government Official/Employee', 'For other foreign government officials and employees, and immediate family.', 'Are you employed by a foreign government in an official capacity?', 'Are you accompanied by immediate family eligible under A-2 classification?', NULL, 'Active', GETUTCDATE(), GETUTCDATE(), 1.0),

(NEWID(), 'A-3', 'A-3 Attendant/Personal Employee', 'For attendants or personal employees of A-1/A-2 principals and their immediate family.', 'Are you employed directly as a servant, attendant, or employee by an A-1/A-2 visa holder?', 'Does your employer provide room, board, and prevailing wage as required?', NULL, 'Active', GETUTCDATE(), GETUTCDATE(), 1.0),

(NEWID(), 'B-1/B-2', 'Visitor Visa (Business & Tourism)', 'Temporary visitor for business (B-1) or pleasure/medical treatment (B-2), must overcome INA 214(b) presumption of immigrant intent.', 'Can you demonstrate intent to return (e.g., strong ties to home country)?', 'Do you plan to remain only temporarily and have funds to cover your expenses?', NULL, 'Active', GETUTCDATE(), GETUTCDATE(), 1.0),

(NEWID(), 'H-1B', 'H-1B Specialty Occupation', 'For specialty occupation workers requiring a bachelor''s degree or equivalent, employer-sponsored.', 'Do you have a job offer in a specialty occupation and at least a bachelor''s degree or equivalent?', 'Has your employer filed a certified Labor Condition Application (LCA)?', NULL, 'Active', GETUTCDATE(), GETUTCDATE(), 1.0),

(NEWID(), 'F-1', 'F-1 Academic Student', 'For academic or language students at an accredited U.S. institution.', 'Do you have an approved Form I-20 from a SEVP-certified school?', 'Is your intent solely for full-time academic study and temporary stay?', NULL, 'Active', GETUTCDATE(), GETUTCDATE(), 1.0),

(NEWID(), 'TN', 'TN NAFTA / USMCA Professional', 'For Canadian and Mexican professionals under USMCA agreement.', 'Are you a Canadian or Mexican citizen with a job offer in a qualifying profession?', 'Do you have the required credentials or licensure for that profession?', NULL, 'Active', GETUTCDATE(), GETUTCDATE(), 1.0),

(NEWID(), 'EB-1A', 'EB-1 Extraordinary Ability (Immigrant)', 'Employment-based first preference for persons of extraordinary ability.', 'Can you demonstrate sustained national or international acclaim in your field?', 'Do you meet at least three of the regulatory criteria for extraordinary ability?', NULL, 'Active', GETUTCDATE(), GETUTCDATE(), 1.0);

PRINT 'Sample visa data loaded successfully. This demonstrates the new BaseVisaTypes structure with Code, Name, Description, and qualifying questions.';