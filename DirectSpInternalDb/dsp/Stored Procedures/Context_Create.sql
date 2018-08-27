CREATE PROCEDURE [dsp].[Context_Create]
    @UserId TSTRING, @IsCaptcha INT = 0, @Context TCONTEXT = NULL OUT
AS
BEGIN
    DECLARE @AppName TSTRING;
    DECLARE @AppVersion TSTRING;
    EXEC dsp.Setting_Props @AppName = @AppName OUTPUT, @AppVersion = @AppVersion OUTPUT;

    SET @Context = NULL;
    EXEC dsp.Context_PropsSet @Context = @Context OUTPUT, @UserId = @UserId, @AppName = @AppName, @AppVersion = @AppVersion, @IsCaptcha = @IsCaptcha;
END;




