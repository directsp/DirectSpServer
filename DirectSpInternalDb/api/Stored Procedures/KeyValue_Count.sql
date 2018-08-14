/*
#MetaStart 
	{
		"DataAccessMode": "ReadSnapshot"
	} 
#MetaEnd 
*/
CREATE PROCEDURE [api].[KeyValue_Count]
    @Context TCONTEXT OUTPUT, @KeyNamePattern TSTRING = NULL, @Count INT = NULL OUT
WITH EXECUTE AS OWNER
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dsp.Context_Verify @Context = @Context OUTPUT, @ProcId = @@PROCID;

    -- Check access
    EXEC dbo.Context_SimpleCheckAccess @Context = @Context OUTPUT;

    SET @KeyNamePattern = dsp.Formatter_FormatString(@KeyNamePattern);
    SET @KeyNamePattern = ISNULL(@KeyNamePattern, '%');

    -- Find key count
    EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Find key count';
    SELECT  @Count = COUNT(1)
      FROM  dbo.KeyValue AS KV
     WHERE  KV.KeyName LIKE @KeyNamePattern AND (KV.ExpirationTime IS NULL OR   KV.ExpirationTime > GETDATE());
END;


