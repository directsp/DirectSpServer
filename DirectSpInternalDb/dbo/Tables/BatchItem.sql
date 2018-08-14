CREATE TABLE [dbo].[BatchItem] (
    [BatchId]               INT             NOT NULL,
    [BatchItemIndex]        INT             NOT NULL,
    [CommandSource]         NVARCHAR (1000) NOT NULL,
    [BatchItemResultTypeId] TINYINT         NOT NULL,
    [CommandResult]         NVARCHAR (300)  NULL,
    CONSTRAINT [PK_BatchIdBatchItemIndexId] PRIMARY KEY CLUSTERED ([BatchItemIndex] ASC, [BatchId] ASC),
    CONSTRAINT [FK_BatchItem_Batch_BatchId] FOREIGN KEY ([BatchId]) REFERENCES [dbo].[Batch] ([BatchId]) ON DELETE CASCADE,
    CONSTRAINT [FK_BatchItem_BatchItemResultType_BatchItemResultTypeId] FOREIGN KEY ([BatchItemResultTypeId]) REFERENCES [dbo].[BatchItemResultType] ([BatchItemResultTypeId])
);

