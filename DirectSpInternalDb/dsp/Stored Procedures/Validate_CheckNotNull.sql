
CREATE PROC [dsp].[Validate_CheckNotNull]
	@ProcId INT, @ArgumentName TSTRING, @ArgumentValue TSTRING
AS
BEGIN
	IF (@ArgumentValue IS NULL) --
		EXEC dsp.ThrowInvalidArgument @ProcId = @ProcId, @ArgumentName = @ArgumentName, @ArgumentValue = @ArgumentValue;
END;