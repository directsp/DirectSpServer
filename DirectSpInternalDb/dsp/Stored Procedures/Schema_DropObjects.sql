-- @ObjectType Can be 'FN' or 'P'
CREATE PROC [dsp].[Schema_DropObjects]
	@SchemaName TSTRING, @DropFunctions BIT = 0, @DropProcedures BIT = 0
AS
BEGIN
	SET NOCOUNT ON;
	SET @DropFunctions = ISNULL(@DropFunctions, 0);
	SET @DropProcedures = ISNULL(@DropProcedures, 0);

	DECLARE @ObjectName TSTRING;
	DECLARE @ObjectType TSTRING;
	DECLARE @DdlText TSTRING;

	--Drop String Functions
	DECLARE DropCursor CURSOR LOCAL FAST_FORWARD READ_ONLY FOR
	SELECT	O.name, O.type
	FROM	sys.objects AS O
			INNER JOIN sys.schemas AS S ON S.schema_id = O.schema_id
	WHERE	S.name = @SchemaName
			AND O.type IN ( 'FN', 'P', 'IF', 'TF' );

	OPEN DropCursor;
	FETCH NEXT FROM DropCursor
	INTO @ObjectName, @ObjectType;
	WHILE (@@FETCH_STATUS = 0)
	BEGIN
		IF (@DropFunctions = 1 AND @ObjectType IN ('FN', 'IF', 'TF'))
		BEGIN
			SET @DdlText = 'DROP FUNCTION ' + @SchemaName + '.' + @ObjectName;
			EXEC sys.sp_executesql @DdlText;
		END;

		IF (@DropProcedures = 1 AND @ObjectType = 'P')
		BEGIN
			SET @DdlText = 'DROP PROCEDURE ' + @SchemaName + '.' + @ObjectName;
			EXEC sys.sp_executesql @DdlText;
		END;

		FETCH NEXT FROM DropCursor
		INTO @ObjectName, @ObjectType;
	END;
	CLOSE DropCursor;
	DEALLOCATE DropCursor;

END;