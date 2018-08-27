CREATE	FUNCTION [dsp].[DateTime_FormatInterval] (@Second BIGINT,
	@Format TSTRING)
RETURNS TSTRING
AS
BEGIN
	-- Set default format
	SET @Format = ISNULL(@Format, N'h:m:s');
	RETURN dsp.DateTime_FormatIntervalMillisecond(@Second * 1000, @Format);
END;




