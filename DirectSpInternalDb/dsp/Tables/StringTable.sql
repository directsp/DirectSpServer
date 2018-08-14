CREATE TABLE [dsp].[StringTable] (
    [StringId]    NVARCHAR (100) NOT NULL,
    [StringValue] NVARCHAR (MAX) NOT NULL,
    [LocaleName]  NVARCHAR (10)  NULL,
    [Description] NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_StringTable_Name] PRIMARY KEY CLUSTERED ([StringId] ASC)
);

