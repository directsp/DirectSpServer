
CREATE	PROC [dsp].[ThrowInvalidArgument]
	@ProcId INT, @ArgumentName TSTRING, @ArgumentValue TSTRING, @Message TSTRING = NULL
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @Exception TJSON;
	EXEC @Exception = dsp.Exception_BuildInvalidArgument @ProcId = @ProcId, @ArgumentName = @ArgumentName, @ArgumentValue = @ArgumentValue, @Message = @Message;
	EXEC dsp.ThrowException @Exception = @Exception;
END;











