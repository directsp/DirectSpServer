CREATE PROCEDURE [tCodeQuality].[test Declare generic types INT, TSTRING]
AS
BEGIN
	DECLARE @msg TSTRING;
	DECLARE @ProcedureName TSTRING;

	-- Getting list all procedures with pagination
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Getting list all procedures with api schema';
	DECLARE @t TABLE (SchemaName TSTRING,
		ProcedureName TSTRING,
		Script TBIGSTRING);
	INSERT INTO @t
	SELECT	PD.SchemaName, PD.ObjectName, PD.Script
	FROM	dsp.Metadata_ProceduresDefination() AS PD
	WHERE	PD.ObjectName NOT IN ('Convert_ToString', 'Convert_ToSqlvariant', 'CRYPT_PBKDF2_VARBINARY_SHA512');

	-- Looking for "tinyint and smallint" phrase
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Looking for "tinyint and smallint" phrase';
	SET @ProcedureName = NULL;
	SELECT	@ProcedureName = SchemaName + '.' + ProcedureName
	FROM	@t
	WHERE	(SchemaName IN ( 'dbo', 'api', 'dsp' )) AND (CHARINDEX('tinyint', Script) > 0 OR	CHARINDEX('smallint', Script) > 0);
	IF (@ProcedureName IS NOT NULL)
	BEGIN
		SET @msg = 'Code should not contains SMALLINT or TINYINT in procedure: ' + @ProcedureName;
		EXEC tSQLt.Fail @Message0 = @msg;
	END;

	-- Looking for "NVARCHAR(MAX)" phrase
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Looking for "NVARCHAR(MAX)" phrase';
	SET @ProcedureName = NULL;
	SELECT	@ProcedureName = SchemaName + '.' + ProcedureName
	FROM	@t
	WHERE	(SchemaName IN ( 'dbo', 'api', 'dsp' )) AND (Script LIKE '%VARCHAR([0-9]%' OR Script LIKE '%VARCHAR(MAX)%');

	IF (@ProcedureName IS NOT NULL)
	BEGIN
		SET @msg = 'Code should not contains NVARCHAR(XX) in procedure: ' + @ProcedureName;
		EXEC tSQLt.Fail @Message0 = @msg;
	END;

END;