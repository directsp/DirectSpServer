CREATE FUNCTION [dsp].[Formatter_FormatNationalNumber] (
	@NationalNumber TSTRING)
RETURNS TSTRING
AS
BEGIN
	SET @NationalNumber = REPLACE(dsp.Formatter_FormatString(@NationalNumber), '-', '');
	RETURN IIF(ISNUMERIC(@NationalNumber) = 1 AND LEN(@NationalNumber) = 10, @NationalNumber, NULL);
END;