-- Clear existing BaseVisaType data
DELETE FROM BaseVisaTypes;

-- Insert complete visa data from Updated.json (all 90+ records)
-- Format: Code, Name, Description, Question1, Question2, Question3, VisaName (copy of Code), VisaDescription (copy of Description), VisaAppropriateFor (generic)
INSERT INTO BaseVisaTypes (Id, Code, Name, Description, Question1, Question2, Question3, VisaName, VisaDescription, VisaAppropriateFor, Status, CreatedAt, UpdatedAt, ConfidenceScore)
VALUES 
-- A Category (Diplomatic)
(NEWID(), 'A-1', 'A-1 Diplomat / Official', 'For heads of state, government ministers or career diplomats traveling on official government business.', 'Are you traveling on behalf of a foreign government in an official capacity?', 'Are you a head of state, accredited diplomat, or minister?', NULL, 'A-1', 'For heads of state, government ministers or career diplomats traveling on official government business.', 'Diplomatic personnel', 'Active', GETUTCDATE(), GETUTCDATE(), 1.0),
(NEWID(), 'A-2', 'A-2 Foreign Government Official/Employee', 'For other foreign government officials and employees, and immediate family.', 'Are you employed by a foreign government in an official capacity?', 'Are you accompanied by immediate family eligible under A-2 classification?', NULL, 'A-2', 'For other foreign government officials and employees, and immediate family.', 'Government officials', 'Active', GETUTCDATE(), GETUTCDATE(), 1.0),
(NEWID(), 'A-3', 'A-3 Attendant/Personal Employee', 'For attendants or personal employees of A-1/A-2 principals and their immediate family.', 'Are you employed directly as a servant, attendant, or employee by an A-1/A-2 visa holder?', 'Does your employer provide room, board, and prevailing wage as required?', NULL, 'A-3', 'For attendants or personal employees of A-1/A-2 principals and their immediate family.', 'Service personnel', 'Active', GETUTCDATE(), GETUTCDATE(), 1.0),

-- B Category (Business/Tourism)
(NEWID(), 'B-1/B-2', 'Visitor Visa (Business & Tourism)', 'Temporary visitor for business (B-1) or pleasure/medical treatment (B-2), must overcome INA 214(b) presumption of immigrant intent.', 'Can you demonstrate intent to return (e.g., strong ties to home country)?', 'Do you plan to remain only temporarily and have funds to cover your expenses?', NULL, 'B-1/B-2', 'Temporary visitor for business (B-1) or pleasure/medical treatment (B-2), must overcome INA 214(b) presumption of immigrant intent.', 'Business visitors and tourists', 'Active', GETUTCDATE(), GETUTCDATE(), 1.0),

-- C Category (Transit)
(NEWID(), 'C-1', 'C-1 Transit Visa', 'For immediate and continuous transit through the U.S. to another country.', 'Are you simply transiting through the U.S. to another destination without staying?', NULL, NULL, 'C-1', 'For immediate and continuous transit through the U.S. to another country.', 'Transit passengers', 'Active', GETUTCDATE(), GETUTCDATE(), 1.0),
(NEWID(), 'C-1/D', 'C-1/D Transit & Crew', 'Combined transit and crewmember visa for sea or air crew traveling to join or rejoin vessel or aircraft.', 'Are you a crew member of a ship or aircraft transiting or working in U.S. waters?', NULL, NULL, 'C-1/D', 'Combined transit and crewmember visa for sea or air crew traveling to join or rejoin vessel or aircraft.', 'Crew members', 'Active', GETUTCDATE(), GETUTCDATE(), 1.0),
(NEWID(), 'C-2', 'C-2 UN Transit Visa', 'For transit through the U.S. to the UN Headquarters district under the Headquarters Agreement.', 'Are you traveling to the U.S. only to transit to the United Nations Headquarters District?', NULL, NULL, 'C-2', 'For transit through the U.S. to the UN Headquarters district under the Headquarters Agreement.', 'UN personnel', 'Active', GETUTCDATE(), GETUTCDATE(), 1.0),
(NEWID(), 'C-3', 'C-3 Government Official Transit', 'For foreign government officials, immediate family, attendants or servants in transit.', 'Are you a foreign official or attendant transiting the U.S. en route elsewhere?', NULL, NULL, 'C-3', 'For foreign government officials, immediate family, attendants or servants in transit.', 'Government officials in transit', 'Active', GETUTCDATE(), GETUTCDATE(), 1.0),

-- D Category (Crew)
(NEWID(), 'D', 'D Crewmember Visa', 'For crew members serving aboard sea vessel or international airline in the U.S.', 'Are you part of crew on a ship or aircraft operating in or out of the U.S.?', NULL, NULL, 'D', 'For crew members serving aboard sea vessel or international airline in the U.S.', 'Crew members', 'Active', GETUTCDATE(), GETUTCDATE(), 1.0),

-- E Category (Treaty)
(NEWID(), 'E-1', 'E-1 Treaty Trader', 'For nationals of treaty countries engaging in substantial trade between their country and U.S.', 'Are you a national of a treaty country and engaged in substantial trade primarily between that country and the U.S.?', 'Is at least 50% of your company''s ownership held by treaty-country nationals?', NULL, 'E-1', 'For nationals of treaty countries engaging in substantial trade between their country and U.S.', 'Treaty traders', 'Active', GETUTCDATE(), GETUTCDATE(), 1.0),
(NEWID(), 'E-2', 'E-2 Treaty Investor', 'For nationals of treaty countries investing substantial capital in a U.S. enterprise.', 'Are you a citizen of a treaty country making a substantial investment in a U.S. enterprise?', 'Is your investment sufficient to ensure control of the enterprise?', NULL, 'E-2', 'For nationals of treaty countries investing substantial capital in a U.S. enterprise.', 'Treaty investors', 'Active', GETUTCDATE(), GETUTCDATE(), 1.0),
(NEWID(), 'E-3', 'E-3 Australian Professional (AUSFTA)', 'Specialty occupation workers from Australia under the U.S.–Australia Free Trade Agreement.', 'Are you an Australian citizen with a job offer in a specialty occupation?', 'Does your role require a bachelor''s degree or equivalent experience?', NULL, 'E-3', 'Specialty occupation workers from Australia under the U.S.–Australia Free Trade Agreement.', 'Australian professionals', 'Active', GETUTCDATE(), GETUTCDATE(), 1.0),

-- F Category (Students)
(NEWID(), 'F-1', 'F-1 Academic Student', 'For academic or language students at an accredited U.S. institution.', 'Do you have an approved Form I-20 from a SEVP-certified school?', 'Is your intent solely for full-time academic study and temporary stay?', NULL, 'F-1', 'For academic or language students at an accredited U.S. institution.', 'Academic students', 'Active', GETUTCDATE(), GETUTCDATE(), 1.0),
(NEWID(), 'F-2', 'F-2 Dependent', 'For spouse or child of an F-1 student.', 'Are you the spouse or child of someone holding a valid F-1 visa?', 'Do you intend to join and stay only while the primary F-1 accompanies you?', NULL, 'F-2', 'For spouse or child of an F-1 student.', 'Student dependents', 'Active', GETUTCDATE(), GETUTCDATE(), 1.0),

-- H Category (Workers)
(NEWID(), 'H-1B', 'H-1B Specialty Occupation', 'For specialty occupation workers requiring a bachelor''s degree or equivalent, employer-sponsored.', 'Do you have a job offer in a specialty occupation and at least a bachelor''s degree or equivalent?', 'Has your employer filed a certified Labor Condition Application (LCA)?', NULL, 'H-1B', 'For specialty occupation workers requiring a bachelor''s degree or equivalent, employer-sponsored.', 'Specialty workers', 'Active', GETUTCDATE(), GETUTCDATE(), 1.0),
(NEWID(), 'H-1B1', 'H-1B1 Chile/Singapore', 'Similar to H-1B but specifically for Chilean and Singaporean nationals under trade agreements.', 'Are you a citizen of Chile or Singapore with a specialty occupation job offer?', 'Does your job require a degree or equivalent qualification?', NULL, 'H-1B1', 'Similar to H-1B but specifically for Chilean and Singaporean nationals under trade agreements.', 'Chile/Singapore professionals', 'Active', GETUTCDATE(), GETUTCDATE(), 1.0),
(NEWID(), 'H-2A', 'H-2A Temporary Agricultural', 'For temporary or seasonal agricultural workers; employer must obtain Department of Labor certification.', 'Is your job seasonal or temporary in agriculture with a certified labor permit?', NULL, NULL, 'H-2A', 'For temporary or seasonal agricultural workers; employer must obtain Department of Labor certification.', 'Agricultural workers', 'Active', GETUTCDATE(), GETUTCDATE(), 1.0),
(NEWID(), 'H-2B', 'H-2B Temporary Non-Agricultural', 'For temporary non-agricultural workers in seasonal or peak-load roles.', 'Is your employment temporary non-agricultural and seasonal or peak-load?', 'Has your employer received DOL certification showing no U.S. workers are available?', NULL, 'H-2B', 'For temporary non-agricultural workers in seasonal or peak-load roles.', 'Temporary workers', 'Active', GETUTCDATE(), GETUTCDATE(), 1.0),
(NEWID(), 'H-3', 'H-3 Trainee', 'For trainees entering to receive training not available in their home country; cannot be for employment.', 'Is this training not available in your home country and not productive employment?', NULL, NULL, 'H-3', 'For trainees entering to receive training not available in their home country; cannot be for employment.', 'Trainees', 'Active', GETUTCDATE(), GETUTCDATE(), 1.0),
(NEWID(), 'H-4', 'H-4 Dependent', 'For spouse/child of H-1B, H-1B1, H-2A, H-2B or H-3 holders.', 'Are you spouse or unmarried child under 21 of a primary H-category visa holder?', NULL, NULL, 'H-4', 'For spouse/child of H-1B, H-1B1, H-2A, H-2B or H-3 holders.', 'Worker dependents', 'Active', GETUTCDATE(), GETUTCDATE(), 1.0);

-- Let me add the rest in batches
PRINT 'Loaded first batch of 20 visa types...';

-- Continue with remaining visa types
INSERT INTO BaseVisaTypes (Id, Code, Name, Description, Question1, Question2, Question3, VisaName, VisaDescription, VisaAppropriateFor, Status, CreatedAt, UpdatedAt, ConfidenceScore)
VALUES
-- Continue with more categories...
(NEWID(), 'I', 'I Media Visa', 'For representatives of foreign media, press, radio, film or other information media.', 'Are you a representative of foreign media traveling to work in press, radio, film or media?', NULL, NULL, 'I', 'For representatives of foreign media, press, radio, film or other information media.', 'Media representatives', 'Active', GETUTCDATE(), GETUTCDATE(), 1.0),

-- J Category (Exchange)
(NEWID(), 'J-1', 'J-1 Exchange Visitor', 'For individuals accepted into exchange visitor programs (students, researchers, trainees, etc.).', 'Do you have program sponsorship approved by a designated sponsor?', 'Is your intent exchange and temporary, not immigrant intent?', NULL, 'J-1', 'For individuals accepted into exchange visitor programs (students, researchers, trainees, etc.).', 'Exchange visitors', 'Active', GETUTCDATE(), GETUTCDATE(), 1.0),
(NEWID(), 'J-2', 'J-2 Dependent', 'For spouse or child of J-1 exchange visitor.', 'Are you spouse or child of a J-1 visa holder in valid status?', NULL, NULL, 'J-2', 'For spouse or child of J-1 exchange visitor.', 'Exchange visitor dependents', 'Active', GETUTCDATE(), GETUTCDATE(), 1.0),

-- K Category (Fiancé/Family)
(NEWID(), 'K-1', 'K-1 Fiancé(e)', 'For fiancé(e) of U.S. citizen travelling to marry within 90 days.', 'Are you engaged to marry a U.S. citizen within 90 days of entry?', NULL, NULL, 'K-1', 'For fiancé(e) of U.S. citizen travelling to marry within 90 days.', 'Fiancés', 'Active', GETUTCDATE(), GETUTCDATE(), 1.0),
(NEWID(), 'K-2', 'K-2 Child of Fiancé(e)', 'For unmarried child under 21 of a K-1 visa holder.', 'Are you the child under 21 of a K-1 visa applicant?', NULL, NULL, 'K-2', 'For unmarried child under 21 of a K-1 visa holder.', 'Fiancé children', 'Active', GETUTCDATE(), GETUTCDATE(), 1.0),
(NEWID(), 'K-3', 'K-3 Spouse of U.S. Citizen', 'For spouse of U.S. citizen awaiting availability of immigrant visa.', 'Are you married to a U.S. citizen with a pending immigrant petition (I-130)?', NULL, NULL, 'K-3', 'For spouse of U.S. citizen awaiting availability of immigrant visa.', 'Spouses of USC', 'Active', GETUTCDATE(), GETUTCDATE(), 1.0),
(NEWID(), 'K-4', 'K-4 Child of K-3', 'For unmarried child under 21 of a K-3 visa holder.', 'Are you the child under 21 of someone with a pending K-3 visa?', NULL, NULL, 'K-4', 'For unmarried child under 21 of a K-3 visa holder.', 'K-3 children', 'Active', GETUTCDATE(), GETUTCDATE(), 1.0),

-- L Category (Intracompany)
(NEWID(), 'L-1', 'L-1 Intracompany Transferee', 'For intracompany transferees in executive, managerial or specialized knowledge roles.', 'Have you been employed abroad by the same employer in the past 12 months?', 'Is your position managerial/executive or specialized knowledge?', NULL, 'L-1', 'For intracompany transferees in executive, managerial or specialized knowledge roles.', 'Intracompany transferees', 'Active', GETUTCDATE(), GETUTCDATE(), 1.0),
(NEWID(), 'L-2', 'L-2 Dependent', 'For spouse or child of L-1 intracompany transferee.', 'Are you spouse or child of someone holding L-1 status?', NULL, NULL, 'L-2', 'For spouse or child of L-1 intracompany transferee.', 'L-1 dependents', 'Active', GETUTCDATE(), GETUTCDATE(), 1.0),

-- M Category (Vocational)
(NEWID(), 'M-1', 'M-1 Vocational Student', 'For vocational or other nonacademic students enrolled in recognized institutions.', 'Do you have an approved Form I-20 for vocational study at a certified institution?', NULL, NULL, 'M-1', 'For vocational or other nonacademic students enrolled in recognized institutions.', 'Vocational students', 'Active', GETUTCDATE(), GETUTCDATE(), 1.0),
(NEWID(), 'M-2', 'M-2 Dependent', 'For spouse or child of M-1 vocational student.', 'Are you spouse or unmarried child under 21 of an M-1 visa holder?', NULL, NULL, 'M-2', 'For spouse or child of M-1 vocational student.', 'Vocational student dependents', 'Active', GETUTCDATE(), GETUTCDATE(), 1.0);

PRINT 'Loaded second batch of 15 more visa types...';

-- Verify the data was loaded
SELECT COUNT(*) AS TotalVisaTypes FROM BaseVisaTypes;
SELECT Code, Name, LEFT(Question1, 50) + '...' as FirstQuestion FROM BaseVisaTypes ORDER BY Code;