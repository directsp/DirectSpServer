CREATE TYPE [dbo].[ud_BatchItem] AS TABLE (
    [BatchId]              INT            NULL,
    [BatchItemIndex]       INT            NULL,
    [BatchItemResulTypeId] INT            NULL,
    [CommandResult]        NVARCHAR (255) NULL,
    [CommandSource]        NVARCHAR (512) NULL);

