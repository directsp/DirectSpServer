CREATE TYPE [dbo].[ud_KeyValue] AS TABLE (
    [KeyName]      [dbo].[TSTRING]    NULL,
    [TextValue]    [dbo].[TBIGSTRING] NULL,
    [ModifiedTime] DATETIME           NULL);

