CREATE PROCEDURE [dsp].[SetIfChanged_String]
	@IsUpdated BIT OUT, @OldValue TSTRING OUT, @NewValue TSTRING, @ExceptionId INT = NULL, @ExceptionMessage TSTRING = NULL,
	@NullAsNotSet BIT = 0
AS
BEGIN
	IF (dsp.Param_IsChanged(dsp.Convert_ToSqlvariant(@OldValue), dsp.Convert_ToSqlvariant(@NewValue), @NullAsNotSet) = 0)
		RETURN;

	IF (@ExceptionId IS NOT NULL) --
		EXEC dsp.ThrowAppException @ExceptionId = @ExceptionId, @Message = @ExceptionMessage;

	SET @IsUpdated = 1;
	SET @OldValue = @NewValue;
END;

