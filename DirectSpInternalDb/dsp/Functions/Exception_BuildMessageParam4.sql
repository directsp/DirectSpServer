CREATE	FUNCTION [dsp].[Exception_BuildMessageParam4] (@ProcId INT,
	@ExceptionId INT,
	@Message TSTRING = NULL,
	@Param0 TSTRING = '<notset>',
	@Param1 TSTRING = '<notset>',
	@Param2 TSTRING = '<notset>',
	@Param3 TSTRING = '<notset>')
RETURNS TJSON
AS
BEGIN
	-- get exception name and detail
	DECLARE @Description TSTRING;
	DECLARE @ExceptionName TSTRING;

	SELECT	@Description = Description, @ExceptionName = ExceptionName
	FROM	dsp.Exception
	WHERE	ExceptionId = @ExceptionId;

	-- validate exception Id
	IF (@ExceptionName IS NULL)
	BEGIN
		SET @Message = 'Inavlid AppExceptionId; ExceptionId: {0}';
		SET @Param0 = @ExceptionId;
		SET @ExceptionId = 55001;
	END;

	-- Replace Message
	EXEC @Message = dsp.Formatter_FormatMessage @Message = @Message, @Param0 = @Param0, @Param1 = @Param1, @Param2 = @Param2, @Param3 = @Param3;

	-- generate exception
	DECLARE @Exception TJSON = '{}';
	SET @Exception = JSON_MODIFY(@Exception, '$.errorId', @ExceptionId);
	SET @Exception = JSON_MODIFY(@Exception, '$.errorName', @ExceptionName);
	IF (@Description IS NOT NULL)
		SET @Exception = JSON_MODIFY(@Exception, '$.errorDescription', @Description);
	IF (@Description IS NOT NULL)
		SET @Exception = JSON_MODIFY(@Exception, '$.errorDescription', @Description);
	IF (@Message IS NOT NULL)
		SET @Exception = JSON_MODIFY(@Exception, '$.errorMessage', @Message);

	-- Set Schema and ProcName
	IF (@ProcId IS NOT NULL)
	BEGIN
		DECLARE @ProcName TSTRING = ISNULL(OBJECT_NAME(@ProcId), '(NoSP)');
		DECLARE @SchemaName TSTRING = OBJECT_SCHEMA_NAME(@ProcId);
		IF (@SchemaName IS NOT NULL)
			SET @ProcName = @SchemaName + '.' + @ProcName;
		SET @Exception = JSON_MODIFY(@Exception, '$.errorProcName', @ProcName);
	END;

	RETURN @Exception;
END;


