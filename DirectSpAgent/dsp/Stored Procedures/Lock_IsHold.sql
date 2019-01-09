CREATE	PROCEDURE [dsp].[Lock_IsHold]
	@ObjectTypeName TSTRING, @ObjectName TSTRING = NULL, @IsTransactionMode BIT = 1, @IsLockHold BIT = NULL OUT
AS
BEGIN
	SET @ObjectName = ISNULL(@ObjectName, '');
	SET @IsTransactionMode = ISNULL(@IsTransactionMode, 0);
	DECLARE @LockName TSTRING = @ObjectTypeName + @ObjectName;
	DECLARE @LockOwner TSTRING = IIF(@IsTransactionMode = 1, 'Transaction', 'Session');

	-- Check that Session in Lock
	SET @IsLockHold = IIF(APPLOCK_TEST('public', @LockName, 'Exclusive', @LockOwner) = 1, 0, 1);
END;
































