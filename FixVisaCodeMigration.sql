-- Temporary fix for empty Code values
UPDATE BaseVisaTypes 
SET Code = CASE 
    WHEN VisaSubtype IS NOT NULL AND VisaSubtype != '' THEN VisaSubtype
    WHEN VisaTypeCode IS NOT NULL AND VisaTypeCode != '' THEN VisaTypeCode  
    ELSE 'UNKNOWN-' + CAST(Id AS NVARCHAR(36))
END
WHERE Code = '' OR Code IS NULL;

-- Update Name field
UPDATE BaseVisaTypes 
SET Name = CASE 
    WHEN VisaName IS NOT NULL AND VisaName != '' THEN VisaName
    WHEN VisaSubtype IS NOT NULL AND VisaSubtype != '' THEN VisaSubtype
    ELSE 'Unknown Visa Type'
END
WHERE Name = '' OR Name IS NULL;

-- Update Description field  
UPDATE BaseVisaTypes 
SET Description = CASE 
    WHEN VisaDescription IS NOT NULL AND VisaDescription != '' THEN VisaDescription
    WHEN VisaAppropriateFor IS NOT NULL AND VisaAppropriateFor != '' THEN VisaAppropriateFor
    ELSE 'No description available'
END
WHERE Description = '' OR Description IS NULL;