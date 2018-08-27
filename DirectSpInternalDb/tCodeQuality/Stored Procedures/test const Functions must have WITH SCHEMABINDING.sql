CREATE PROCEDURE [tCodeQuality].[test const Functions must have WITH SCHEMABINDING]
AS
BEGIN

	-- Declaring pattern
	DECLARE @Pattern_Context TCONTEXT = dsp.String_RemoveWhitespaces('WITH SCHEMABINDING');

	DECLARE @msg TSTRING;
	DECLARE @FunctionName TSTRING;

	-- Getting list all procedures with pagination
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Getting list all functions with const schema';
	DECLARE @t TABLE (SchemaName TSTRING,
		FunctionName TSTRING,
		Script TBIGSTRING);
	INSERT INTO @t
	SELECT	PD.SchemaName, PD.ObjectName, dsp.String_RemoveWhitespaces(PD.Script)
	FROM	dsp.Metadata_ProceduresDefination() AS PD
	WHERE	PD.SchemaName = 'const';

	-- Looking for "@Context TCONTEXT OUT" phrase
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Looking for "WITH SCHEMABINDING" phrase';
	SELECT	@FunctionName = SchemaName + '.' + FunctionName
	FROM	@t
	WHERE	CHARINDEX(@Pattern_Context, Script) < 1 AND CHARINDEX('TSTRING', Script) = 0;

	IF (@FunctionName IS NOT NULL)
	BEGIN
		SET @msg = '"WITH SCHEMABINDING" phrase was not found in Function: ' + @FunctionName;
		EXEC tSQLt.Fail @Message0 = @msg;
	END;
END;