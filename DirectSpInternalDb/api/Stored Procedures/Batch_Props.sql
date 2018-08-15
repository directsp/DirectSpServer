/*
#MetaStart 
	{
		"DataAccessMode": "Read"
	} 
#MetaEnd 
*/
CREATE PROCEDURE [api].[Batch_Props]
    @Context TCONTEXT OUTPUT, @BatchId INT, @IsIncludedCommandResult BIT = 0, @IsIncludedCommandSource BIT = 1, @BatchName TSTRING = NULL OUT,
    @UserId INT = NULL, @TotalItemCount INT = NULL OUT, @ProcessedCount INT = NULL OUT, @SuccessCount INT = NULL OUT, @ErrorCount INT = NULL OUT,
    @WarningCount INT = NULL OUT, @IsCompleted BIT = NULL OUT, @IsCanceled BIT = NULL OUT, @ErrorMessage TSTRING = NULL OUT
WITH EXECUTE AS OWNER
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dsp.Context_Verify @Context = @Context OUTPUT, @ProcId = @@PROCID;

    DECLARE @OwnerUserId INT;
    SELECT  @OwnerUserId = B.OwnerUserId
      FROM  dbo.Batch AS B
     WHERE  B.BatchId = @BatchId;

    -- Validating
    EXEC dbo.Context_SimpleCheckAccess @Context = @Context OUTPUT, @UserId = @OwnerUserId;

    -- Get Pagination Params
    DECLARE @RecordCount INT;
    DECLARE @RecordIndex INT;
    EXEC dsp.Context_Props @Context = @Context OUTPUT, @RecordCount = @RecordCount OUT, @RecordIndex = @RecordIndex OUT;

    -- prepare batch properties
    SELECT  @BatchName = B.BatchName, @UserId = B.OwnerUserId, @IsCanceled = B.IsCanceled, @IsCompleted = B.IsCompleted, @TotalItemCount = B.TotalItemCount,
        @ProcessedCount = B.ProccessedCount, @SuccessCount = B.SuccessedCount, @WarningCount = B.WarningCount, @ErrorCount = B.ErrorCount,
        @ErrorMessage = B.ErrorMessage
      FROM  dbo.Batch AS B
     WHERE  B.BatchId = @BatchId;

    -- GET result if requested
    IF (@RecordCount != 0)
    BEGIN
        SELECT  BI.BatchId, BI.BatchItemIndex, BI.BatchItemResultTypeId, --
            IIF(@IsIncludedCommandResult = 1, BI.CommandResult, NULL) AS CommandResult,
            IIF(@IsIncludedCommandSource = 1, BI.CommandSource, NULL) AS CommandSource
          FROM  dbo.BatchItem AS BI
         WHERE  BI.BatchId = @BatchId
         ORDER BY BI.BatchItemIndex OFFSET @RecordIndex ROWS FETCH NEXT @RecordCount ROWS ONLY
        OPTION (RECOMPILE);
    END;

END;