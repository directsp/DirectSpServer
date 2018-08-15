CREATE PROC [dbo].[Setting_Props]
    @CaptchaLifetime INT = NULL OUT
AS
BEGIN
    SELECT  @CaptchaLifetime = S.CaptchaLifetime
      FROM  dbo.Setting AS S;
END;