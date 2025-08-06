-- Migration to restructure BaseVisaTypes and add CategoryClass table

-- First, create the new CategoryClasses table
CREATE TABLE [CategoryClasses] (
    [Id] uniqueidentifier NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [ClassCode] nvarchar(10) NOT NULL,
    [ClassName] nvarchar(100) NOT NULL,
    [Description] nvarchar(max) NULL,
    [GeneralCategory] nvarchar(50) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [IsActive] bit NOT NULL DEFAULT 1
);

-- Create unique index on ClassCode
CREATE UNIQUE INDEX [IX_CategoryClasses_ClassCode] ON [CategoryClasses] ([ClassCode]);

-- Now restructure BaseVisaTypes table
-- First, add the new columns
ALTER TABLE [BaseVisaTypes] ADD [Code] nvarchar(50) NULL;
ALTER TABLE [BaseVisaTypes] ADD [Name] nvarchar(200) NULL;
ALTER TABLE [BaseVisaTypes] ADD [Description] nvarchar(max) NULL;
ALTER TABLE [BaseVisaTypes] ADD [Question1] nvarchar(max) NULL;
ALTER TABLE [BaseVisaTypes] ADD [Question2] nvarchar(max) NULL;
ALTER TABLE [BaseVisaTypes] ADD [Question3] nvarchar(max) NULL;

-- Copy existing data to new columns (temporary mapping)
UPDATE [BaseVisaTypes] 
SET [Code] = [VisaName],
    [Name] = [VisaName],
    [Description] = [VisaDescription];

-- Make the new columns NOT NULL where appropriate
ALTER TABLE [BaseVisaTypes] ALTER COLUMN [Code] nvarchar(50) NOT NULL;
ALTER TABLE [BaseVisaTypes] ALTER COLUMN [Name] nvarchar(200) NOT NULL;
ALTER TABLE [BaseVisaTypes] ALTER COLUMN [Description] nvarchar(max) NOT NULL;

-- Drop the old unique index on VisaName
DROP INDEX [IX_BaseVisaTypes_VisaName] ON [BaseVisaTypes];

-- Create new unique index on Code
CREATE UNIQUE INDEX [IX_BaseVisaTypes_Code] ON [BaseVisaTypes] ([Code]);

-- Clear existing data to prepare for new data load
DELETE FROM [BaseVisaTypes];

PRINT 'Schema migration completed successfully. Ready for data loading.';