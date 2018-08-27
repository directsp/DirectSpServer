CREATE FUNCTION [dsp].[Formatter_RedactCardNumber] (@CardNumber TSTRING)
RETURNS TSTRING
AS
BEGIN
    SET @CardNumber = dsp.Formatter_FormatString(@CardNumber);
    RETURN LEFT(@CardNumber, 6) + REPLICATE('x', LEN(@CardNumber) - 6 - 4) + RIGHT(@CardNumber, 4);
END;



