IF SCHEMA_ID(N'sam') IS NULL EXEC(N'CREATE SCHEMA [sam];');
GO


CREATE TABLE [sam].[Contact] (
    [Id] int NOT NULL IDENTITY,
    [Name] varchar(128) NOT NULL,
    [Property] varchar(max) NULL,
    [CreatedBy] varchar(128) NOT NULL,
    [ModifiedBy] varchar(128) NOT NULL,
    [CreatedUtc] datetime2 NOT NULL,
    [ModifiedUtc] datetime2 NOT NULL,
    CONSTRAINT [PK_Contact] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [sam].[Address] (
    [Id] int NOT NULL IDENTITY,
    [Line1] varchar(512) NULL,
    [Line2] varchar(512) NULL,
    [City] varchar(512) NULL,
    [State] varchar(512) NULL,
    [PostalCode] varchar(512) NULL,
    [ContactId] int NOT NULL,
    [CreatedBy] varchar(128) NOT NULL,
    [ModifiedBy] varchar(128) NOT NULL,
    [CreatedUtc] datetime2 NOT NULL,
    [ModifiedUtc] datetime2 NOT NULL,
    CONSTRAINT [PK_Address] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Address_Contact_ContactId] FOREIGN KEY ([ContactId]) REFERENCES [sam].[Contact] ([Id]) ON DELETE CASCADE
);
GO


CREATE INDEX [IX_Address_ContactId] ON [sam].[Address] ([ContactId]);
GO



