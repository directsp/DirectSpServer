CREATE PROCEDURE [dsp].[ThrowException]
	@Exception TJSON
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ExceptionId INT = JSON_VALUE(@Exception, '$.errorId');
	THROW @ExceptionId, @Exception, 1;
END;