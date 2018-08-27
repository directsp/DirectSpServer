CREATE PROCEDURE [tCodeQuality].[test None of tests should have RETURN phrase]
AS
BEGIN

	-- find first tests with [RET URN] phrase 
	DECLARE @ObjectName TSTRING;
	
	SELECT	@ObjectName = S.name + '.' + O.name
	FROM	sys.objects AS O
			INNER JOIN sys.schemas AS S ON S.schema_id = O.schema_id
			INNER JOIN sys.extended_properties AS EP ON EP.major_id = S.schema_id
	WHERE	EP.name = 'tSQLt.TestClass' AND O.name LIKE 'test%' --
		AND CHARINDEX('RETURN', OBJECT_DEFINITION(O.object_id)) > 0 --
		AND O.name <> 'test None of tests should have RETURN phrase';

	DECLARE @ErrorMessage TSTRING = @ObjectName + ' has [RETURN] phrase';

	IF (@ObjectName IS NOT NULL) --
		EXEC tSQLt.Fail @Message0 = @ErrorMessage;



END;