CREATE FUNCTION [dsp].[Metadata_ExtendedPropertyValueOfSchema] (@SchemaName TSTRING,
	@ExtendedPropertyName TSTRING)
RETURNS SQL_VARIANT
AS
BEGIN
	DECLARE @Value SQL_VARIANT;

	SELECT	@Value = value
	FROM	sys.fn_listextendedproperty(NULL, 'SCHEMA', NULL, NULL, NULL, NULL, NULL)
	WHERE	objname = @SchemaName AND	name = @ExtendedPropertyName;

	RETURN @Value;
END;