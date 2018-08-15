CREATE PROCEDURE [tCodeQuality].[test Relations for ForeignKeys]
AS
BEGIN

	DECLARE @TableName TSTRING;
	DECLARE @ColumnName TSTRING;
	DECLARE @major_id INT
	DECLARE @minor_id INT

	DECLARE ForeignKeysCusrsor CURSOR LOCAL FAST_FORWARD READ_ONLY FOR
	SELECT	tables.name, columns.name, tables.object_id AS major_id, columns.column_id AS minor_id
	FROM	sys.tables
			INNER JOIN sys.schemas ON schemas.schema_id = tables.schema_id
			INNER JOIN sys.columns ON columns.object_id = tables.object_id			
	WHERE	columns.name LIKE N'%Id%'
			AND schemas.name = 'dbo'
			-- to do
			-- Must be remove from query
			AND tables.name != 'sysdiagrams'			
			AND columns.name != tables.name + 'Id'						
	ORDER BY tables.name;

	OPEN ForeignKeysCusrsor;

	WHILE (1=1)
	BEGIN
		FETCH NEXT FROM ForeignKeysCusrsor
		INTO @TableName, @ColumnName, @major_id, @minor_id;
		IF (@@FETCH_STATUS <> 0)
			BREAK;

		DECLARE @Description TSTRING = NULL
		SELECT @Description = dsp.Convert_ToString(value)
		FROM sys.extended_properties AS ep
		WHERE ep.name = 'MS_Description' AND ep.major_id = @major_id AND ep.minor_id = @minor_id		
		SET @Description = LOWER(ISNULL(@Description, ''))

		-- ignore /NoFk column
		IF (CHARINDEX('/nofk', @Description) > 0)
		BEGIN
			EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = '**** NoFk: {0}.{1}', @Param0 = @TableName, @Param1 = @ColumnName
			CONTINUE;
		END
		    
		
		IF NOT EXISTS (SELECT 1
							FROM sys.foreign_keys AS f
								INNER JOIN sys.foreign_key_columns AS fc ON f.object_id = fc.constraint_object_id
						WHERE	(	OBJECT_NAME(f.parent_object_id) = @TableName
									OR	OBJECT_NAME(f.referenced_object_id) = @TableName)
								AND (	COL_NAME(fc.parent_object_id, fc.parent_column_id) = @ColumnName
										OR COL_NAME(fc.referenced_object_id, fc.referenced_column_id) = @ColumnName))
		BEGIN
			DECLARE @msg TSTRING = 'Properly relation does not exists for "' + @TableName + '.' + @ColumnName + '"';
			EXEC tSQLt.Fail @Message0 = @msg;
		END;

		IF EXISTS (SELECT 1
						FROM sys.foreign_keys AS f
							INNER JOIN sys.foreign_key_columns AS fc ON f.object_id = fc.constraint_object_id
					WHERE	(	OBJECT_NAME(f.parent_object_id) = @TableName
								OR	OBJECT_NAME(f.referenced_object_id) = @TableName)
							AND (	COL_NAME(fc.parent_object_id, fc.parent_column_id) = @ColumnName
									OR COL_NAME(fc.referenced_object_id, fc.referenced_column_id) = @ColumnName)
							AND f.name NOT LIKE N'%Id' COLLATE SQL_Latin1_General_CP1_CS_AS)
		BEGIN
			SET @msg = 'Properly RelatioName does not correct for "' + @TableName + '.' + @ColumnName + '"';
			EXEC tSQLt.Fail @Message0 = @msg;
		END;
	
	END;
	CLOSE ForeignKeysCusrsor;
	DEALLOCATE ForeignKeysCusrsor;
END;