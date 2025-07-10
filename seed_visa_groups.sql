-- Inserts visa group labels and one example visa per group
-- I WAS UNABLE TO SCAN

-- Visa Groups
INSERT INTO VisaGroups (Id, Name) VALUES
  ('11111111-1111-1111-1111-111111111111', 1), -- Visit
  ('22222222-2222-2222-2222-222222222222', 2), -- Immigrate
  ('33333333-3333-3333-3333-333333333333', 3), -- Investment
  ('44444444-4444-4444-4444-444444444444', 4), -- Work
  ('55555555-5555-5555-5555-555555555555', 5), -- Asylum
  ('66666666-6666-6666-6666-666666666666', 6), -- Study
  ('77777777-7777-7777-7777-777777777777', 7); -- Family

-- Visa Types mapped to groups
INSERT INTO VisaTypes (Id, Name, Description, Category, VisaGroupId) VALUES
  ('1aa2bf5e-7242-49c2-9303-04ddef44e8b1', 'B1/B2 Visitor Visa', 'Temporary visit for business or tourism', 'Visit', '11111111-1111-1111-1111-111111111111'),
  ('a8e01e04-27a1-4380-b9cc-cace62830fab', 'Family Based Green Card', 'Immigrate through qualifying family', 'Immigrate', '22222222-2222-2222-2222-222222222222'),
  ('88888888-8888-8888-8888-888888888888', 'EB-5 Immigrant Investor', 'Investment-based path to permanent residence', 'Investment', '33333333-3333-3333-3333-333333333333'),
  ('162e3e30-ec8b-438e-8f96-e836465d0908', 'H1B Specialty Occupation', 'Work visa for specialty occupations', 'Work', '44444444-4444-4444-4444-444444444444'),
  ('a767bd9a-272e-440d-99fc-955e1b1d9303', 'Asylum', 'Protection for those fearing persecution', 'Protect', '55555555-5555-5555-5555-555555555555'),
  ('cdeb31d4-d778-495b-a4e3-dcd67e1aa737', 'F1 Student Visa', 'Academic study in the U.S.', 'Study', '66666666-6666-6666-6666-666666666666'),
  ('99999999-9999-9999-9999-999999999999', 'K-1 Fianc\u00e9(e) Visa', 'For foreign-citizen fianc\u00e9s of U.S. citizens', 'Family', '77777777-7777-7777-7777-777777777777');
