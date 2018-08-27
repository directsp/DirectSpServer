CREATE PROC [tCodeQuality].[Private_CompareConstFunctionReturnValueWithScriptValue]
	@Script TBIGSTRING OUT, @ConstFunctionName TSTRING OUT, @ConstValueInFunction INT OUT, @ConstValueInScript INT OUT, @IsMatch BIT OUT
AS
BEGIN
	DECLARE @StartStrCharIndex INT;
	DECLARE @EndStrCharIndex INT;
	DECLARE @StartNumPadIndex INT;
	DECLARE @EndStrNumdIndex INT;

	SET @IsMatch = 0;
	SET @ConstValueInScript = NULL;
	SET @ConstValueInFunction = NULL;

	-- Getting Function Name
	SET @StartStrCharIndex = CHARINDEX('/*co' + 'nst.', @Script);
	IF (@StartStrCharIndex = 0)
		RETURN;

	SET @EndStrCharIndex = CHARINDEX('()*/', @Script, @StartStrCharIndex) + 2;
	SET @StartStrCharIndex = @StartStrCharIndex + 2;
	SET @ConstFunctionName = SUBSTRING(@Script, @StartStrCharIndex, @EndStrCharIndex - @StartStrCharIndex);
	
	-- Getting Function Value
	BEGIN TRY
		SET @Script = SUBSTRING(@Script, @EndStrCharIndex + 2, LEN(@Script));
		SET @StartNumPadIndex = PATINDEX('%[0-9]%', @Script);
		SET @EndStrNumdIndex = PATINDEX('%[^0-9]%', @Script);
		SET @ConstValueInScript = CAST(SUBSTRING(@Script, @StartNumPadIndex, @EndStrNumdIndex - @StartNumPadIndex) AS INT);
	END TRY
	BEGIN CATCH
		EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = 'ConstFunctionValue has written before ConstFunctionName!';
	END CATCH;

	-- Getting Function Id
	BEGIN TRY
		DECLARE @SqlQuery TSTRING = 'SET @Id = ' + @ConstFunctionName;
		EXEC sys.sp_executesql @SqlQuery, N'@Id INT OUTPUT', @ConstValueInFunction OUTPUT;
		SET @IsMatch = IIF(@ConstValueInFunction = @ConstValueInScript, 1, 0);
	END TRY
	BEGIN CATCH
	END CATCH;

	DECLARE @StartIndex INT = @EndStrCharIndex - @StartStrCharIndex;
	
	SET @Script = SUBSTRING(@Script, @StartIndex, (LEN(@Script)))
END;



