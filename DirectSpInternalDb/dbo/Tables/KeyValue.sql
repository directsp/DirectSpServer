CREATE TABLE [dbo].[KeyValue] (
    [KeyId]          INT            IDENTITY (1, 1) NOT NULL,
    [KeyName]        NVARCHAR (255) NOT NULL,
    [TextValue]      NVARCHAR (MAX) NULL,
    [ModifiedTime]   DATETIME       CONSTRAINT [DF__KeyValue__Modifi__4D5F7D71] DEFAULT (getdate()) NOT NULL,
    [ExpirationTime] DATETIME       NULL,
    CONSTRAINT [PK_KeyId] PRIMARY KEY CLUSTERED ([KeyId] ASC),
    CONSTRAINT [IX_KeyName] UNIQUE NONCLUSTERED ([KeyName] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_ExpirationTime]
    ON [dbo].[KeyValue]([ExpirationTime] ASC) WHERE ([ExpirationTime] IS NOT NULL);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'/NoFK', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'KeyValue', @level2type = N'COLUMN', @level2name = N'KeyId';

