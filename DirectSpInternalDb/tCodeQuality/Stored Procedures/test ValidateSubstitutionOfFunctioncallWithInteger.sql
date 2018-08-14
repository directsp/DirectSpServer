CREATE PROC [tCodeQuality].[test ValidateSubstitutionOfFunctioncallWithInteger]
AS
BEGIN
	SET NOCOUNT ON;
	-- Getting Stored Procedures and Functions definition
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Getting Stored Procedures definition';
	DECLARE Text_Cursor CURSOR FAST_FORWARD FORWARD_ONLY FORWARD_ONLY LOCAL FOR
	SELECT	PD.SchemaName, PD.ObjectName, PD.Script
	FROM	dsp.Metadata_ProceduresDefination() AS PD
	WHERE	PD.SchemaName IN ( 'api', 'dbo' );

	OPEN Text_Cursor;

	DECLARE @Definition TBIGSTRING;
	DECLARE @ObjectName TSTRING;
	DECLARE @SchemaName TSTRING;
	DECLARE @Pattern TSTRING = '/*co' + 'nst.';

	WHILE (1 = 1)
	BEGIN
		FETCH NEXT FROM Text_Cursor
		INTO @SchemaName, @ObjectName, @Definition;
		IF (@@FETCH_STATUS <> 0)
			BREAK;

		-- Removing Space, Tab, line feed
		EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Removing Space, Tab, line feed';
		SET @Definition = REPLACE(@Definition, ' ', '');
		SET @Definition = REPLACE(@Definition, CHAR(10), '');
		SET @Definition = REPLACE(@Definition, CHAR(13), '');

		-- Cutting out text before /*co+nst
		EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Cutting out text before /*con st';
		DECLARE @StartIndex INT = CHARINDEX(@Pattern, @Definition);
		SET @Definition = SUBSTRING(@Definition, @StartIndex - 1, (LEN(@Definition) - @StartIndex) + 1);

		IF (CHARINDEX(@Pattern, @Definition) = 0)
			CONTINUE;

		-- Validate Function Id with Corresponding value
		EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Validate Function Id with Corresponding value';
		WHILE (CHARINDEX(@Pattern, @Definition) > 0)
		BEGIN
			DECLARE @ConstFunctionName TSTRING;
			DECLARE @ConstValueInFunction INT;
			DECLARE @ConstValueInScript INT;
			DECLARE @IsMatch BIT;
			EXEC tCodeQuality.Private_CompareConstFunctionReturnValueWithScriptValue @Script = @Definition OUTPUT, @ConstFunctionName = @ConstFunctionName OUTPUT,
				@ConstValueInFunction = @ConstValueInFunction OUTPUT, @ConstValueInScript = @ConstValueInScript OUTPUT, @IsMatch = @IsMatch OUTPUT;

			IF (@IsMatch = 0)
			BEGIN
				DECLARE @Message TSTRING;
				DECLARE @FullObjectName TSTRING = @SchemaName + '.' + @ObjectName;
				EXEC @Message = dsp.Formatter_FormatMessage @Message = N'ConstValueInFunction({0}) and ConstValueInScript({1}) are inconsistence; the function name is: {2} in the SP: {3}',
					@Param0 = @ConstValueInFunction, @Param1 = @ConstValueInScript, @Param2 = @ConstFunctionName, @Param3 = @FullObjectName;
				EXEC tSQLt.Fail @Message0 = @Message;
			END;

			SET @StartIndex = CHARINDEX(@Pattern, @Definition);
			SET @Definition = SUBSTRING(@Definition, @StartIndex, (LEN(@Definition) - @StartIndex) + 1);
		END;

	END;

	CLOSE Text_Cursor;
	DEALLOCATE Text_Cursor;
END;













