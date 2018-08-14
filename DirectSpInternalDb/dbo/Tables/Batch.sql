CREATE TABLE [dbo].[Batch] (
    [BatchId]         INT            IDENTITY (1, 1) NOT NULL,
    [BatchName]       NVARCHAR (100) NULL,
    [OwnerUserId]     INT            NOT NULL,
    [IsCanceled]      BIT            CONSTRAINT [DF_Batch_IsCanceled] DEFAULT ((0)) NOT NULL,
    [IsCompleted]     BIT            CONSTRAINT [DF_Batch_IsCompleted] DEFAULT ((0)) NOT NULL,
    [TotalItemCount]  INT            CONSTRAINT [DF_Batch_TotalCommand] DEFAULT ((0)) NOT NULL,
    [ProccessedCount] INT            CONSTRAINT [DF_Batch_ProccessedCount] DEFAULT ((0)) NOT NULL,
    [SuccessedCount]  INT            CONSTRAINT [DF_Batch_SuccessedCount] DEFAULT ((0)) NOT NULL,
    [WarningCount]    INT            CONSTRAINT [DF_Batch_WarningCount] DEFAULT ((0)) NOT NULL,
    [ErrorCount]      INT            CONSTRAINT [DF_Batch_ErrorCount] DEFAULT ((0)) NOT NULL,
    [ErrorMessage]    NVARCHAR (MAX) NULL,
    [ModifiedTime]    DATETIME       CONSTRAINT [DF_Batch_ModifiedTime] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_BatchId] PRIMARY KEY CLUSTERED ([BatchId] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'/NoFK', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Batch', @level2type = N'COLUMN', @level2name = N'OwnerUserId';

