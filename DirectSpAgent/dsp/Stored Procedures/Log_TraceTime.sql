CREATE PROCEDURE [dsp].[Log_TraceTime]
	@Time AS DATETIME OUT, @Tag TSTRING
AS
BEGIN
	SET @Time = ISNULL(@Time, GETDATE());
	DECLARE @TimeDiff INT = DATEDIFF(MILLISECOND, @Time, GETDATE());

	DECLARE @Msg TSTRING;
	EXEC @Msg = dsp.Log_FormatMessage2 @ProcId = @@PROCID, @Message = '{0}: {1}', @Param0 = @Tag, @Param1 = @TimeDiff, @Elipsis = 0;
    RAISERROR(@Msg, 0, 1) WITH NOWAIT; -- force to flush the buffer

	SET @Time = GETDATE();
END;









