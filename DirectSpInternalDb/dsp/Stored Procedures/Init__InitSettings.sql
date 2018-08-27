CREATE PROC [dsp].[Init_$InitSettings]
AS
BEGIN
    SET NOCOUNT ON;

    ----------------
    -- Insert the only dsp.Settings record
    ----------------
    IF (NOT EXISTS (SELECT  1 FROM  dsp.Setting))
    BEGIN
        -- Report it is done
        EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = 'Creating default dsp.Settings';
        INSERT  dsp.Setting (SettingId)
        VALUES (1);
    END;

    ----------------
    -- Insert the only dbo.Settings record
    ----------------
    IF (NOT EXISTS (SELECT  1 FROM  dbo.Setting))
    BEGIN
        EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = 'Creating default dbo.Settings';
        INSERT  dbo.Setting (SettingId)
        VALUES (1);
    END;

END;