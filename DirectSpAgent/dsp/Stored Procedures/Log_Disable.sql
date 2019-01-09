-- Stop Logging System but keep settings
CREATE PROCEDURE [dsp].[Log_Disable]
AS
BEGIN
	
	-- Set enable flag
	UPDATE dsp.LogUser SET	IsEnabled = 0 WHERE UserName = SYSTEM_USER;
	PRINT 'LogSystem> LogSystem has been disbaled.';

	EXEC sp_set_session_context 'dsp.Log_IsEnabled', 0, @read_only = 0;

END