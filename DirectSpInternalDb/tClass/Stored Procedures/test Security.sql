
CREATE PROCEDURE [tClass].[test Security]
AS
BEGIN
    DECLARE @SystemUserId INT = dsp.Setting_SystemUserId();
    DECLARE @CommnonUserId INT = 100;

    EXEC tAccess.Private_Assert_SecurityAccessOnObject @CallerUserId = @SystemUserId, @Captcha_Create = 1, @Captcha_Match = 1, @KeyValue_R = 1, @KeyValue_W = 1, @ApiList = 1;
    EXEC tAccess.Private_Assert_SecurityAccessOnObject @CallerUserId = @CommnonUserId, @Captcha_Create = 0, @Captcha_Match = 0, @KeyValue_R = 0, @KeyValue_W = 0, @ApiList = 1;


END;








