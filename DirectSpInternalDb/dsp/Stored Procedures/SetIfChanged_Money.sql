CREATE PROCEDURE [dsp].[SetIfChanged_Money]
	@IsUpdated BIT OUT, @OldValue MONEY OUT, @NewValue MONEY, @ExceptionId INT = NULL, @ExceptionMessage TSTRING = NULL, @NullAsNotSet BIT = 0
AS
BEGIN
	IF (dsp.Param_IsChanged(@OldValue, @NewValue, @NullAsNotSet) = 0)
		RETURN;

	IF (@ExceptionId IS NOT NULL) EXEC dsp.ThrowAppException @ExceptionId = @ExceptionId, @Message = @ExceptionMessage;

	SET @IsUpdated = 1;
	SET @OldValue = @NewValue;
END;



