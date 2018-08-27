CREATE PROCEDURE [dbo].[Context_SimpleCheckAccess]
    @Context TCONTEXT OUT, @UserId INT = -1
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @SystemUserId INT = dsp.Setting_SystemUserId();
    DECLARE @ContextUserId INT = dsp.Context_UserId(@Context);

    -- Throw if UserId is set but it is NULL
    IF (dsp.Param_IsSet(@UserId) = 1 AND @UserId IS NULL) --
        EXEC err.ThrowAccessDeniedOrObjectNotExists @ProcId = @@PROCID;

    -- accept if caller is valid
    IF (@ContextUserId = @SystemUserId OR   @ContextUserId = @UserId)
        RETURN;

    EXEC err.ThrowAccessDeniedOrObjectNotExists @ProcId = @@PROCID;
END;