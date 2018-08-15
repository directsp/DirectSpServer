CREATE FUNCTION [dsp].[Formatter_FormatEmail] (
	@Email TSTRING)
RETURNS TSTRING
AS
BEGIN
	SET @Email = dsp.Formatter_FormatString(@Email);
	RETURN IIF(dsp.Validate_IsValidEmail(@Email) = 1, @Email, NULL);
END;