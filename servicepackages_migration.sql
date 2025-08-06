-- ServicePackages Migration Script
-- This script rebuilds the ServicePackages table with VisaTypeId and seeds default pricing tiers

-- Step 1: Drop existing ServicePackages table if it exists
IF OBJECT_ID('dbo.ServicePackages', 'U') IS NOT NULL
BEGIN
    -- Drop foreign key constraints referencing ServicePackages first
    IF OBJECT_ID('dbo.FK_ServiceRequests_ServicePackages_ServicePackageId', 'F') IS NOT NULL
        ALTER TABLE [dbo].[ServiceRequests] DROP CONSTRAINT [FK_ServiceRequests_ServicePackages_ServicePackageId];
    
    DROP TABLE [dbo].[ServicePackages];
END
GO

-- Step 2: Recreate ServicePackages table with new structure
CREATE TABLE [dbo].[ServicePackages] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Description] nvarchar(1000) NOT NULL,
    [Type] int NOT NULL,
    [BasePrice] decimal(18,2) NOT NULL,
    [HasMoneyBackGuarantee] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [LastModifiedAt] datetime2 NULL,
    [VisaTypeId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_ServicePackages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ServicePackages_BaseVisaTypes_VisaTypeId] FOREIGN KEY ([VisaTypeId]) REFERENCES [dbo].[BaseVisaTypes] ([Id]) ON DELETE CASCADE
);
GO

-- Step 3: Create index on VisaTypeId for performance
CREATE NONCLUSTERED INDEX [IX_ServicePackages_VisaTypeId] ON [dbo].[ServicePackages] ([VisaTypeId]);
GO

-- Step 4: Add Admin role to AspNetRoles if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetRoles] WHERE [Name] = 'Admin')
BEGIN
    INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
    VALUES (NEWID(), 'Admin', 'ADMIN', NEWID());
END
GO

-- Step 5: Seed default service packages for each active BaseVisaType
-- We'll create 4 pricing tiers for each visa type
DECLARE @VisaTypeId uniqueidentifier;
DECLARE @VisaTypeName nvarchar(100);

DECLARE visa_cursor CURSOR FOR 
SELECT [Id], [Name] FROM [dbo].[BaseVisaTypes] WHERE [Status] = 'Active';

OPEN visa_cursor;
FETCH NEXT FROM visa_cursor INTO @VisaTypeId, @VisaTypeName;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Self-representation package
    INSERT INTO [dbo].[ServicePackages] 
    ([Name], [Description], [Type], [BasePrice], [HasMoneyBackGuarantee], [IsActive], [CreatedAt], [VisaTypeId])
    VALUES 
    (
        @VisaTypeName + ' - Self Representation with Paralegal', 
        'DIY guidance and document templates with paralegal support for ' + @VisaTypeName + ' visa application', 
        1, -- PackageType.SelfRepresentationWithParalegal
        500.00, 
        0, -- No guarantee for self-rep
        1, -- Active
        GETUTCDATE(), 
        @VisaTypeId
    );

    -- Hybrid package
    INSERT INTO [dbo].[ServicePackages] 
    ([Name], [Description], [Type], [BasePrice], [HasMoneyBackGuarantee], [IsActive], [CreatedAt], [VisaTypeId])
    VALUES 
    (
        @VisaTypeName + ' - Hybrid with Attorney Overview', 
        'Attorney review and guidance with client preparation for ' + @VisaTypeName + ' visa', 
        2, -- PackageType.HybridWithAttorneyOverview
        1000.00, 
        1, -- Money back guarantee
        1, -- Active
        GETUTCDATE(), 
        @VisaTypeId
    );

    -- Full representation standard
    INSERT INTO [dbo].[ServicePackages] 
    ([Name], [Description], [Type], [BasePrice], [HasMoneyBackGuarantee], [IsActive], [CreatedAt], [VisaTypeId])
    VALUES 
    (
        @VisaTypeName + ' - Full Representation Standard', 
        'Complete attorney representation for ' + @VisaTypeName + ' visa application', 
        3, -- PackageType.FullRepresentationStandard
        2500.00, 
        1, -- Money back guarantee
        1, -- Active
        GETUTCDATE(), 
        @VisaTypeId
    );

    -- Full representation guaranteed
    INSERT INTO [dbo].[ServicePackages] 
    ([Name], [Description], [Type], [BasePrice], [HasMoneyBackGuarantee], [IsActive], [CreatedAt], [VisaTypeId])
    VALUES 
    (
        @VisaTypeName + ' - Full Representation Guaranteed', 
        'Premium attorney representation with guaranteed approval or money back for ' + @VisaTypeName + ' visa', 
        4, -- PackageType.FullRepresentationGuaranteed
        5000.00, 
        1, -- Money back guarantee
        1, -- Active
        GETUTCDATE(), 
        @VisaTypeId
    );

    FETCH NEXT FROM visa_cursor INTO @VisaTypeId, @VisaTypeName;
END

CLOSE visa_cursor;
DEALLOCATE visa_cursor;
GO

-- Step 6: Update existing users' passwords and assign Admin role
-- Update dcann@cannsoft.com password
DECLARE @DcannUserId nvarchar(450);
DECLARE @AiUserId nvarchar(450);
DECLARE @AdminRoleId nvarchar(450);

-- Get Admin role ID
SELECT @AdminRoleId = [Id] FROM [dbo].[AspNetRoles] WHERE [Name] = 'Admin';

-- Get user IDs
SELECT @DcannUserId = [Id] FROM [dbo].[AspNetUsers] WHERE [Email] = 'dcann@cannsoft.com';
SELECT @AiUserId = [Id] FROM [dbo].[AspNetUsers] WHERE [Email] = 'ai@law4hire.com';

-- Update passwords (hashed for SecureTest123!)
-- Note: This is a BCrypt hash for 'SecureTest123!' with work factor 10
DECLARE @HashedPassword nvarchar(max) = 'AQAAAAEAACcQAAAAEMwVzgyzv8r+JwG4K5Nk7qSTk+Zy8j5R9iGLO2PuHr8hCxJx4Nz6aQsE/K+D9eHYTg==';

IF @DcannUserId IS NOT NULL
BEGIN
    UPDATE [dbo].[AspNetUsers] 
    SET [PasswordHash] = @HashedPassword,
        [SecurityStamp] = NEWID(),
        [ConcurrencyStamp] = NEWID()
    WHERE [Id] = @DcannUserId;

    -- Add to Admin role if not already there
    IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetUserRoles] WHERE [UserId] = @DcannUserId AND [RoleId] = @AdminRoleId)
    BEGIN
        INSERT INTO [dbo].[AspNetUserRoles] ([UserId], [RoleId])
        VALUES (@DcannUserId, @AdminRoleId);
    END
END

IF @AiUserId IS NOT NULL
BEGIN
    UPDATE [dbo].[AspNetUsers] 
    SET [PasswordHash] = @HashedPassword,
        [SecurityStamp] = NEWID(),
        [ConcurrencyStamp] = NEWID()
    WHERE [Id] = @AiUserId;

    -- Add to Admin role if not already there
    IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetUserRoles] WHERE [UserId] = @AiUserId AND [RoleId] = @AdminRoleId)
    BEGIN
        INSERT INTO [dbo].[AspNetUserRoles] ([UserId], [RoleId])
        VALUES (@AiUserId, @AdminRoleId);
    END
END
GO

-- Step 7: Recreate foreign key constraint for ServiceRequests if it exists
IF OBJECT_ID('dbo.ServiceRequests', 'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ServiceRequests_ServicePackages_ServicePackageId')
    BEGIN
        ALTER TABLE [dbo].[ServiceRequests]
        ADD CONSTRAINT [FK_ServiceRequests_ServicePackages_ServicePackageId] 
        FOREIGN KEY ([ServicePackageId]) REFERENCES [dbo].[ServicePackages] ([Id]);
    END
END
GO

PRINT 'ServicePackages migration completed successfully!';
PRINT 'Admin role added and users updated.';
PRINT 'Service packages seeded for all active visa types.';