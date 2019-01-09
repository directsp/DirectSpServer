CREATE	FUNCTION [dsp].[Exception_BuildMessage] (@ProcId INT,
	@ExceptionId INT,
	@Message TSTRING = NULL)
RETURNS TJSON
AS
BEGIN
	RETURN	dsp.Exception_BuildMessageParam4(@ProcId, @ExceptionId, @Message, DEFAULT, DEFAULT, DEFAULT, DEFAULT);
END;
