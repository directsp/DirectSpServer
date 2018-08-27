
-- Create Procedure RecreateStringFunctions
CREATE PROC [dsp].[Init_$RecreateStringFunctions]
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @FunctionBody TSTRING =
		'
CREATE FUNCTION @SchemaName.@KeyColumnValue() 
RETURNS TSTRING
AS 
BEGIN
	RETURN dsp.StringTable_Value(''@KeyColumnValue'');
END';

	EXEC dsp.Init_RecreateEnumFunctions @SchemaName = 'str', @TableSchemaName = 'dsp', @TableName = 'StringTable', @KeyColumnName = 'StringId', @TextColumnName = 'StringValue', @FunctionBody = @FunctionBody;
END;