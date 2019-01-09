CREATE FUNCTION [dsp].[Param_IsSetOrNotNull] (
	@value SQL_VARIANT
)
RETURNS BIT
AS
BEGIN
	RETURN IIF(@value IS NULL OR dsp.Param_IsSet(@value) = 0, 0, 1);
END;