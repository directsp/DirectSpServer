CREATE FUNCTION [dsp].[Formatter_FormatPostalCode] (@PostalCode TSTRING)
RETURNS TSTRING
AS
BEGIN
	SET @PostalCode = dsp.Formatter_FormatString(@PostalCode);
	RETURN IIF(ISNUMERIC(@PostalCode) = 1 AND LEN(@PostalCode) = 10, @PostalCode, NULL);
END;