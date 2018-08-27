CREATE PROCEDURE [dsp].[Context_CreateSystem]
    @SystemContext TCONTEXT OUT
AS
BEGIN
    EXEC dsp.Context_Create @UserId = '$', @Context = @SystemContext OUT, @IsCaptcha = 1;
END;