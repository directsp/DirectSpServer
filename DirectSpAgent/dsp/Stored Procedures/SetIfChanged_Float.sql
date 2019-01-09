CREATE PROCEDURE [dsp].[SetIfChanged_Float]
    @ProcId INT, @PropName TSTRING, @NewValue FLOAT, @OldValue FLOAT OUT, @HasAccess BIT = 1, @NullAsNotSet BIT = 0, @IsUpdated BIT OUT
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





