CREATE FUNCTION [dsp].[Exception_InvalidArgumentName] (
	@ErrorMessage TSTRING
)
RETURNS TSTRING
AS
BEGIN
	RETURN JSON_VALUE(JSON_VALUE(@ErrorMessage, '$.errorMessage'), '$.ArgumentName');
END;