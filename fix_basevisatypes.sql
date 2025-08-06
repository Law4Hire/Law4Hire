-- Fix BaseVisaTypes data
UPDATE BaseVisaTypes 
SET 
    Name = COALESCE(VisaName, 'Unknown'),
    Description = COALESCE(VisaDescription, 'No description'),
    Code = CASE 
        WHEN VisaName IS NOT NULL AND VisaName != '' THEN VisaName
        ELSE 'UNKNOWN-' + CAST(Id AS NVARCHAR(36))
    END
WHERE Code = '' OR Code IS NULL;

-- Check the results
SELECT TOP 5 Id, Code, Name, Description FROM BaseVisaTypes;