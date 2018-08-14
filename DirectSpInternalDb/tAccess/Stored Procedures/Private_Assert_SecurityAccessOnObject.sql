CREATE PROCEDURE [tAccess].[Private_Assert_SecurityAccessOnObject]
    @CallerUserId INT, @Captcha_Create BIT, @Captcha_Match BIT, @KeyValue_R BIT, @KeyValue_W BIT, @ApiList BIT
AS
BEGIN
    DECLARE @action TSTRING;
    DECLARE @Access BIT;

    -- Messages
    DECLARE @msg1 TSTRING;
    DECLARE @msg2 TSTRING;
    DECLARE @msg3 TSTRING;

    EXEC @msg1 = dsp.Formatter_FormatMessage @Message = N'User {0} should have access! Permission: ', @Param0 = @CallerUserId;
    EXEC @msg2 = dsp.Formatter_FormatMessage @Message = N'User {0} should NOT have access! Permission: ', @Param0 = @CallerUserId;

    -- Get System Context
    DECLARE @SystemContext TCONTEXT;
    EXEC dsp.Context_CreateSystem @SystemContext OUT;

    -- Get User Context
    DECLARE @Context TCONTEXT;
    EXEC dsp.Context_Create @UserId = @CallerUserId, @Context = @Context OUT;

    -- Checking
    SET @action = 'Captcha_Create';
    SET @Access = @Captcha_Create;
    EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = 'Checking {0} permission', @Param0 = @action;
    DECLARE @CaptchaId UNIQUEIDENTIFIER;
    SAVE TRANSACTION TEST;
    BEGIN TRY
        EXEC api.Captcha_Create @Context = @Context OUTPUT, @Code = N'123', @CaptchaId = @CaptchaId OUTPUT;
        SET @msg3 = @msg2 + @action;
        EXEC tSQLt.AssertEquals @Access, 1, @msg3;
    END TRY
    BEGIN CATCH
        IF (ERROR_NUMBER() <> err.AccessDeniedOrObjectNotExistsId()) -- access denied
            THROW;
        SET @msg3 = @msg1 + @action + '; InternalError: ' + ERROR_MESSAGE();
        EXEC tSQLt.AssertEquals @Access, 0, @msg3;
    END CATCH;
    ROLLBACK TRANSACTION TEST;

    -- Checking
    SET @action = 'Captcha_Match';
    SET @Access = @Captcha_Match;
    EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = 'Checking {0} permission', @Param0 = @action;
    SAVE TRANSACTION TEST;
    BEGIN TRY
        EXEC api.Captcha_Create @Context = @Context OUTPUT, @Code = N'123', @CaptchaId = @CaptchaId OUTPUT;
        EXEC api.Captcha_Match @Context = @Context OUTPUT, @CaptchaId = @CaptchaId, @Code = N'123';
        SET @msg3 = @msg2 + @action;
        EXEC tSQLt.AssertEquals @Access, 1, @msg3;
    END TRY
    BEGIN CATCH
        IF (ERROR_NUMBER() <> err.AccessDeniedOrObjectNotExistsId()) -- access denied
            THROW;
        SET @msg3 = @msg1 + @action + '; InternalError: ' + ERROR_MESSAGE();
        EXEC tSQLt.AssertEquals @Access, 0, @msg3;
    END CATCH;
    ROLLBACK TRANSACTION TEST;

    -- Checking
    SET @action = 'KeyValue_R > KeyValue_All';
    SET @Access = @KeyValue_R;
    EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = 'Checking {0} permission', @Param0 = @action;
    SAVE TRANSACTION TEST;
    BEGIN TRY
        -- create a key by system
        EXEC api.KeyValue_ValueSet @Context = @SystemContext OUTPUT, @KeyName = 't_unittest_testkey', @TextValue = '100';

        -- check security
        DECLARE @ud_KeyValue ud_KeyValue;
        INSERT INTO @ud_KeyValue
        EXEC api.KeyValue_All @Context = @Context OUTPUT;
        SET @msg3 = @msg2 + @action;
        EXEC tSQLt.AssertEquals @Access, 1, @msg3;
    END TRY
    BEGIN CATCH
        IF (ERROR_NUMBER() <> err.AccessDeniedOrObjectNotExistsId()) -- access denied
            THROW;
        SET @msg3 = @msg1 + @action + '; InternalError: ' + ERROR_MESSAGE();
        EXEC tSQLt.AssertEquals @Access, 0, @msg3;
    END CATCH;
    ROLLBACK TRANSACTION TEST;

    -- Checking
    SET @action = 'KeyValue_R > KeyValue_AllKeys';
    SET @Access = @KeyValue_R;
    EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = 'Checking {0} permission', @Param0 = @action;
    SAVE TRANSACTION TEST;
    BEGIN TRY
        -- create a key by system
        EXEC api.KeyValue_ValueSet @Context = @SystemContext OUTPUT, @KeyName = 't_unittest_testkey', @TextValue = '100';

        -- check security
        DECLARE @ud_AllKeys ud_KeyValue;
        INSERT INTO @ud_AllKeys (KeyName)
        EXEC api.KeyValue_AllKeys @Context = @Context OUTPUT;

        SET @msg3 = @msg2 + @action;
        EXEC tSQLt.AssertEquals @Access, 1, @msg3;
    END TRY
    BEGIN CATCH
        IF (ERROR_NUMBER() <> err.AccessDeniedOrObjectNotExistsId()) -- access denied
            THROW;
        SET @msg3 = @msg1 + @action + '; InternalError: ' + ERROR_MESSAGE();
        EXEC tSQLt.AssertEquals @Access, 0, @msg3;
    END CATCH;
    ROLLBACK TRANSACTION TEST;

    -- Checking
    SET @action = 'KeyValue_R > KeyValue_Count';
    SET @Access = @KeyValue_R;
    EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = 'Checking {0} permission', @Param0 = @action;
    SAVE TRANSACTION TEST;
    BEGIN TRY
        -- create a key by system
        EXEC api.KeyValue_ValueSet @Context = @SystemContext OUTPUT, @KeyName = 't_unittest_testkey', @TextValue = '100';

        -- check security
        EXEC api.KeyValue_Count @Context = @Context OUTPUT;
        SET @msg3 = @msg2 + @action;
        EXEC tSQLt.AssertEquals @Access, 1, @msg3;
    END TRY
    BEGIN CATCH
        IF (ERROR_NUMBER() <> err.AccessDeniedOrObjectNotExistsId()) -- access denied
            THROW;
        SET @msg3 = @msg1 + @action + '; InternalError: ' + ERROR_MESSAGE();
        EXEC tSQLt.AssertEquals @Access, 0, @msg3;
    END CATCH;
    ROLLBACK TRANSACTION TEST;

    -- Checking
    SET @action = 'KeyValue_R > KeyValue_Value';
    SET @Access = @KeyValue_R;
    EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = 'Checking {0} permission', @Param0 = @action;
    SAVE TRANSACTION TEST;
    BEGIN TRY
        -- create a key by system
        EXEC api.KeyValue_ValueSet @Context = @SystemContext OUTPUT, @KeyName = 't_unittest_testkey', @TextValue = '100';

        -- check security
        EXEC api.KeyValue_Value @Context = @Context OUTPUT, @KeyName = 't_unittest_testkey';
        SET @msg3 = @msg2 + @action;
        EXEC tSQLt.AssertEquals @Access, 1, @msg3;
    END TRY
    BEGIN CATCH
        IF (ERROR_NUMBER() <> err.AccessDeniedOrObjectNotExistsId()) -- access denied
            THROW;
        SET @msg3 = @msg1 + @action + '; InternalError: ' + ERROR_MESSAGE();
        EXEC tSQLt.AssertEquals @Access, 0, @msg3;
    END CATCH;
    ROLLBACK TRANSACTION TEST;

    -- Checking
    SET @action = 'KeyValue_W > KeyValue_CreateByJson';
    SET @Access = @KeyValue_W;
    EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = 'Checking {0} permission', @Param0 = @action;
    SAVE TRANSACTION TEST;
    BEGIN TRY
        DECLARE @JsonForKeyValueCreate TSTRING = '{"KeyName" : "t_key_11", "TextValue" : "new_Text11", "TimeToLife" : "60", "IsOverwrite" : "0"}';
        EXEC api.KeyValue_CreateByJson @Context = @Context OUTPUT, @Json = @JsonForKeyValueCreate;

        SET @msg3 = @msg2 + @action;
        EXEC tSQLt.AssertEquals @Access, 1, @msg3;
    END TRY
    BEGIN CATCH
        IF (ERROR_NUMBER() <> err.AccessDeniedOrObjectNotExistsId()) -- access denied
            THROW;
        SET @msg3 = @msg1 + @action + '; InternalError: ' + ERROR_MESSAGE();
        EXEC tSQLt.AssertEquals @Access, 0, @msg3;
    END CATCH;
    ROLLBACK TRANSACTION TEST;

    -- Checking
    SET @action = 'KeyValue_W > KeyValue_Delete';
    SET @Access = @KeyValue_W;
    EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = 'Checking {0} permission', @Param0 = @action;
    SAVE TRANSACTION TEST;
    BEGIN TRY
        EXEC api.KeyValue_ValueSet @Context = @SystemContext OUTPUT, @KeyName = 't_unittest_testkey', @TextValue = '100';
        EXEC api.KeyValue_Delete @Context = @Context OUTPUT, @KeyNamePattern = 't_unittest_testkey';

        SET @msg3 = @msg2 + @action;
        EXEC tSQLt.AssertEquals @Access, 1, @msg3;
    END TRY
    BEGIN CATCH
        IF (ERROR_NUMBER() <> err.AccessDeniedOrObjectNotExistsId()) -- access denied
            THROW;
        SET @msg3 = @msg1 + @action + '; InternalError: ' + ERROR_MESSAGE();
        EXEC tSQLt.AssertEquals @Access, 0, @msg3;
    END CATCH;
    ROLLBACK TRANSACTION TEST;

    -- Checking
    SET @action = 'KeyValue_W > KeyValue_Set';
    SET @Access = @KeyValue_W;
    EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = 'Checking {0} permission', @Param0 = @action;
    SAVE TRANSACTION TEST;
    BEGIN TRY
        EXEC api.KeyValue_ValueSet @Context = @Context OUTPUT, @KeyName = 't_unittest_testkey', @TextValue = '100';

        SET @msg3 = @msg2 + @action;
        EXEC tSQLt.AssertEquals @Access, 1, @msg3;
    END TRY
    BEGIN CATCH
        IF (ERROR_NUMBER() <> err.AccessDeniedOrObjectNotExistsId()) -- access denied
            THROW;
        SET @msg3 = @msg1 + @action + '; InternalError: ' + ERROR_MESSAGE();
        EXEC tSQLt.AssertEquals @Access, 0, @msg3;
    END CATCH;
    ROLLBACK TRANSACTION TEST;

    -- Checking
    SET @action = 'ApiList';
    SET @Access = @ApiList;
    EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = 'Checking {0} permission', @Param0 = @action;
    SAVE TRANSACTION TEST;
    BEGIN TRY
        EXEC api.System_Api @Context = @Context OUTPUT;

        SET @msg3 = @msg2 + @action;
        EXEC tSQLt.AssertEquals @Access, 1, @msg3;
    END TRY
    BEGIN CATCH
        IF (ERROR_NUMBER() <> err.AccessDeniedOrObjectNotExistsId()) -- access denied
            THROW;
        SET @msg3 = @msg1 + @action + '; InternalError: ' + ERROR_MESSAGE();
        EXEC tSQLt.AssertEquals @Access, 0, @msg3;
    END CATCH;
    ROLLBACK TRANSACTION TEST;

END;



