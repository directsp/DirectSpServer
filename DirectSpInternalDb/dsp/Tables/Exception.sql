CREATE TABLE [dsp].[Exception] (
    [ExceptionId]   INT            NOT NULL,
    [ExceptionName] NVARCHAR (100) NOT NULL,
    [StringId]      NVARCHAR (100) NULL,
    [Description]   NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_Exception] PRIMARY KEY CLUSTERED ([ExceptionId] ASC),
    CONSTRAINT [FK_Exception_StringTable_StringId] FOREIGN KEY ([StringId]) REFERENCES [dsp].[StringTable] ([StringId])
);

