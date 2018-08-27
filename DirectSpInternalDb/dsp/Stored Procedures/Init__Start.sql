CREATE	PROC [dsp].[Init_$Start]
	@IsProductionEnvironment BIT = NULL, @IsWithCleanup BIT = NULL, @Reserved BIT = NULL
AS
BEGIN
	SET NOCOUNT ON;
	SET @Reserved = ISNULL(@Reserved, 1);

	-- SetUp DirectSp procedures and tables
	EXEC dsp.[Init_$SetUp];

	----------------
	-- Check Production Environment and Run Cleanup
	----------------
	EXEC dsp.[Init_$InitSettings];
	EXEC dsp.[Init_$Cleanup] @IsProductionEnvironment = @IsProductionEnvironment OUT, @IsWithCleanup = @IsWithCleanup OUT;
	EXEC dsp.[Init_$InitSettings];

	----------------
	-- Recreate Strings
	----------------
	IF (@Reserved = 1) EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = 'Recreating strings';
	DELETE	dsp.StringTable;
	EXEC dbo.Init_FillStrings;
	EXEC dsp.[Init_$RecreateStringFunctions];

	-- Check is internal databse
	DECLARE @IsInternalDatabase BIT = 0;
	SELECT	@IsInternalDatabase = 1
	FROM	dsp.StringTable AS ST
	WHERE	ST.StringId = 'IsDirectSpInternal';
	
	----------------
	-- Recreate Exceptions 
	----------------
	IF (@Reserved = 1) EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = 'Recreating exception';
	DELETE	dsp.Exception;
	EXEC dbo.Init_FillExceptions;

	-- make sure there is no invalid Exception Id for general application
	IF (@IsInternalDatabase = 0 AND EXISTS (SELECT		1
												FROM	dsp.Exception AS E
												WHERE	E.ExceptionId < 56000))
		EXEC dsp.ThrowAppException @ProcId = @@PROCID, @ExceptionId = 55001, @Message = 'Application ExceptionId cannot be less than 56000!';

	-- make sure there is no invalid Exception Id for DirectSpInternal
	IF (@IsInternalDatabase = 1 AND EXISTS (SELECT		1
												FROM	dsp.Exception AS E
												WHERE	E.ExceptionId < 55700 OR E.ExceptionId >= 56000))
		EXEC dsp.ThrowAppException @ProcId = @@PROCID, @ExceptionId = 55001, @Message = 'DirectSpInternal ExceptionId must asigned between 55700 and 56000!';


	EXEC dsp.[Init_$CreateCommonExceptions];
	EXEC dsp.[Init_$RecreateExceptionFunctions];

	----------------
	-- Lookups
	----------------
	IF (@Reserved = 1) --
		EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = 'Filling lookups';
	EXEC dbo.Init_FillLookups;

	IF (@Reserved = 1) EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = 'Filling data';
	EXEC dbo.Init_FillData;

	-- Call Init again to make sure it can be called without cleanup
	IF (@IsProductionEnvironment = 0 AND @IsWithCleanup = 1 AND @Reserved = 1)
	BEGIN
		EXEC dsp.[Init_$Start] @IsProductionEnvironment = 0, @IsWithCleanup = 0, @Reserved = 0;
		RETURN;
	END;


	-- Configure large fields
	EXEC dsp.Table_UpdateToUseBlobForFields;

	-- Report it is done
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = 'Init has been completed.';

END;

















