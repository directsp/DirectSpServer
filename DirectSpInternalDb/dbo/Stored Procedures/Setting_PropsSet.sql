
CREATE PROCEDURE [dbo].[Setting_PropsSet]
    @CaptchaLifetime INT = -1
AS
BEGIN
    SET NOCOUNT ON;

    IF (dsp.Param_IsSet(@CaptchaLifetime) = 1)
        UPDATE  dbo.Setting
           SET  CaptchaLifetime = @CaptchaLifetime;
END;

