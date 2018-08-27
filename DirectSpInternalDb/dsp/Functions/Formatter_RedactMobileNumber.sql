CREATE FUNCTION [dsp].[Formatter_RedactMobileNumber] (@MobileNumber TSTRING)
RETURNS TSTRING
BEGIN
	SET @MobileNumber = dsp.Formatter_FormatMobileNumber(@MobileNumber);
	IF (@MobileNumber IS NOT NULL)
		RETURN '*********' + SUBSTRING(@MobileNumber, LEN(@MobileNumber) - 1, 2);

	RETURN NULL;
END;