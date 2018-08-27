CREATE PROC [tCodeQuality].[test API must have Context TCONTEXT OUT]
AS
BEGIN

	-- Declearing pattern
	DECLARE @Pattern_Context TCONTEXT = dsp.String_RemoveWhitespaces('@Context TCONTEXT OUT');

	DECLARE @msg TSTRING;
	DECLARE @ProcedureName TSTRING;

	-- Getting list all procedures with pagination
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Getting list all procedures with api schema';
	DECLARE @t TABLE (SchemaName TSTRING,
		ProcedureName TSTRING,
		Script TBIGSTRING);
	INSERT INTO @t
	SELECT	PD.SchemaName, PD.ObjectName, dsp.String_RemoveWhitespaces(PD.Script)
	FROM	dsp.Metadata_ProceduresDefination() AS PD
	WHERE	PD.SchemaName = 'api';

	-- Looking for "@Context TCONTEXT OUT" phrase
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Looking for "@Context TCONTEXT OUT" phrase';
	SELECT	@ProcedureName = SchemaName + '.' + ProcedureName
	FROM	@t
	WHERE	CHARINDEX(@Pattern_Context, Script) < 1;

	IF (@ProcedureName IS NOT NULL)
	BEGIN
		SET @msg = '"@Context TCONTEXT OUT" phrase was not found in procedure: ' + @ProcedureName;
		EXEC tSQLt.Fail @Message0 = @msg;
	END;
END;