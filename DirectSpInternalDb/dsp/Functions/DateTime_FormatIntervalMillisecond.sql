CREATE FUNCTION [dsp].[DateTime_FormatIntervalMillisecond] (@Millisecond BIGINT,
    @Format TSTRING)
RETURNS TSTRING
AS
BEGIN
	-- Set default format
	SET @Format = ISNULL(@Format, N'h:m:s.t')

	-- Set hour if needed
	IF (CHARINDEX('h', @Format) > 0)
	BEGIN
		DECLARE @Hours BIGINT = @Millisecond / (3600 * 1000);
		SET @Format = REPLACE(@Format, 'h', FORMAT(@Hours, '0#'))
		SET @Millisecond = @Millisecond - (@Hours * 3600 * 1000)
	END
	
	-- Set minute if needed
	IF (CHARINDEX('m', @Format) > 0)
	BEGIN
		DECLARE @Minutes BIGINT = @Millisecond / (60 * 1000);
		SET @Format = REPLACE(@Format, 'm', FORMAT(@Minutes, '0#'))
		SET @Millisecond = @Millisecond - (@Minutes * 60 * 1000)
	END

	-- Set minute if needed
	IF (CHARINDEX('s', @Format) > 0)
	BEGIN
		DECLARE @Seconds INT = @Millisecond / (1 * 1000);
		SET @Format = REPLACE(@Format, 's', FORMAT(@Seconds, '0#'))
		SET @Millisecond = @Millisecond - (@Seconds * 1 * 1000)
	END
    
	-- Set second if needed
	IF (CHARINDEX('t', @Format) > 0)
		SET @Format = REPLACE(@Format, 't', FORMAT(@Millisecond, '00#'))

    RETURN @Format
END;






