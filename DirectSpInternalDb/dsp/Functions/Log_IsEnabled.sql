

CREATE FUNCTION [dsp].[Log_IsEnabled] ()
RETURNS BIT
AS
BEGIN
    DECLARE @Log_IsEnabled BIT = CAST(SESSION_CONTEXT(N'dsp.Log_IsEnabled') AS BIT);
    IF (@Log_IsEnabled IS NOT NULL)
        RETURN @Log_IsEnabled;

    SELECT  @Log_IsEnabled = LU.IsEnabled
      FROM  dsp.LogUser AS LU
     WHERE  LU.UserName = SYSTEM_USER;

    SET @Log_IsEnabled = ISNULL(@Log_IsEnabled, 0);
    EXEC sys.sp_set_session_context 'dsp.Log_IsEnabled', @Log_IsEnabled, @read_only = 0;

    RETURN @Log_IsEnabled;
END;