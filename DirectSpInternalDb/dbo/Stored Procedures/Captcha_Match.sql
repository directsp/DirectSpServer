CREATE PROCEDURE [dbo].[Captcha_Match]
	@CaptchaId UNIQUEIDENTIFIER, @Code TSTRING
AS
BEGIN
	--@AuditUserId NotUsed

	DECLARE @Inserted TABLE (
		CaptchaId UNIQUEIDENTIFIER,
		Code TSTRING,
		CreatedTime DATETIME);

	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Finding and updating the unused captcha';
	-- Use the unused captcha
	UPDATE	dbo.Captcha
	SET		IsUsed = 1
	OUTPUT Inserted.CaptchaId, Inserted.Code, Inserted.CreatedTime
	INTO @Inserted
	WHERE	CaptchaId = @CaptchaId AND	IsUsed = 0;

	-- get found captcha
	DECLARE @OldCode TSTRING;
	DECLARE @CreatedTime DATETIME;
	SELECT	@OldCode = I.Code, @CreatedTime = I.CreatedTime
	FROM	@Inserted AS I
	WHERE	I.CaptchaId = @CaptchaId;


	-- check code
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Check captcha code';
	IF (@OldCode IS NULL OR @OldCode != @Code) --
		EXEC err.ThrowInvalidCaptcha @ProcId = @@PROCID;

	-- Check expiration
	DECLARE @CaptchaLifetime INT;
	EXEC dbo.Setting_Props @CaptchaLifetime = @CaptchaLifetime OUTPUT;

	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Check captcha expiration. CaptchaTime: {0}', @Param0 = @CreatedTime;
	IF (DATEADD(SECOND, @CaptchaLifetime, @CreatedTime) <= GETDATE()) --
		EXEC err.ThrowInvalidCaptcha @ProcId = @@PROCID, @Message = N'Captcha has been expired!';
END;






