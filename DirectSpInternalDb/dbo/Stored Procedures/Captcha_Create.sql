CREATE PROCEDURE [dbo].[Captcha_Create]
	@CaptchaCode TSTRING, @CaptchaId UNIQUEIDENTIFIER OUT
AS
BEGIN
	DECLARE @TempTable TABLE (
		CaptchaId UNIQUEIDENTIFIER);

	INSERT INTO dbo.Captcha (Code)
	OUTPUT Inserted.CaptchaId
	INTO @TempTable
	VALUES (@CaptchaCode);

	SELECT	@CaptchaId = CaptchaId
	FROM	@TempTable;

END;