CREATE PROCEDURE [dsp].[System_Api]
    @Api TJSON = NULL OUT
WITH EXECUTE AS OWNER
AS
BEGIN
    EXEC dsp.Metadata_StoreProcedures @Json = @Api OUTPUT;
END;