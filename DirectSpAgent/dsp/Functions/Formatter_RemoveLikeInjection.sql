CREATE FUNCTION [dsp].[Formatter_RemoveLikeInjection] (
	@Value TSTRING)
RETURNS TSTRING
AS
BEGIN
	SET @Value = dsp.Formatter_FormatString(@Value);
	SET @Value = REPLACE(@Value, '%', '');
	SET @Value = REPLACE(@Value, '[', '');
	SET @Value = REPLACE(@Value, '_', '[_]');

	RETURN @Value;
END;