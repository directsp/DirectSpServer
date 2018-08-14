
CREATE PROC [dsp].[ThrowInvalidArgument]
	@ProcId INT, @ArgumentName TSTRING, @ArgumentValue TSTRING, @Message TSTRING = NULL, @Param0 TSTRING = '<notset>',
	@Param1 TSTRING = '<notset>', @Param2 TSTRING = '<notset>', @Param3 TSTRING = '<notset>'
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ArgMessage TSTRING;
	EXEC @ArgMessage = dsp.Formatter_FormatMessage @Message = N'{"ArgumentName": "{0}", "ArgumentValue": "{1}"}', @Param0 = @ArgumentName, @Param1 = @ArgumentValue;
	
	IF (@Message IS NOT NULL)
		SET @ArgMessage = JSON_MODIFY(@ArgMessage, '$.Message', @Message);

	DECLARE @ExceptionId INT = 55011; /* err.InvalidArgumentId() */
	EXEC dsp.ThrowAppException @ProcId = @ProcId, @ExceptionId = @ExceptionId, @Message = @ArgMessage, @Param0 = @Param0, @Param1 = @Param1,
		@Param2 = @Param2, @Param3 = @Param3;
END;







