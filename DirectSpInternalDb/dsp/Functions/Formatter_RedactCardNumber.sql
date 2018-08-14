CREATE FUNCTION [dsp].[Formatter_RedactCardNumber] (@CardNumber TSTRING)
RETURNS TSTRING
AS
BEGIN
	SET @CardNumber = dsp.Formatter_FormatString(@CardNumber);
	RETURN RIGHT(@CardNumber, 4) + REPLICATE('*', LEN(@CardNumber) - 4);
END;


