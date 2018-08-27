/*
#MetaStart 
	{
		"DataAccessMode": "Write"
	} 
#MetaEnd 
*/
CREATE PROCEDURE [api].[Captcha_Create]
    @Context TCONTEXT OUT, @Code TSTRING, @CaptchaId UNIQUEIDENTIFIER OUT
WITH EXECUTE AS OWNER
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dsp.Context_Verify @Context = @Context OUT, @ProcId = @@PROCID;

    -- Check access
    EXEC dbo.Context_SimpleCheckAccess @Context = @Context OUTPUT;
    

    EXEC dbo.Captcha_Create @CaptchaCode = @Code, @CaptchaId = @CaptchaId OUT;
END;