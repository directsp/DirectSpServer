/*
#MetaStart 
	{
		"DataAccessMode": "Write"
	} 
#MetaEnd 
*/
CREATE	PROCEDURE [api].[Batch_SetItemState]
	@Context TCONTEXT OUTPUT, @BatchId INT, @BatchItemIndexId INT, @BatchItemResultTypeId INT, @Result TSTRING
WITH EXECUTE AS OWNER
AS
BEGIN
	SET NOCOUNT ON;
	EXEC dsp.Context_Verify @Context = @Context OUTPUT, @ProcId = @@PROCID;

	-- Get old BatchItem ResultType
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Get old BatchItem ResultType';

	DECLARE @OldBatchItemResultTypeId INT;
	DECLARE @OwnerUserId INT;
	SELECT	@OldBatchItemResultTypeId = BI.BatchItemResultTypeId, @OwnerUserId = B.OwnerUserId
	FROM	dbo.BatchItem AS BI
			INNER JOIN dbo.Batch AS B ON B.BatchId = BI.BatchId
	WHERE	BI.BatchId = @BatchId AND	BI.BatchItemIndex = @BatchItemIndexId;

	-- Check access
	EXEC dbo.Context_SimpleCheckAccess @Context = @Context OUTPUT, @UserId = @OwnerUserId;

	-- Validate 
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'validate';
	IF (@BatchItemResultTypeId = const.BatchItemResultType_NotProccessed()) --
		EXEC err.ThrowInvalidOperation @ProcId = @@PROCID, @Message = 'Can not set NotProccessed ResultTypeId!';

	IF (@OldBatchItemResultTypeId != const.BatchItemResultType_NotProccessed())
		EXEC err.ThrowInvalidOperation @ProcId = @@PROCID, @Message = N'can not update item if  BatchItem ResultType is not NotProccessed!';

	DECLARE @TranCount INT = @@TRANCOUNT;
	IF (@TranCount = 0)
		BEGIN TRANSACTION;
	BEGIN TRY
		-- update BatchItem
		UPDATE	dbo.BatchItem
		SET BatchItemResultTypeId = @BatchItemResultTypeId, CommandResult = @Result
		WHERE	BatchId = @BatchId AND	BatchItemIndex = @BatchItemIndexId;

		-- update batch
		IF (@BatchItemResultTypeId = const.BatchItemResultType_Success())
		BEGIN
			UPDATE	dbo.Batch
			SET SuccessedCount = SuccessedCount + 1, ProccessedCount = ProccessedCount + 1
			WHERE	BatchId = @BatchId;
		END;
		ELSE IF (@BatchItemResultTypeId = const.BatchItemResultType_Warning())
		BEGIN
			UPDATE	dbo.Batch
			SET WarningCount = WarningCount + 1, ProccessedCount = ProccessedCount + 1
			WHERE	BatchId = @BatchId;
		END;
		ELSE IF (@BatchItemResultTypeId = const.BatchItemResultType_Error())
		BEGIN
			UPDATE	dbo.Batch
			SET ErrorCount = ErrorCount + 1, ProccessedCount = ProccessedCount + 1
			WHERE	BatchId = @BatchId;
		END;

		IF (@TranCount = 0) COMMIT;
	END TRY
	BEGIN CATCH
		IF (@TranCount = 0)
			ROLLBACK TRANSACTION;

		-- manage BatchItemResultIsTooLargeId
		IF (ERROR_NUMBER() = 8152)
		BEGIN
			DECLARE @BatchItemResultIsTooLargeId INT = err.BatchItemResultIsTooLargeId();
			DECLARE @Exception TJSON = dsp.Exception_BuildMessage(@@PROCID, @BatchItemResultIsTooLargeId, NULL);

			UPDATE	dbo.Batch
			SET ErrorMessage = @Exception, IsCanceled = 1
			WHERE	BatchId = @BatchId;

			EXEC dsp.ThrowException @Exception = @Exception;
		END;

		SET @TranCount = @TranCount; -- BUG: Throw can not be after end
		THROW;
	END CATCH;

END;

