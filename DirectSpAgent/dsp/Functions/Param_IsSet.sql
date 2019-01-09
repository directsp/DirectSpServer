CREATE	FUNCTION [dsp].[Param_IsSet] (@Value SQL_VARIANT)
RETURNS BIT
AS
BEGIN
	RETURN	dsp.Param_IsSetBase(@Value, 0);
END;