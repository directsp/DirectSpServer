
--todo: refactor to String_Format
CREATE FUNCTION [dsp].[Formatter_FormatMessage] (
	@Message TSTRING,
	@Param0 TSTRING = '<notset>',
	@Param1 TSTRING = '<notset>',
	@Param2 TSTRING = '<notset>',
	@Param3 TSTRING = '<notset>')
RETURNS TSTRING
AS
BEGIN

	-- Validate Params
	SET @Param0 = dsp.Formatter_FormatParam(@Param0);
	SET @Param1 = dsp.Formatter_FormatParam(@Param1);
	SET @Param2 = dsp.Formatter_FormatParam(@Param2);
	SET @Param3 = dsp.Formatter_FormatParam(@Param3);

	-- Replace Message
	SET @Message = dsp.Formatter_FormatString(@Message);
	SET @Message = REPLACE(@Message, '{0}', @Param0);
	SET @Message = REPLACE(@Message, '{1}', @Param1);
	SET @Message = REPLACE(@Message, '{2}', @Param2);
	SET @Message = REPLACE(@Message, '{3}', @Param3);

	RETURN @Message;

END;






