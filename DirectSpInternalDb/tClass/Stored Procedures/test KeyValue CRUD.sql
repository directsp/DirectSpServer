CREATE PROCEDURE [tClass].[test KeyValue CRUD]
AS
BEGIN
	-- Get SystemContext
	DECLARE @SystemContext TCONTEXT;
	EXEC dsp.Context_CreateSystem @SystemContext = @SystemContext OUTPUT;

	-- Prepare data in table
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Prepare data in table';
	EXEC api.KeyValue_ValueSet @Context = @SystemContext OUTPUT, @KeyName = N't_key_11', @TextValue = N'text11', @TimeToLife = 600;
	EXEC api.KeyValue_ValueSet @Context = @SystemContext OUTPUT, @KeyName = N't_key_12', @TextValue = N'text12', @TimeToLife = 600;
	EXEC api.KeyValue_ValueSet @Context = @SystemContext OUTPUT, @KeyName = N't_key_21', @TextValue = N'text21', @TimeToLife = 600;

	-- insert expired data
	EXEC api.KeyValue_ValueSet @Context = @SystemContext OUTPUT, @KeyName = N't_key_31', @TextValue = N'text31';
	EXEC api.KeyValue_ValueSet @Context = @SystemContext OUTPUT, @KeyName = N't_key_13', @TextValue = N'text13';

	UPDATE	dbo.KeyValue
	SET ExpirationTime = DATEADD(MINUTE, -1, GETDATE())
	WHERE	KeyName = N't_key_31' OR KeyName = N't_key_13';


	-------------------------------
	-- Checking: Expired key should not be returned
	-------------------------------
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Checking: t_key_31 should not be returned';

	DECLARE @IsExist BIT;
	EXEC api.KeyValue_Value @Context = @SystemContext OUTPUT, @KeyName = 't_key_31', @ThrowErrorIfNotExists = 0, @IsExist = @IsExist OUT;
	EXEC tSQLt.AssertEquals @Expected = 0, @Actual = @IsExist, @Message = N'IsExist';

	-------------------------------
	-- Checking: KeyValue_All should return all data by NULL pattern
	-------------------------------
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Checking: KeyValue_All should return all data by NULL pattern';

	DECLARE @KeyValueInfo ud_KeyValue;
	INSERT INTO @KeyValueInfo (KeyName, TextValue, ModifiedTime)
	EXEC api.KeyValue_All @Context = @SystemContext, @KeyNamePattern = NULL;

	DECLARE @ActualKeyValueCount INT;
	EXEC api.KeyValue_Count @Context = @SystemContext OUTPUT, @Count = @ActualKeyValueCount OUTPUT;

	IF (@ActualKeyValueCount < 3) --
		EXEC tSQLt.Fail @Message0 = N'KeyValue_All has returned record(s) less than expected!';

	IF NOT EXISTS (SELECT		1
						FROM	@KeyValueInfo AS KVI
					WHERE	KVI.KeyName = 't_key_11')
		EXEC tSQLt.Fail @Message0 = N't_key_11 should be returned';


	-------------------------------
	-- Checking: KeyValue_All should return all data by given pattern
	-------------------------------
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Checking: KeyValue_All should return all data by given pattern';

	INSERT INTO @KeyValueInfo (KeyName, TextValue, ModifiedTime)
	EXEC api.KeyValue_All @Context = @SystemContext, @KeyNamePattern = 't_key_1%';

	EXEC api.KeyValue_Count @Context = @SystemContext OUTPUT, @KeyNamePattern = 't_key_1%', @Count = @ActualKeyValueCount OUTPUT;

	IF (@ActualKeyValueCount < 2) --
		EXEC tSQLt.Fail @Message0 = N'KeyValue_All has returned record(s) less than expected!';

	IF NOT EXISTS (SELECT		1
						FROM	@KeyValueInfo AS KVI
					WHERE	KVI.KeyName = 't_key_11')
		EXEC tSQLt.Fail @Message0 = N't_key_11 is not in result';

	-------------------------------
	-- Checking: KeyValue_AllKeys should return all data by NULL pattern
	-------------------------------
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Checking: KeyValue_AllKeys should return all data by NULL pattern';

	INSERT INTO @KeyValueInfo (KeyName)
	EXEC api.KeyValue_AllKeys @Context = @SystemContext, @KeyNamePattern = NULL;

	EXEC api.KeyValue_Count @Context = @SystemContext OUTPUT, @Count = @ActualKeyValueCount OUTPUT;

	IF (@ActualKeyValueCount < 3) --
		EXEC tSQLt.Fail @Message0 = N'KeyValue_AllKeys has returned record(s) less than expected!';

	IF NOT EXISTS (SELECT		1
						FROM	@KeyValueInfo AS KVI
					WHERE	KVI.KeyName = 't_key_11')
		EXEC tSQLt.Fail @Message0 = N't_key_11 should be returned';


	-------------------------------
	-- Checking: KeyValue_AllKeys should return all data by given pattern
	-------------------------------
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Checking: KeyValue_AllKeys should return all data by given pattern';

	INSERT INTO @KeyValueInfo (KeyName)
	EXEC api.KeyValue_AllKeys @Context = @SystemContext, @KeyNamePattern = 't_key_1%';

	EXEC api.KeyValue_Count @Context = @SystemContext OUTPUT, @KeyNamePattern = 't_key_1%', @Count = @ActualKeyValueCount OUTPUT;

	IF (@ActualKeyValueCount < 2) --
		EXEC tSQLt.Fail @Message0 = N'KeyValue_AllKeys has returned record(s) less than expected!';

	IF NOT EXISTS (SELECT		1
						FROM	@KeyValueInfo AS KVI
					WHERE	KVI.KeyName = 't_key_11')
		EXEC tSQLt.Fail @Message0 = N't_key_11 is not in result';

	-------------------------------
	-- Checking: KeyValue_Count should return correct count by NULL Pattern
	-------------------------------
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Checking: KeyValue_Count should return correct count by NULL Pattern';

	DECLARE @ActualCount INT;
	EXEC api.KeyValue_Count @Context = @SystemContext OUTPUT, @Count = @ActualCount OUTPUT;

	IF (@ActualCount < 3) --
		EXEC tSQLt.Fail @Message0 = N'All KeyValue count should be returned';

	-------------------------------
	-- Checking: KeyValue_Count should return correct count by given Pattern
	-------------------------------
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Checking: KeyValue_Count should return correct count by given Pattern';

	EXEC api.KeyValue_Count @Context = @SystemContext OUTPUT, @KeyNamePattern = 't_key_1%', @Count = @ActualCount OUTPUT;

	IF (@ActualCount != 2) --
		EXEC tSQLt.Fail @Message0 = N'Count of KeyValue(s) containing the KeyName should is less than expected';

	-------------------------------
	-- Checking: The key should not be updated if IsOverwrite is false
	-------------------------------
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Checking: The key should not be updated if IsOverwrite is false';

	SAVE TRANSACTION Test;

	DECLARE @Json TSTRING = '{"KeyName" : "t_key_11", "TextValue" : "new_Text11", "TimeToLife" : "60", "IsOverwrite" : "0"}';

	BEGIN TRY
		EXEC api.KeyValue_CreateByJson @Context = @SystemContext OUTPUT, @Json = @Json;
		EXEC tSQLt.Fail @Message0 = N'The key should not be updated if IsOverwrite is false';
	END TRY
	BEGIN CATCH
		IF (ERROR_NUMBER() != err.ObjectAlreadyExistsId())
			THROW;
	END CATCH;

	ROLLBACK TRANSACTION Test;

	-------------------------------
	-- Checking: Bach mode > The key should not be updated if IsOverwrite is true
	-------------------------------
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Checking: Bach mode > The key should not be updated if IsOverwrite is true';

	SAVE TRANSACTION Test;

	DECLARE @TextValue TSTRING;

	SET @Json = '[{"KeyName" : "t_key_11", "TextValue" : "new_Text11", "TimeToLife" : "60", "IsOverwrite" : "1"}]';
	EXEC api.KeyValue_CreateByJson @Context = @SystemContext OUTPUT, @Json = @Json;
	EXEC api.KeyValue_Value @Context = @SystemContext OUTPUT, @KeyName = 't_key_11', @TextValue = @TextValue OUT;
	EXEC tSQLt.AssertEquals @Expected = 'new_Text11', @Actual = @TextValue, @Message = N'TextValue';

	ROLLBACK TRANSACTION Test;

	-------------------------------
	-- Checking: Bach mode > All key(s) should be inserted
	-------------------------------
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Checking: Bach mode > All key(s) should be inserted';

	SAVE TRANSACTION Test;

	SET @Json =
		'[{"KeyName" : "t_key_101", "TextValue" : "Text101", "TimeToLife" : "60", "IsOverwrite" : "0"},
		  {"KeyName" : "t_key_102", "TextValue" : "Text102", "TimeToLife" : "60", "IsOverwrite" : "0"}]';
	EXEC api.KeyValue_CreateByJson @Context = @SystemContext OUTPUT, @Json = @Json;
	EXEC api.KeyValue_Value @Context = @SystemContext OUTPUT, @KeyName = 't_key_101', @ThrowErrorIfNotExists = 1;
	EXEC api.KeyValue_Value @Context = @SystemContext OUTPUT, @KeyName = 't_key_102', @ThrowErrorIfNotExists = 1;


	ROLLBACK TRANSACTION Test;

	-------------------------------
	-- Checking: KeyValue_ValueSet should insert when KeyName does not exists
	-------------------------------
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Checking: Checking: KeyValue_ValueSet should insert when KeyName does not exists';

	SAVE TRANSACTION Test;

	EXEC api.KeyValue_ValueSet @Context = @SystemContext OUTPUT, @KeyName = 't_key_201', @TextValue = 'Text201';
	EXEC api.KeyValue_Value @Context = @SystemContext OUTPUT, @KeyName = 't_key_201', @ThrowErrorIfNotExists = 1;

	ROLLBACK TRANSACTION Test;

	-------------------------------
	-- Checking: KeyValue_ValueSet should update data when KeyName exists and IsOverwrite is true
	-------------------------------
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Checking: KeyValue_ValueSet should update data when KeyName exists and IsOverwrite is true';

	SAVE TRANSACTION Test;

	EXEC api.KeyValue_ValueSet @Context = @SystemContext OUTPUT, @KeyName = 't_key_201', @TextValue = 'Text201';
	EXEC api.KeyValue_ValueSet @Context = @SystemContext OUTPUT, @KeyName = 't_key_201', @TextValue = 'new_Text201', @IsOverwrite = 1;

	DECLARE @ActualTextValue TBIGSTRING;
	EXEC api.KeyValue_Value @Context = @SystemContext OUTPUT, @KeyName = 't_key_201', @TextValue = @ActualTextValue OUT;
	EXEC tSQLt.AssertEqualsString @Expected = 'new_Text201', @Actual = @ActualTextValue, @Message = N'TextValue';

	ROLLBACK TRANSACTION Test;

	-------------------------------
	-- Checking: KeyValue_ValueSet should update data when KeyName exists and IsOverwrite is false
	-------------------------------
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Checking: KeyValue_ValueSet should update data when KeyName exists and IsOverwrite is false';

	SAVE TRANSACTION Test;

	EXEC api.KeyValue_ValueSet @Context = @SystemContext OUTPUT, @KeyName = 't_key_201', @TextValue = 'Text201';

	BEGIN TRY
		EXEC api.KeyValue_ValueSet @Context = @SystemContext OUTPUT, @KeyName = 't_key_201', @TextValue = 'new_Text201', @IsOverwrite = 0;
		EXEC tSQLt.Fail @Message0 = N'KeyValue_ValueSet should not insert duplicate data';
	END TRY
	BEGIN CATCH
		IF (ERROR_NUMBER() != err.ObjectAlreadyExistsId())
			THROW;
	END CATCH;

	ROLLBACK TRANSACTION Test;

	-------------------------------
	-- Checking: KeyValue_Delete should delete The given KeyName
	-------------------------------
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Checking: KeyValue_Delete should delete The given KeyName';

	SAVE TRANSACTION Test;

	DECLARE @AffectedCount INT;
	EXEC api.KeyValue_Delete @Context = @SystemContext OUTPUT, @KeyNamePattern = 't_key_21', @AffectedCount = @AffectedCount OUTPUT;

	IF (@AffectedCount < 1) --
		EXEC tSQLt.Fail @Message0 = N't_key_21 should not exists any more';

	ROLLBACK TRANSACTION Test;

	-------------------------------
	-- Checking: KeyValue_Delete should delete The given pattern
	-------------------------------
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Checking: KeyValue_Delete should delete The given pattern';

	SAVE TRANSACTION Test;

	EXEC api.KeyValue_Delete @Context = @SystemContext OUTPUT, @KeyNamePattern = 't_key_1%', @AffectedCount = @AffectedCount OUTPUT;

	IF (@AffectedCount != 2) --
		EXEC tSQLt.Fail @Message0 = N'KeyValue_Delete has not deleted all data having given pattern';

	ROLLBACK TRANSACTION Test;

END;




