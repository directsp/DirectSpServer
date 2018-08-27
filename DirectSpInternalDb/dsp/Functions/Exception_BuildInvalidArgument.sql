CREATE	FUNCTION [dsp].[Exception_BuildInvalidArgument] (@ProcId INT,
	@ArgumentName TSTRING,
	@ArgumentValue TSTRING,
	@Message TSTRING = NULL)
RETURNS TSTRING
AS
BEGIN
	DECLARE @ArgMessage TSTRING;
	EXEC @ArgMessage = dsp.Formatter_FormatMessage @Message = N'{"ArgumentName": "{0}", "ArgumentValue": "{1}"}', @Param0 = @ArgumentName,
		@Param1 = @ArgumentValue;

	IF (@Message IS NOT NULL)
		SET @ArgMessage = JSON_MODIFY(@ArgMessage, '$.Message', @Message);

	DECLARE @ExceptionId INT = 55011; /* err.InvalidArgumentId() */
	RETURN dsp.Exception_BuildMessage(@ProcId, @ExceptionId, @ArgMessage);
END;




