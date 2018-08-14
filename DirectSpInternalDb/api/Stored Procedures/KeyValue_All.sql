/*
#MetaStart 
	{
		"DataAccessMode": "ReadSnapshot"
	} 
#MetaEnd 
*/
CREATE PROCEDURE [api].[KeyValue_All]
    @Context TCONTEXT OUTPUT, @KeyNamePattern TSTRING = NULL
WITH EXECUTE AS OWNER
AS
BEGIN
    -- Verify Context
    EXEC dsp.Context_Verify @Context = @Context OUTPUT, @ProcId = @@PROCID;

    -- Check access
    EXEC dbo.Context_SimpleCheckAccess @Context = @Context OUTPUT;

    SET NOCOUNT ON;
    SET @KeyNamePattern = LTRIM(RTRIM(@KeyNamePattern));
    SET @KeyNamePattern = NULLIF(@KeyNamePattern, '');
    SET @KeyNamePattern = ISNULL(@KeyNamePattern, '%');

    SELECT  KV.KeyName, KV.TextValue, KV.ModifiedTime
      FROM  dbo.KeyValue AS KV
     WHERE  (KV.KeyName LIKE @KeyNamePattern) AND   (KV.ExpirationTime IS NULL OR   KV.ExpirationTime > GETDATE());
END;



