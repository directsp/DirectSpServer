CREATE PROC [dsp].[Init_Cleanup]
AS
BEGIN
    SET NOCOUNT ON;
    ----------------
    -- Check Production Environment and Run Cleanup
    ----------------
    EXEC dsp.[Init_$Cleanup] @IsProductionEnvironment = 0, @IsWithCleanup = 1;

    -- Report it is done
    EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = 'System has been cleaned up!.';
END;















