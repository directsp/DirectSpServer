/*
#MetaStart 
	{
		"DataAccessMode": "Write"
	} 
#MetaEnd 
*/
CREATE PROCEDURE [api].[Batch_Create]
    @Context TCONTEXT OUTPUT, @Commands TBIGSTRING, @UserId INT, @BatchName TSTRING = NULL, @BatchId INT OUT
WITH EXECUTE AS OWNER
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dsp.Context_Verify @Context = @Context OUTPUT, @ProcId = @@PROCID;

    DECLARE @TranCount INT = @@TRANCOUNT;
    IF (@TranCount = 0)
        BEGIN TRANSACTION;

    BEGIN TRY
        -- Create Batch
        EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Create Batch';
        INSERT  dbo.Batch (BatchName, OwnerUserId)
        VALUES (@BatchName, @UserId);

        SET @BatchId = SCOPE_IDENTITY();

        -- Create Batch items
        EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Create Batch items';
        INSERT INTO dbo.BatchItem (BatchId, BatchItemIndex, CommandSource, BatchItemResultTypeId)
        SELECT  @BatchId, OJ.[key], OJ.value, const.BatchItemResultType_NotProccessed()
          FROM  OPENJSON(@Commands) AS OJ;

        WITH BatchTotalItem
            AS (SELECT  COUNT(1) AS TotalItemCount
                  FROM  dbo.BatchItem AS BI
                 WHERE  BI.BatchId = @BatchId)
        UPDATE  dbo.Batch
           SET  TotalItemCount = BTI.TotalItemCount
          FROM  BatchTotalItem AS BTI;

        IF (@TranCount = 0) COMMIT;
    END TRY
    BEGIN CATCH
        IF (@TranCount = 0)
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH;




END;


