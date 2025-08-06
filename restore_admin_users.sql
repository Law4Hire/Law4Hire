-- Restore the admin users that were lost when database was dropped
-- AI user
INSERT INTO [AspNetUsers] (
    [Id], [Email], [UserName], [EmailConfirmed], [LockoutEnabled], 
    [AccessFailedCount], [PhoneNumberConfirmed], [TwoFactorEnabled],
    [FirstName], [LastName], [PhoneNumber], [PreferredLanguage], 
    [CreatedAt], [IsActive], [PasswordHash], [SecurityStamp]
) VALUES (
    NEWID(), 
    'ai@law4hire.com', 
    'ai@law4hire.com', 
    1, 1, 0, 0, 0,
    'AI', 'Assistant', '+1-555-0100', 'en',
    GETUTCDATE(), 1,
    NULL, -- PasswordHash will be set by Identity
    NEWID()
);

-- Denise Cann user  
INSERT INTO [AspNetUsers] (
    [Id], [Email], [UserName], [EmailConfirmed], [LockoutEnabled],
    [AccessFailedCount], [PhoneNumberConfirmed], [TwoFactorEnabled], 
    [FirstName], [LastName], [PhoneNumber], [PreferredLanguage],
    [CreatedAt], [IsActive], [PasswordHash], [SecurityStamp]
) VALUES (
    '0a825cb5-2174-4f8b-e8fc-08ddcc8c307f', 
    'dcann@cannlaw.com', 
    'dcann@cannlaw.com', 
    1, 1, 0, 0, 0,
    'Denise', 'Cann', '+1-555-0101', 'en',
    GETUTCDATE(), 1,
    NULL, -- PasswordHash will be set by Identity
    NEWID()
);

-- Create Admin role if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM [AspNetRoles] WHERE [Name] = 'Admin')
BEGIN
    INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName]) 
    VALUES (NEWID(), 'Admin', 'ADMIN');
END

-- Create LegalProfessionals role if it doesn't exist  
IF NOT EXISTS (SELECT 1 FROM [AspNetRoles] WHERE [Name] = 'LegalProfessionals')
BEGIN
    INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName]) 
    VALUES (NEWID(), 'LegalProfessionals', 'LEGALPROFESSIONALS');
END

-- Add users to roles
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId])
SELECT u.[Id], r.[Id] 
FROM [AspNetUsers] u, [AspNetRoles] r 
WHERE u.[Email] = 'ai@law4hire.com' AND r.[Name] = 'Admin'
AND NOT EXISTS (SELECT 1 FROM [AspNetUserRoles] WHERE [UserId] = u.[Id] AND [RoleId] = r.[Id]);

INSERT INTO [AspNetUserRoles] ([UserId], [RoleId])
SELECT u.[Id], r.[Id] 
FROM [AspNetUsers] u, [AspNetRoles] r 
WHERE u.[Email] = 'ai@law4hire.com' AND r.[Name] = 'LegalProfessionals'
AND NOT EXISTS (SELECT 1 FROM [AspNetUserRoles] WHERE [UserId] = u.[Id] AND [RoleId] = r.[Id]);

INSERT INTO [AspNetUserRoles] ([UserId], [RoleId])
SELECT u.[Id], r.[Id] 
FROM [AspNetUsers] u, [AspNetRoles] r 
WHERE u.[Email] = 'dcann@cannlaw.com' AND r.[Name] = 'Admin'
AND NOT EXISTS (SELECT 1 FROM [AspNetUserRoles] WHERE [UserId] = u.[Id] AND [RoleId] = r.[Id]);

INSERT INTO [AspNetUserRoles] ([UserId], [RoleId])
SELECT u.[Id], r.[Id] 
FROM [AspNetUsers] u, [AspNetRoles] r 
WHERE u.[Email] = 'dcann@cannlaw.com' AND r.[Name] = 'LegalProfessionals'
AND NOT EXISTS (SELECT 1 FROM [AspNetUserRoles] WHERE [UserId] = u.[Id] AND [RoleId] = r.[Id]);