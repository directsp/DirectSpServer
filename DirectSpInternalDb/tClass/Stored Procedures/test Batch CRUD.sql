CREATE PROCEDURE [tClass].[test Batch CRUD]
AS
BEGIN
    -- Get System Context
    DECLARE @SystemContext TCONTEXT;
    EXEC dsp.Context_CreateSystem @SystemContext = @SystemContext OUTPUT;

    DECLARE @UserId INT = 1;
    DECLARE @SystemUserId INT = dsp.Setting_SystemUserId();
    DECLARE @BatchName TSTRING = 't_batch';
    DECLARE @BatchItemResultType_NotProccessed INT = const.BatchItemResultType_NotProccessed();
    DECLARE @BatchItemResultType_Success INT = const.BatchItemResultType_Success();
    DECLARE @BatchItemResultType_Warning INT = const.BatchItemResultType_Warning();
    DECLARE @BatchItemResultType_Error INT = const.BatchItemResultType_Error();
    DECLARE @BatchItemIndex_Success INT = 0;
    DECLARE @BatchItemIndex_Warning INT = 1;
    DECLARE @BatchItemIndex_Error INT = 2;
    DECLARE @BatchItemIndex_NotProccessed INT = 3;
    DECLARE @UserContext TCONTEXT 
	EXEC dsp.Context_Create @UserId = 2, @Context = @UserContext OUTPUT	

    DECLARE @Commands TBIGSTRING;
    SET @Commands =
        '[{"ApiName": "ApiName1", "Parameters": {"Param0": "paramValue0", "Param0": "paramValue1"}},
		  {"ApiName": "ApiName1", "Parameters": {"Param0": "paramValue0", "Param0": "paramValue1"}},
		  {"ApiName": "ApiName1", "Parameters": {"Param0": "paramValue0", "Param0": "paramValue1"}},
		  {"ApiName": "ApiName1", "Parameters": {"Param0": "paramValue0", "Param0": "paramValue1"}}]';

    -- Create Batch
    DECLARE @BatchId INT;
    DECLARE @BatchId1 INT;
    EXEC api.Batch_Create @Context = @SystemContext OUT, @BatchName = @BatchName, @Commands = @Commands, @UserId = @SystemUserId, @BatchId = @BatchId OUT;
    EXEC api.Batch_Create @Context = @SystemContext OUT, @Commands = @Commands, @UserId = @UserId, @BatchId = @BatchId1 OUTPUT;

    -- Set BatchItem Result
    EXEC api.Batch_SetItemState @Context = @SystemContext, @BatchId = @BatchId, @BatchItemIndexId = @BatchItemIndex_Success,
        @BatchItemResultTypeId = @BatchItemResultType_Success, @Result = 'ResultValueSuccess';
    EXEC api.Batch_SetItemState @Context = @SystemContext, @BatchId = @BatchId, @BatchItemIndexId = @BatchItemIndex_Warning,
        @BatchItemResultTypeId = @BatchItemResultType_Warning, @Result = 'ResultValueWarning';
    EXEC api.Batch_SetItemState @Context = @SystemContext, @BatchId = @BatchId, @BatchItemIndexId = @BatchItemIndex_Error,
        @BatchItemResultTypeId = @BatchItemResultType_Error, @Result = 'ResultValueError';

    -------------------------------------
    -- Checking: Batch_Props should throw AccessDeniedOrObjectNotExists Exception if batch does not belong to user
    -------------------------------------
    EXEC dsp.Log_Trace @ProcId = @@PROCID,
        @Message = N'Checking: Batch_Props should throw AccessDeniedOrObjectNotExists Exception if batch does not belong to user';

    SAVE TRANSACTION Test;

    BEGIN TRY
        EXEC api.Batch_Props @Context = @UserContext, @BatchId = @BatchId;
        EXEC tSQLt.Fail @Message0 = N'Batch_Props should throw AccessDeniedOrObjectNotExists Exception if batch does not belong to user';
    END TRY
    BEGIN CATCH
        IF (ERROR_NUMBER() != err.AccessDeniedOrObjectNotExistsId())
            THROW;
    END CATCH;

    ROLLBACK TRANSACTION Test;

    -------------------------------------
    -- Checking: Batch_Props should return inserted Batch properties
    -------------------------------------
    EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Checking: Batch_Props should return inserted Batch properties';

    SAVE TRANSACTION Test;

    DECLARE @ActualBatchName TSTRING;
    DECLARE @ActualIsCompleted BIT;
    DECLARE @ActualIsCanceled BIT;
    DECLARE @ActualTotalItemCount INT;
    DECLARE @ActualProcessedCount INT;
    DECLARE @ActualSuccessCount INT;
    DECLARE @ActualWarningCount INT;
    DECLARE @ActualErrorCount INT;
    EXEC api.Batch_Props @Context = @SystemContext, @BatchId = @BatchId, @BatchName = @ActualBatchName OUT, @TotalItemCount = @ActualTotalItemCount OUT,
        @ProcessedCount = @ActualProcessedCount OUT, @SuccessCount = @ActualSuccessCount OUT, @ErrorCount = @ActualErrorCount OUT,
        @WarningCount = @ActualWarningCount OUT, @IsCompleted = @ActualIsCompleted OUT, @IsCanceled = @ActualIsCanceled OUT;

    EXEC tSQLt.AssertEquals @Expected = 0, @Actual = @ActualIsCompleted, @Message = N'IsCompleted';
    EXEC tSQLt.AssertEquals @Expected = 0, @Actual = @ActualIsCanceled, @Message = N'IsCanceled';
    EXEC tSQLt.AssertEquals @Expected = 4, @Actual = @ActualTotalItemCount, @Message = N'TotalItemCount';
    EXEC tSQLt.AssertEquals @Expected = 3, @Actual = @ActualProcessedCount, @Message = N'ProcessedCount';
    EXEC tSQLt.AssertEquals @Expected = 1, @Actual = @ActualSuccessCount, @Message = N'SuccessCount';
    EXEC tSQLt.AssertEquals @Expected = 1, @Actual = @ActualWarningCount, @Message = N'WarningCount';
    EXEC tSQLt.AssertEquals @Expected = 1, @Actual = @ActualErrorCount, @Message = N'ErrorCount';

    ROLLBACK TRANSACTION Test;

    -------------------------------------
    -- Checking: Batch_Props should not return CommandResult if IsIncludedCommandResult is false
    -------------------------------------
    EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Checking: Batch_Props should not return CommandResult if IsIncludedCommandResult is false';

    SAVE TRANSACTION Test;

    DECLARE @BatchItemInfo ud_BatchItem;
    INSERT INTO @BatchItemInfo
    EXEC api.Batch_Props @Context = @SystemContext, @BatchId = @BatchId, @IsIncludedCommandResult = 0;

    IF EXISTS (   SELECT    1
                    FROM    @BatchItemInfo AS BII
                   WHERE BII.CommandResult IS NOT NULL) --
        EXEC tSQLt.Fail @Message0 = N'Batch_Props has returned CommandResult';

    ROLLBACK TRANSACTION Test;

    -------------------------------------
    -- Checking: Batch_Props should return CommandResult if IsIncludedCommandResult is true
    -------------------------------------
    EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Checking: Batch_Props should return CommandResult if IsIncludedCommandResult is true';

    SAVE TRANSACTION Test;

    DELETE @BatchItemInfo;
    INSERT INTO @BatchItemInfo
    EXEC api.Batch_Props @Context = @SystemContext, @BatchId = @BatchId, @IsIncludedCommandResult = 1;

    IF NOT EXISTS (   SELECT    1
                        FROM    @BatchItemInfo AS BII
                       WHERE BII.CommandResult IS NOT NULL) --
        EXEC tSQLt.Fail @Message0 = N'Batch_Props has not returned CommandResult';

    ROLLBACK TRANSACTION Test;

    -------------------------------------
    -- Checking: Batch_Props should not return CommandSource if IsIncludedCommandSource is false
    -------------------------------------
    EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Checking: Batch_Props should not return CommandSource if IsIncludedCommandSource is false';

    SAVE TRANSACTION Test;

    DELETE @BatchItemInfo;
    INSERT INTO @BatchItemInfo
    EXEC api.Batch_Props @Context = @SystemContext, @BatchId = @BatchId, @IsIncludedCommandSource = 0;

    IF EXISTS (   SELECT    1
                    FROM    @BatchItemInfo AS BII
                   WHERE BII.CommandSource IS NOT NULL) --
        EXEC tSQLt.Fail @Message0 = N'Batch_Props has returned CommandSource';

    ROLLBACK TRANSACTION Test;

    -------------------------------------
    -- Checking: Batch_Props should return CommandSource if IsIncludedCommandSource true
    -------------------------------------
    EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Checking: Batch_Props should return CommandSource if IsIncludedCommandSource true';

    SAVE TRANSACTION Test;

    DELETE @BatchItemInfo;
    INSERT INTO @BatchItemInfo
    EXEC api.Batch_Props @Context = @SystemContext, @BatchId = @BatchId, @IsIncludedCommandSource = 1;

    IF NOT EXISTS (   SELECT    1
                        FROM    @BatchItemInfo AS BII
                       WHERE BII.CommandSource IS NOT NULL) --
        EXEC tSQLt.Fail @Message0 = N'Batch_Props has not returned CommandSource';

    ROLLBACK TRANSACTION Test;

    -------------------------------------
    -- Checking: Batch_SetItemState can not set Result Type to NotProcess
    -------------------------------------
    EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Checking: Batch_SetItemState can not set Result Type to NotProcess';

    SAVE TRANSACTION Test;

    BEGIN TRY
        EXEC api.Batch_SetItemState @Context = @SystemContext, @BatchId = @BatchId, @BatchItemIndexId = @BatchItemIndex_NotProccessed,
            @BatchItemResultTypeId = @BatchItemResultType_NotProccessed, @Result = 'ResultValueNotProccessed';
        EXEC tSQLt.Fail @Message0 = N'Batch_SetItemState can not set Result Type to NotProcess';
    END TRY
    BEGIN CATCH
        IF (ERROR_NUMBER() != err.InvalidOperationId())
            THROW;
    END CATCH;

    ROLLBACK TRANSACTION Test;

    -------------------------------------
    -- Checking: Batch_SetItemState can not have big result
    -------------------------------------
    EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Checking: Batch_SetItemState can not have big result';

    SAVE TRANSACTION Test;

    DECLARE @BigResult TSTRING = REPLICATE('a', 2000);

    BEGIN TRY
        EXEC api.Batch_SetItemState @Context = @SystemContext, @BatchId = @BatchId, @BatchItemIndexId = @BatchItemIndex_NotProccessed, @Result = @BigResult,
            @BatchItemResultTypeId = @BatchItemResultType_Success;
        EXEC tSQLt.Fail @Message0 = N'Batch_SetItemState can not have big result';
    END TRY
    BEGIN CATCH
        IF (ERROR_NUMBER() != err.BatchItemResultIsTooLargeId())
            THROW;
    END CATCH;

    ROLLBACK TRANSACTION Test;

    -------------------------------------
    -- Checking: Batch_SetItemState should cancel the batch proccess when command return too large result
    -------------------------------------
    EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Checking: Batch_SetItemState should cancel the batch proccess when command return too large result';

    SAVE TRANSACTION Test;
    BEGIN TRY
        EXEC api.Batch_SetItemState @Context = @SystemContext, @BatchId = @BatchId, @BatchItemIndexId = @BatchItemIndex_NotProccessed, @Result = @BigResult,
            @BatchItemResultTypeId = @BatchItemResultType_Success;
        EXEC tSQLt.Fail @Message0 = N'Batch_SetItemState should throw BatchItemResultIsTooLarge exception!';
    END TRY
    BEGIN CATCH
        IF (ERROR_NUMBER() != err.BatchItemResultIsTooLargeId())
            THROW;
    END CATCH;

    DECLARE @ErrorMessage TSTRING;
    DECLARE @IsCanceled BIT;
    EXEC api.Batch_Props @Context = @SystemContext, @BatchId = @BatchId, @ErrorMessage = @ErrorMessage OUT, @IsCanceled = @IsCanceled OUT;
    EXEC tSQLt.AssertNotEquals @Expected = NULL, @Actual = @ErrorMessage, @Message = N'ErrorMessage';
    EXEC tSQLt.AssertEquals @Expected = 1, @Actual = @IsCanceled, @Message = N'IsCanceled';

    ROLLBACK TRANSACTION Test;

    -------------------------------------
    -- Checking: Batch_SetItemState should throw AccessDeniedOrObjectNoExist exception if batch had been deleted
    -------------------------------------
    EXEC dsp.Log_Trace @ProcId = @@PROCID,
        @Message = N'Checking: Batch_SetItemState should throw AccessDeniedOrObjectNoExist exception if batch had been deleted';

    SAVE TRANSACTION Test;

    EXEC api.Batch_Delete @Context = @SystemContext, @BatchId = @BatchId;

    BEGIN TRY
        EXEC api.Batch_SetItemState @Context = @SystemContext, @BatchId = @BatchId, @BatchItemIndexId = @BatchItemIndex_NotProccessed,
            @BatchItemResultTypeId = @BatchItemResultType_Success, @Result = 'ResultValueSuccessed';
        EXEC tSQLt.Fail @Message0 = N'Batch_SetItemState should throw AccessDeniedOrObjectNoExist exception if Batch had been deleted';
    END TRY
    BEGIN CATCH
        IF (ERROR_NUMBER() != err.AccessDeniedOrObjectNotExistsId())
            THROW;
    END CATCH;

    ROLLBACK TRANSACTION Test;

    -------------------------------------
    -- Checking: Batch_SetItemState should throw InvalidOperation exception current result is NotProccessed
    -------------------------------------
    EXEC dsp.Log_Trace @ProcId = @@PROCID,
        @Message = N'Checking: Batch_SetItemState should throw InvalidOperation exception current result is NotProccessed';

    SAVE TRANSACTION Test;

    BEGIN TRY
        EXEC api.Batch_SetItemState @Context = @SystemContext, @BatchId = @BatchId, @BatchItemIndexId = @BatchItemIndex_Success,
            @BatchItemResultTypeId = @BatchItemResultType_Error, @Result = 'BatchItemResultSuccess';
        EXEC tSQLt.Fail @Message0 = N'Batch_SetItemState should throw InvalidOperation exception current result is NotProccessed';
    END TRY
    BEGIN CATCH
        IF (ERROR_NUMBER() != err.InvalidOperationId())
            THROW;
    END CATCH;

    ROLLBACK TRANSACTION Test;


    -------------------------------------
    -- Checking: Batch_Props should throw AccessDeniedOrObjectNoExist exception if batch had been deleted
    -------------------------------------
    EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Checking: Batch_Props should throw AccessDeniedOrObjectNoExist exception if batch had been deleted';

    SAVE TRANSACTION Test;

    EXEC api.Batch_Delete @Context = @SystemContext, @BatchId = @BatchId;

    BEGIN TRY
        EXEC api.Batch_Props @Context = @SystemContext, @BatchId = @BatchId;
        EXEC tSQLt.Fail @Message0 = N'Batch_Props should throw AccessDeniedOrObjectNoExist exception if Batch had been deleted';
    END TRY
    BEGIN CATCH
        IF (ERROR_NUMBER() != err.AccessDeniedOrObjectNotExistsId())
            THROW;
    END CATCH;

    ROLLBACK TRANSACTION Test;

    -------------------------------------
    -- Checking: Batch_Cancel should set IsCancel is true
    -------------------------------------
    EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Checking: Batch_Cancel should set IsCancel = true';

    SAVE TRANSACTION Test;

    EXEC api.Batch_Cancel @Context = @SystemContext, @BatchId = @BatchId;
    EXEC api.Batch_Props @Context = @SystemContext, @BatchId = @BatchId, @IsCanceled = @ActualIsCanceled OUT;
    EXEC tSQLt.AssertEquals @Expected = 1, @Actual = @ActualIsCanceled, @Message = N'IsCanceled';

    ROLLBACK TRANSACTION Test;

    -------------------------------------
    -- Checking: Batch_SetItemState should throw exception if batch had been canceled
    -------------------------------------
    EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Checking: Batch_Cancel should set IsCancel = true';

    SAVE TRANSACTION Test;

    EXEC api.Batch_Cancel @Context = @SystemContext, @BatchId = @BatchId;

    BEGIN TRY
        EXEC api.Batch_SetItemState @Context = @SystemContext, @BatchId = @BatchId, @BatchItemIndexId = @BatchItemIndex_NotProccessed,
            @BatchItemResultTypeId = @BatchItemIndex_Success, @Result = 'ResultValueSuccessed';
    END TRY
    BEGIN CATCH

    END CATCH;

    ROLLBACK TRANSACTION Test;

    -------------------------------------
    -- Checking: Batch_Cancel should throw AccessDeniedOrObjectNoExist exception if Batch had been deleted
    -------------------------------------
    EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Checking: Batch_Cancel should throw AccessDeniedOrObjectNoExist exception if Batch had been deleted';

    SAVE TRANSACTION Test;

    EXEC api.Batch_Delete @Context = @SystemContext, @BatchId = @BatchId;

    BEGIN TRY
        EXEC api.Batch_Cancel @Context = @SystemContext, @BatchId = @BatchId;
        EXEC tSQLt.Fail @Message0 = N'Batch_Cancel should throw AccessDeniedOrObjectNoExist exception if Batch had been deleted';
    END TRY
    BEGIN CATCH
        IF (ERROR_NUMBER() != err.AccessDeniedOrObjectNotExistsId())
            THROW;
    END CATCH;

    ROLLBACK TRANSACTION Test;

    -------------------------------------
    -- Checking: Batch_Delete should throw AccessDeniedOrObjectNoExist exception if Batch belong to another user
    -------------------------------------
    EXEC dsp.Log_Trace @ProcId = @@PROCID,
        @Message = N'Checking: Batch_Delete should throw AccessDeniedOrObjectNoExist exception if Batch belong to another user';

    SAVE TRANSACTION Test;

    BEGIN TRY
        EXEC api.Batch_Delete @Context = @UserContext, @BatchId = @BatchId1;
        EXEC tSQLt.Fail @Message0 = N'Batch_Delete should throw AccessDeniedOrObjectNoExist exception if Batch belong to another user';
    END TRY
    BEGIN CATCH
        IF (ERROR_NUMBER() != err.AccessDeniedOrObjectNotExistsId())
            THROW;
    END CATCH;

    ROLLBACK TRANSACTION Test;

    -------------------------------------
    -- Checking: Batch_Cancel should throw AccessDeniedOrObjectNoExist exception if Batch belong to another user
    -------------------------------------
    EXEC dsp.Log_Trace @ProcId = @@PROCID,
        @Message = N'Checking: Batch_Cancel should throw AccessDeniedOrObjectNoExist exception if Batch belong to another user';

    SAVE TRANSACTION Test;

    BEGIN TRY
        EXEC api.Batch_Cancel @Context = @UserContext, @BatchId = @BatchId1;
        EXEC tSQLt.Fail @Message0 = N'Batch_Cancel should throw AccessDeniedOrObjectNoExist exception if Batch belong to another user';
    END TRY
    BEGIN CATCH
        IF (ERROR_NUMBER() != err.AccessDeniedOrObjectNotExistsId())
            THROW;
    END CATCH;

    ROLLBACK TRANSACTION Test;

    -------------------------------------
    -- Checking: Batch_SetItemState should throw AccessDeniedOrObjectNoExist exception if Batch belong to another user
    -------------------------------------
    EXEC dsp.Log_Trace @ProcId = @@PROCID,
        @Message = N'Checking: Batch_SetItemState should throw AccessDeniedOrObjectNoExist exception if Batch belong to another user';

    SAVE TRANSACTION Test;

    BEGIN TRY
        EXEC api.Batch_SetItemState @Context = @UserContext, @BatchId = @BatchId1, @BatchItemIndexId = @BatchItemIndex_NotProccessed,
            @BatchItemResultTypeId = @BatchItemResultType_Success, @Result = '@Result';
        EXEC tSQLt.Fail @Message0 = N'Batch_SetItemState should throw AccessDeniedOrObjectNoExist exception if Batch belong to another user';
    END TRY
    BEGIN CATCH
        IF (ERROR_NUMBER() != err.AccessDeniedOrObjectNotExistsId())
            THROW;
    END CATCH;

    ROLLBACK TRANSACTION Test;
END;










