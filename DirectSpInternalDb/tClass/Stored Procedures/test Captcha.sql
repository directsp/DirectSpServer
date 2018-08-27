CREATE PROCEDURE [tClass].[test Captcha]
AS
BEGIN
	DECLARE @SystemContext NVARCHAR(MAX);
	EXEC dsp.Context_CreateSystem @SystemContext = @SystemContext OUTPUT;

	DECLARE @CaptchaId UNIQUEIDENTIFIER;

	---------------------------------
	-- Checking 1: Captcha_Match with valid code should accept for first time
	---------------------------------
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = 'Checking 1: Captcha_Match with valid code should accept for first time';
	EXEC api.Captcha_Create @Context = @SystemContext OUT, @Code = '100', @CaptchaId = @CaptchaId OUT;
	EXEC api.Captcha_Match @Context = @SystemContext OUT, @CaptchaId = @CaptchaId, @Code = '100';

	---------------------------------
	-- Checking 2: Captcha_Match with valid code should reject the request for second time
	---------------------------------
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = 'Checking 2: Captcha_Match with valid code should accept for first time';
	BEGIN TRY
		EXEC api.Captcha_Match @Context = @SystemContext OUT, @CaptchaId = @CaptchaId, @Code = '100';
		EXEC tSQLt.Fail @Message0 = N'Captcha should not match for second time';
	END TRY
	BEGIN CATCH
		IF (ERROR_NUMBER() != err.InvalidCaptchaId())
			THROW;
	END CATCH;

	---------------------------------
	-- Captcha_Match with valid code reject after expiration time
	---------------------------------
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = 'Checking 2: Captcha_Match with valid code reject after expiration time';
	BEGIN TRY
		EXEC api.Captcha_Create @Context = @SystemContext OUT, @Code = '100', @CaptchaId = @CaptchaId OUT;
		DECLARE @CaptchaLifetime INT;
		EXEC dbo.Setting_Props @CaptchaLifetime = @CaptchaLifetime OUTPUT;

		UPDATE	dbo.Captcha
		SET		CreatedTime = DATEADD(SECOND, -@CaptchaLifetime, GETDATE())
		WHERE	CaptchaId = @CaptchaId;

		EXEC api.Captcha_Match @Context = @SystemContext OUT, @CaptchaId = @CaptchaId, @Code = '100';
		EXEC tSQLt.Fail @Message0 = N'Captcha should not match after its lifetime';
	END TRY
	BEGIN CATCH
		IF (ERROR_NUMBER() != err.InvalidCaptchaId())
			THROW;
	END CATCH;

	---------------------------------
	-- Checking 3: Captcha_Match with invalid code should reject for first time
	---------------------------------
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = 'Checking 3: Captcha_Match with invalid code should reject for first time';
	BEGIN TRY
		EXEC api.Captcha_Create @Context = @SystemContext OUT, @Code = '100', @CaptchaId = @CaptchaId OUT;
		EXEC api.Captcha_Match @Context = @SystemContext OUT, @CaptchaId = @CaptchaId, @Code = '200';
		EXEC tSQLt.Fail @Message0 = N'Captcha should not match for second time';
	END TRY
	BEGIN CATCH
		IF (ERROR_NUMBER() != err.InvalidCaptchaId())
			THROW;
	END CATCH;


	---------------------------------
	-- Checking 4: Captcha_Match with valid code should reject for second request
	---------------------------------
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = 'Checking 4: Captcha_Match with valid code should reject for second request';
	BEGIN TRY
		EXEC api.Captcha_Match @Context = @SystemContext OUT, @CaptchaId = @CaptchaId, @Code = '100';
		EXEC tSQLt.Fail @Message0 = N'Captcha should not match for second time';
	END TRY
	BEGIN CATCH
		IF (ERROR_NUMBER() != err.InvalidCaptchaId())
			THROW;
	END CATCH;


END;