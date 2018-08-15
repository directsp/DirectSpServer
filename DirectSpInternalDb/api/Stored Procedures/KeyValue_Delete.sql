/*
#MetaStart 
	{
		"DataAccessMode": "Write"
	} 
#MetaEnd 
*/
CREATE PROCEDURE [api].[KeyValue_Delete]
    @Context TCONTEXT OUTPUT, @KeyNamePattern TSTRING, @AffectedCount INT = NULL OUT
WITH EXECUTE AS OWNER
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dsp.Context_Verify @Context = @Context OUTPUT, @ProcId = @@PROCID;

    -- Check access
    EXEC dbo.Context_SimpleCheckAccess @Context = @Context OUTPUT;

    SET @KeyNamePattern = LTRIM(RTRIM(@KeyNamePattern));
    SET @KeyNamePattern = NULLIF(@KeyNamePattern, '');
    SET @KeyNamePattern = ISNULL(@KeyNamePattern, '%');

    -- Clean expired values
    DELETE  dbo.KeyValue
     WHERE  ExpirationTime IS NOT NULL AND  ExpirationTime <= GETDATE();

    -- Remove keys
    DELETE  dbo.KeyValue
     WHERE  KeyName LIKE @KeyNamePattern AND (ExpirationTime IS NULL OR ExpirationTime > GETDATE());

    SET @AffectedCount = @@ROWCOUNT;
END;