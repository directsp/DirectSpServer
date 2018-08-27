
-- Create Procedure RecreatePermissionFunctions
CREATE PROC [dsp].[Init_RecreateEnumFunctions]
	@SchemaName TSTRING, @TableSchemaName TSTRING = 'dbo', @TableName TSTRING, @KeyColumnName TSTRING, @TextColumnName TSTRING, @FunctionBody TSTRING = NULL, @FunctionNamePostfix TSTRING = NULL
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @KeyColumnValue TSTRING;
	DECLARE @TextColumnValue TSTRING;
	DECLARE @Sql TSTRING;
	SET @FunctionNamePostfix = ISNULL(@FunctionNamePostfix, '');

	-- Set default function body
	IF (@FunctionBody IS NULL)
	BEGIN
		SET @FunctionBody = '
CREATE FUNCTION @SchemaName.@TextColumnValue@FunctionNamePostfix()
RETURNS INT WITH SCHEMABINDING
AS
BEGIN
	RETURN @KeyColumnValue;  
END
'	;
	END;
	SET @FunctionBody = REPLACE(@FunctionBody, '@SchemaName', @SchemaName);
	SET @FunctionBody = REPLACE(@FunctionBody, '@FunctionNamePostfix', @FunctionNamePostfix);

	-- Drop const Functions
	EXEC dsp.Schema_DropObjects @SchemaName = @SchemaName, @DropFunctions = 1;

	CREATE TABLE #EnumIdName (ObjectId NVARCHAR(/*Ignore code quality*/ 500),
		ObjectName NVARCHAR(/*Ignore code quality*/ 500));
	SET @Sql = 'INSERT INTO #EnumIdName (ObjectId, ObjectName) SELECT @KeyColumnName, @TextColumnName FROM @TableSchemaName.@TableName';
	SET @Sql = REPLACE(@Sql, '@TableSchemaName', @TableSchemaName);
	SET @Sql = REPLACE(@Sql, '@TableName', @TableName);
	SET @Sql = REPLACE(@Sql, '@KeyColumnName', @KeyColumnName);
	SET @Sql = REPLACE(@Sql, '@TextColumnName', @TextColumnName);
	EXEC (@Sql);

	-- Recreate Permissions Functions
	DECLARE LocalCursor CURSOR LOCAL FAST_FORWARD READ_ONLY FOR SELECT E.ObjectId, E.ObjectName FROM #EnumIdName AS E;
	OPEN LocalCursor;
	WHILE (1 = 1)
	BEGIN
		FETCH NEXT FROM LocalCursor
		INTO @KeyColumnValue, @TextColumnValue;
		IF (@@FETCH_STATUS <> 0)
			BREAK;

		SET @Sql = @FunctionBody;

		SET @Sql = REPLACE(@Sql, '@TextColumnValue', @TextColumnValue);
		SET @Sql = REPLACE(@Sql, '@KeyColumnValue', @KeyColumnValue);

		EXEC (@Sql);
	END;
	CLOSE LocalCursor;
	DEALLOCATE LocalCursor;

	DROP TABLE #EnumIdName;

END;