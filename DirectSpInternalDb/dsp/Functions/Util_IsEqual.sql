/*TO check if two input are equal*/
--null aware
CREATE FUNCTION [dsp].[Util_IsEqual] (
	@Value1 SQL_VARIANT,
	@Value2 SQL_VARIANT)
RETURNS BIT
AS
BEGIN
	RETURN IIF(@Value1 = @Value2 OR ISNULL(@Value1, @Value2) IS NULL, 1, 0);
END;