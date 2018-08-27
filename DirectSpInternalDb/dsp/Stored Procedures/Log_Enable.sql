CREATE PROCEDURE [dsp].[Log_Enable]
    @RemoveAllFilters AS BIT = 0
AS
BEGIN
    SET @RemoveAllFilters = ISNULL(@RemoveAllFilters, 0);

	-- Find current status
    DECLARE @IsEnabled BIT;
    SELECT  @IsEnabled = LU.IsEnabled
      FROM  dsp.LogUser AS LU
     WHERE  LU.UserName = SYSTEM_USER;

    -- Set enable flag
    IF (@IsEnabled IS NULL)
        INSERT  dsp.LogUser (UserName, IsEnabled)
        VALUES (SYSTEM_USER, 1);
    ELSE
        UPDATE  dsp.LogUser
           SET  IsEnabled = 1
         WHERE  UserName = SYSTEM_USER;

    -- Remove All old filters
    IF (@RemoveAllFilters = 1)
        EXEC dsp.Log_RemoveFilter @Filter = NULL;

   -- Cache the result
    EXEC sys.sp_set_session_context 'dsp.Log_IsEnabled', 1, @read_only = 0;

    PRINT 'LogSystem> LogSystem has been enabled.';
END;