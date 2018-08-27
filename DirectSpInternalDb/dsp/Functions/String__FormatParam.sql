CREATE FUNCTION [dsp].[String_$FormatParam] (@Param TSTRING)
RETURNS TSTRING
AS
BEGIN
	-- return nothing has been set
	IF (dsp.Param_IsSetString(@Param) = 0)
		RETURN '<notset>';

	-- set <null> string for NULL to indicate the value is null
	RETURN ISNULL(@Param, '<null>');
END;