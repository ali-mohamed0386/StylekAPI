IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260430205124_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] uniqueidentifier NOT NULL,
        [Email] nvarchar(256) NOT NULL,
        [PasswordHash] nvarchar(512) NOT NULL,
        [Role] nvarchar(20) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260430205124_InitialCreate'
)
BEGIN
    CREATE TABLE [DoctorProfiles] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Name] nvarchar(150) NULL,
        [Specialty] nvarchar(100) NULL,
        [Location] nvarchar(250) NULL,
        [Phone] nvarchar(32) NULL,
        [Price] decimal(18,2) NOT NULL,
        [Bio] nvarchar(1500) NULL,
        [ProfileImageUrl] nvarchar(500) NULL,
        [WorkingTimes] nvarchar(2000) NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_DoctorProfiles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DoctorProfiles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260430205124_InitialCreate'
)
BEGIN
    CREATE TABLE [PatientProfiles] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [PhoneNumber] nvarchar(32) NULL,
        [Bio] nvarchar(1000) NULL,
        [ProfileImageUrl] nvarchar(500) NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_PatientProfiles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PatientProfiles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260430205124_InitialCreate'
)
BEGIN
    CREATE TABLE [Appointments] (
        [Id] uniqueidentifier NOT NULL,
        [PatientProfileId] uniqueidentifier NOT NULL,
        [DoctorProfileId] uniqueidentifier NOT NULL,
        [ScheduledAtUtc] datetime2 NOT NULL,
        [DurationMinutes] int NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [Notes] nvarchar(500) NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_Appointments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Appointments_DoctorProfiles_DoctorProfileId] FOREIGN KEY ([DoctorProfileId]) REFERENCES [DoctorProfiles] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Appointments_PatientProfiles_PatientProfileId] FOREIGN KEY ([PatientProfileId]) REFERENCES [PatientProfiles] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260430205124_InitialCreate'
)
BEGIN
    CREATE TABLE [Ratings] (
        [Id] uniqueidentifier NOT NULL,
        [PatientProfileId] uniqueidentifier NOT NULL,
        [DoctorProfileId] uniqueidentifier NOT NULL,
        [Value] int NOT NULL,
        [Comment] nvarchar(500) NULL,
        [CreatedAtUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_Ratings] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Ratings_Value] CHECK ([Value] BETWEEN 1 AND 5),
        CONSTRAINT [FK_Ratings_DoctorProfiles_DoctorProfileId] FOREIGN KEY ([DoctorProfileId]) REFERENCES [DoctorProfiles] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Ratings_PatientProfiles_PatientProfileId] FOREIGN KEY ([PatientProfileId]) REFERENCES [PatientProfiles] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260430205124_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Appointments_DoctorProfileId_ScheduledAtUtc] ON [Appointments] ([DoctorProfileId], [ScheduledAtUtc]) WHERE [Status] IN (''Pending'', ''Accepted'')');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260430205124_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Appointments_PatientProfileId] ON [Appointments] ([PatientProfileId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260430205124_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_DoctorProfiles_Specialty] ON [DoctorProfiles] ([Specialty]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260430205124_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_DoctorProfiles_UserId] ON [DoctorProfiles] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260430205124_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PatientProfiles_UserId] ON [PatientProfiles] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260430205124_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Ratings_DoctorProfileId] ON [Ratings] ([DoctorProfileId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260430205124_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Ratings_PatientProfileId_DoctorProfileId] ON [Ratings] ([PatientProfileId], [DoctorProfileId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260430205124_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260430205124_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260430205124_InitialCreate', N'10.0.7');
END;

COMMIT;
GO

