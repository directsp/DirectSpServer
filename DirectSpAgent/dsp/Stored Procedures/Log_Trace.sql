CREATE PROCEDURE [dsp].[Log_Trace]
    @ProcId AS INT, @Message AS TSTRING, @Param0 AS TSTRING = '<notset>', @Param1 AS TSTRING = '<notset>', @Param2 AS TSTRING = '<notset>',
    @Param3 AS TSTRING = '<notset>', @Param4 AS TSTRING = '<notset>', @Param5 AS TSTRING = '<notset>', @Elipse BIT = 1, @IsHeader BIT = 0
AS
BEGIN
    -- check is log enbaled (fast)
    DECLARE @Log_IsEnabled BIT = CAST(SESSION_CONTEXT(N'dsp.Log_IsEnabled') AS BIT);
    IF (@Log_IsEnabled IS NOT NULL AND   @Log_IsEnabled = 0)
        RETURN;

    -- check is log enbaled
    IF (dsp.Log_IsEnabled() = 0)
        RETURN;

    -- Format Message
    DECLARE @Msg TSTRING = dsp.Log_FormatMessage2(@ProcId, @Message, @Elipse, @Param0, @Param1, @Param2, @Param3, @Param4, @Param5);

    -- Manage header
    IF (@IsHeader = 1)
        SET @Msg = dsp.String_ReplaceEnter(N'\n-----------------------\n-- ' + @Msg + N'\n-----------------------');

    -- Check Filter
    IF (dsp.Log_$CheckFilters(@Msg) = 0)
        RETURN;

    -- Print with Black Color
    -- PRINT @Msg;
    RAISERROR(@Msg, 0, 1) WITH NOWAIT; -- force to flush the buffer
END;