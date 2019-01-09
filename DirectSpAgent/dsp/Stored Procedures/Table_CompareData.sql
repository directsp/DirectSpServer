CREATE PROC [dsp].[Table_CompareData]
	@SourceSchemaName TSTRING = NULL, @SourceTableName TSTRING = NULL, @DestinationTableName TSTRING, @DestinationSchemaName TSTRING = 'dbo',
	@IsWithDelete BIT = 1
AS
BEGIN
	SET @SourceTableName = ISNULL(@SourceSchemaName, '#' + @DestinationTableName);

	DECLARE @SourceTable TSTRING = @SourceTableName;
	IF (dsp.Formatter_FormatString(@SourceSchemaName) IS NOT NULL)
		SET @SourceTable = @SourceSchemaName + '.' + @SourceTableName;

	DECLARE @DestinationTable TSTRING = @DestinationTableName;
	IF (dsp.Formatter_FormatString(@DestinationSchemaName) IS NOT NULL)
		SET @DestinationTable = @DestinationSchemaName + '.' + @DestinationTableName;

	-- find primary key
	DECLARE @PrimaryKey TSTRING;
	SELECT	@PrimaryKey = COLUMN_NAME
	FROM	INFORMATION_SCHEMA.KEY_COLUMN_USAGE
	WHERE	OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + QUOTENAME(CONSTRAINT_NAME)), 'IsPrimaryKey') = 1 AND TABLE_NAME = @DestinationTableName AND
			TABLE_SCHEMA = @DestinationSchemaName;

	IF (@PrimaryKey IS NULL)
		EXEC err.ThrowGeneralException @ProcId = @@PROCID, @Message = 'Could not find Primary key for table {0}!', @Param0 = @DestinationTableName

	-- columns
	DECLARE @ColumnsForUpdate TSTRING =
			(	SELECT		',' + c.name + '=S.' + c.name
				FROM	sys.columns c
						INNER JOIN sys.objects o ON o.object_id = c.object_id
						LEFT JOIN sys.types t ON t.user_type_id = c.user_type_id
				WHERE	o.type = 'U' AND SCHEMA_NAME(o.schema_id) = @DestinationSchemaName AND	o.name = @DestinationTableName AND	c.is_identity = 0 AND
						c.name <> @PrimaryKey
				FOR XML PATH(''));

	SET @ColumnsForUpdate = STUFF(@ColumnsForUpdate, 1, 1, '');

		-- columns
	DECLARE @ColumnsForInsert TSTRING =
			(	SELECT		',' + c.name 
				FROM	sys.columns c
						INNER JOIN sys.objects o ON o.object_id = c.object_id
						LEFT JOIN sys.types t ON t.user_type_id = c.user_type_id
				WHERE	o.type = 'U' AND SCHEMA_NAME(o.schema_id) = @DestinationSchemaName AND	o.name = @DestinationTableName 	
				FOR XML PATH(''));

	SET @ColumnsForInsert = STUFF(@ColumnsForInsert, 1, 1, '');

	DECLARE @Sql TSTRING =
		'
		UPDATE @DestinationTable SET @ColumnsForUpdate FROM @SourceTable AS S WHERE S.@PrimaryKey=@DestinationTable.@PrimaryKey;

		INSERT @DestinationTable (@ColumnsForInsert) SELECT * FROM @SourceTable AS S WHERE S.@PrimaryKey NOT IN (SELECT @PrimaryKey FROM @DestinationTable AS S);

		IF (@IsWithDelete=1)
			DELETE @DestinationTable WHERE @PrimaryKey NOT IN (SELECT @PrimaryKey FROM @SourceTable AS S);
	';
	SET @Sql = REPLACE(@Sql, '@IsWithDelete', @IsWithDelete);
	SET @Sql = REPLACE(@Sql, '@SourceTable', @SourceTable);
	SET @Sql = REPLACE(@Sql, '@DestinationTable', @DestinationTable);
	SET @Sql = REPLACE(@Sql, '@PrimaryKey', @PrimaryKey);
	SET @Sql = REPLACE(@Sql, '@ColumnsForUpdate', @ColumnsForUpdate);
	SET @Sql = REPLACE(@Sql, '@ColumnsForInsert', @ColumnsForInsert);

	EXEC (@Sql);
END;