CREATE FUNCTION [dsp].[Log_FormatMessage2] (@ProcId INT,
	@Message TSTRING,
	@Elipsis BIT = 0,
	@Param0 TSTRING = '<notset>',
	@Param1 TSTRING = '<notset>',
	@Param2 TSTRING = '<notset>',
	@Param3 TSTRING = '<notset>',
	@Param4 TSTRING = '<notset>',
	@Param5 TSTRING = '<notset>')
RETURNS TSTRING
AS
BEGIN
	SET @Elipsis = ISNULL(@Elipsis, 0);

	-- Validate Params
	SET @Param0 = dsp.Formatter_FormatParam(@Param0);
	SET @Param1 = dsp.Formatter_FormatParam(@Param1);
	SET @Param2 = dsp.Formatter_FormatParam(@Param2);
	SET @Param3 = dsp.Formatter_FormatParam(@Param3);
	SET @Param4 = dsp.Formatter_FormatParam(@Param4);
	SET @Param5 = dsp.Formatter_FormatParam(@Param5);

	-- Replace Message
	SET @Message = dsp.Formatter_FormatString(@Message);
	SET @Message = REPLACE(@Message, '{0}', @Param0);
	SET @Message = REPLACE(@Message, '{1}', @Param1);
	SET @Message = REPLACE(@Message, '{2}', @Param2);
	SET @Message = REPLACE(@Message, '{3}', @Param3);
	SET @Message = REPLACE(@Message, '{4}', @Param4);
	SET @Message = REPLACE(@Message, '{5}', @Param5);

	IF (@Elipsis = 1)
	BEGIN
		DECLARE @LastString TSTRING = SUBSTRING(@Message, LEN(@Message), 1);
		IF (@LastString NOT LIKE '[.?!:>;]')
			SET @Message = @Message + ' ...';
	END;

	-- Set Schema and ProcName
	DECLARE @ProcName TSTRING = ISNULL(OBJECT_NAME(@ProcId), '(NoSP)');
	DECLARE @SchemaName TSTRING = OBJECT_SCHEMA_NAME(@ProcId);
	IF (@SchemaName IS NOT NULL)
		SET @ProcName = @SchemaName + '.' + @ProcName;

	-- Format message 
	DECLARE @msg TSTRING = @ProcName + '> ' + @Message;
	RETURN dsp.String_ReplaceEnter(@msg);
END;




















