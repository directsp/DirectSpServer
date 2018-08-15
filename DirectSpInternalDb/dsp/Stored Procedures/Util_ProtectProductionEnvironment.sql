
CREATE PROC [dsp].[Util_ProtectProductionEnvironment]
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @IsProductionEnvironment BIT;
    EXEC dsp.Setting_Props @IsProductionEnvironment = @IsProductionEnvironment OUTPUT;
    IF (@IsProductionEnvironment = 1) --
        EXEC err.ThrowAccessDeniedOrObjectNotExists @ProcId = @@PROCID, @Message = 'This operation can not be executed in ProductionEnvironment!';
END;