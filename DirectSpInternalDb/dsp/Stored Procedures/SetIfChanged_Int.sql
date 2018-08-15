CREATE PROCEDURE [dsp].[SetIfChanged_Int]
    @ProcId INT, @PropName TSTRING, @NewValue BIGINT, @OldValue BIGINT OUT, @HasAccess BIT = NULL, @NullAsNotSet BIT = 0, @IsUpdated BIT = NULL OUT
AS
BEGIN
	SET @HasAccess = ISNULL(@HasAccess, 1);
    IF (dsp.Param_IsChanged(@OldValue, @NewValue, @NullAsNotSet) = 0)
        RETURN;

    IF (@HasAccess = 0) --
        EXEC err.ThrowAccessDeniedOrObjectNotExists @ProcId = @ProcId, @Message = 'PropName: {0}', @Param0 = @PropName;

    SET @IsUpdated = 1;
    SET @OldValue = @NewValue;
END;







