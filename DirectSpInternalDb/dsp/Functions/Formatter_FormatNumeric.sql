

CREATE FUNCTION [dsp].[Formatter_FormatNumeric] (@NumberStr TSTRING)
RETURNS TSTRING
AS
BEGIN
	RETURN REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(@NumberStr, '*', ''), '-', ''), '_', ''), '/', ''), ' ', ''), '#', '');
END;