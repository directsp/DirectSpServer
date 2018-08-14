/*
#MetaStart 
	{
		"DataAccessMode": "Read"
	} 
#MetaEnd 
*/
CREATE PROCEDURE [api].[Captcha_Match]
    @Context TCONTEXT OUT, @CaptchaId UNIQUEIDENTIFIER, @Code TSTRING
WITH EXECUTE AS OWNER
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dsp.Context_Verify @Context = @Context OUT, @ProcId = @@PROCID;

    -- Check access
	EXEC dbo.Context_SimpleCheckAccess @Context = @Context OUTPUT

    EXEC dbo.Captcha_Match @CaptchaId = @CaptchaId, @Code = @Code;
END;




