
-- to check if the @value has been set or not
-- String not set value: <notset>
-- Int not set value: -1
-- datetime not set value: '1753-01-01
CREATE FUNCTION [dsp].[Param_IsSetOrNotNullString] (
	@Value TSTRING
)
RETURNS BIT
AS
BEGIN
	RETURN IIF(@Value IS NULL OR dsp.Param_IsSetString(@Value) = 0, 0, 1);
END;