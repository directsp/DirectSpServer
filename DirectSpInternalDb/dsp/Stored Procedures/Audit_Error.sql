CREATE PROCEDURE [dsp].[Audit_Error]
	@ProcId AS INT,
	@Message AS TSTRING,
	@Param0 AS TSTRING = '<notset>',
	@Param1 AS TSTRING = '<notset>',
	@Param2 AS TSTRING = '<notset>'
AS
BEGIN
	-- Format Message
	EXEC dsp.Log_Error @ProcId = @ProcId, @Message = @Message, @Param0 = @Param0, @Param1 = @Param1, @Param2 = @Param2;
END;