CREATE PROCEDURE [dsp].[Lock_Create]
	@ObjectTypeName TSTRING, @ObjectName TSTRING = NULL, @IsTransactionMode BIT = 1, @LockId TSTRING = NULL OUT
AS
BEGIN
	SET @LockId = N'{}';
	SET @ObjectName = ISNULL(@ObjectName, '');
	SET @IsTransactionMode = ISNULL(@IsTransactionMode, 1);
	DECLARE @LockName TSTRING = @ObjectTypeName + @ObjectName;
	DECLARE @LockOwner TSTRING = IIF(@IsTransactionMode = 1, 'Transaction', 'Session');

	-- Getting Lock
	DECLARE @Result INT;
	EXEC @Result = sys.sp_getapplock @Resource = @LockName, @LockMode = 'Exclusive', @LockOwner = @LockOwner;

	-- throw error for error result
	IF (@Result < 0) --
		EXEC err.ThrowGeneralException @ProcId = @@PROCID, @Message = N'Get AppLock Error! ErrorNumber: {0}', @Param0 = @Result;

	SET @LockId = JSON_MODIFY(@LockId, '$.LockOwner', @LockOwner);
	SET @LockId = JSON_MODIFY(@LockId, '$.LockName', @LockName);
END;































