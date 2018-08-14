CREATE PROCEDURE [tCodeQuality].[test API must have WITH EXECUTE AS OWNER]
AS
BEGIN
	-- Declaring pattern
	DECLARE @Pattern_WithExecuteASOwner TSTRING = dsp.String_RemoveWhitespaces('WITH EXECUTE AS OWNER');
	DECLARE @Pattern_WithExecASOwner TSTRING = dsp.String_RemoveWhitespaces('WITH EXEC AS OWNER');
	DECLARE @msg TSTRING;
	DECLARE @ProcedureName TSTRING;

	-- Getting list all procedures with pagination
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Getting list all procedures with pagination';
	DECLARE @t TABLE (SchemaName TSTRING,
		ProcedureName TSTRING,
		Script TBIGSTRING);
	INSERT INTO @t
	SELECT	PD.SchemaName, PD.ObjectName AS ProcedureName, dsp.String_RemoveWhitespaces(PD.Script)
	FROM	dsp.Metadata_ProceduresDefination() AS PD
	WHERE	PD.SchemaName = 'api';

	-- Looking for "With Execute AS Owner" phrase
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Looking for "With Execute AS Owner" phrase';
	SELECT	@ProcedureName = SchemaName + '.' + ProcedureName
	FROM	@t
	WHERE	CHARINDEX(@Pattern_WithExecuteASOwner, Script) < 1 AND	CHARINDEX(@Pattern_WithExecASOwner, Script) < 1;

	IF (@ProcedureName IS NOT NULL)
	BEGIN
		SET @msg = '"With Execute AS Owner" phrase was not found in procedure: ' + @ProcedureName;
		EXEC tSQLt.Fail @Message0 = @msg;
	END;
END;







