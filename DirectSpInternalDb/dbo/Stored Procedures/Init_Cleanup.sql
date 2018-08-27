
CREATE PROC [dbo].[Init_Cleanup]
AS
BEGIN
    SET NOCOUNT ON;

    -- Production protection
	EXEC dsp.Util_ProtectProductionEnvironment;

    -- Delete Junction Tables 

    -- Delete base tables 
    DELETE  dbo.KeyValue;
    DELETE  dbo.BatchItem;

    -- Delete Lookup tables (may not required)
    DELETE  dbo.BatchItemResultType;

END;