CREATE PROC [dsp].[ValidateFunctionNameAndReturnValue]
	@Definition TBIGSTRING OUT, @ObjectName TSTRING, @Id INT OUT, @Value INT OUT, @FunctionName TSTRING OUT
AS
BEGIN
	DECLARE @StartStrCharIndex INT;
	DECLARE @EndStrCharIndex INT;
	DECLARE @StartNumPadIndex INT;
	DECLARE @EndStrNumdIndex INT;

	-- Getting Function Name
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Getting Function Name';
	SET @StartStrCharIndex = CHARINDEX('const.', @Definition);
	SET @EndStrCharIndex = CHARINDEX('()*/', @Definition) + 2;
	SET @FunctionName = SUBSTRING(@Definition, @StartStrCharIndex, @EndStrCharIndex - @StartStrCharIndex);

	-- Getting Function Value
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Getting Function Value';
	SET @Definition = SUBSTRING(@Definition, @EndStrCharIndex + 2, 4000);
	SET @StartNumPadIndex = PATINDEX('%[0-9]%', @Definition);
	SET @EndStrNumdIndex = PATINDEX('%[^0-9]%', @Definition);
	SET @Value = CAST(SUBSTRING(@Definition, @StartNumPadIndex, @EndStrNumdIndex - @StartNumPadIndex) AS INT);

	-- Getting Function Id
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Getting Function Id';
	BEGIN TRY
		DECLARE @SqlQuery TSTRING = 'SET @Id = ' + @FunctionName;
		EXEC sys.sp_executesql @SqlQuery, N'@Id INT OUTPUT', @Id OUTPUT;

	END TRY
	BEGIN CATCH
		DECLARE @Message TSTRING = 'Function ' + @FunctionName + ' Does not Exists in the storeProcedure ' + @ObjectName;
		EXEC err.ThrowAccessDeniedOrObjectNotExists @ProcId = @@PROCID, @Message = @Message;
	END CATCH;

	SET @Definition = SUBSTRING(@Definition, @EndStrCharIndex - @StartStrCharIndex, 4000);

END;