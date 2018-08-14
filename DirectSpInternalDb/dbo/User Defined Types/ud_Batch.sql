CREATE TYPE [dbo].[ud_Batch] AS TABLE (
    [BatchId]         INT             NULL,
    [BatchName]       [dbo].[TSTRING] NULL,
    [IsCanceled]      BIT             NULL,
    [IsCompleted]     BIT             NULL,
    [TotalItems]      INT             NULL,
    [TotalProccessed] INT             NULL,
    [SuccessCount]    INT             NULL,
    [WarningCount]    INT             NULL,
    [ErrorCount]      INT             NULL,
    [ErrorResult]     [dbo].[TSTRING] NULL,
    [CreateByUserId]  INT             NULL,
    [ModifiedTime]    DATETIME        DEFAULT (getdate()) NULL);

