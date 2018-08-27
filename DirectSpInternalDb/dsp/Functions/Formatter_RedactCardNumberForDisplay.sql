CREATE FUNCTION [dsp].[Formatter_RedactCardNumberForDisplay] (@CardNumber TSTRING,
	@IsRTL BIT)
RETURNS TSTRING
AS
BEGIN
	SET @CardNumber = dsp.Formatter_FormatString(@CardNumber);
	RETURN FORMAT(CAST(RIGHT(@CardNumber, 4) AS INT), IIF(@IsRTL = 0, '****-****-****-000#', '000#-****-****-****'));
END;