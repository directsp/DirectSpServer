
CREATE PROCEDURE [tCodeQuality].[test Description must be nvarchar(max)]
AS
BEGIN
	DECLARE @Msg TSTRING;
	SELECT	@Msg = QUOTENAME(SCHEMA_NAME(tb.schema_id)) + '.' + QUOTENAME(OBJECT_NAME(tb.object_id))
	FROM		sys.columns C
			INNER JOIN sys.tables tb ON tb.object_id = C.object_id
			INNER JOIN sys.types T ON C.system_type_id = T.user_type_id
	WHERE	tb.is_ms_shipped = 0
			AND C.name LIKE N'%Description%'
			AND (T.name != 'nvarchar'
				OR	C.max_length != -1);

	IF (@Msg IS NOT NULL)
	BEGIN
		SET @Msg = '"Description and type TSTRING" was not found in table ' + @Msg;
		EXEC tSQLt.Fail @Message0 = @Msg;
	END;
END;