CREATE PROCEDURE [dsp].[SetIfChanged_Decimal]
    @ProceId INT, @PropName TSTRING, @NewValue DECIMAL, @OldValue DECIMAL OUT, @HasAccess BIT = 1, @NullAsNotSet BIT = 0, @IsUpdated BIT OUT
AS
BEGIN
    SET @HasAccess = ISNULL(@HasAccess, 0);

    IF (dsp.Param_IsChanged(@OldValue, @NewValue, @NullAsNotSet) = 0)
        RETURN;

    IF (@HasAccess = 0) --
        EXEC err.ThrowAccessDeniedOrObjectNotExists @ProcId = @ProceId, @Message = 'PropName: {0}', @Param0 = @PropName;

    SET @IsUpdated = 1;
    SET @OldValue = @NewValue;
END;
