/*
#MetaStart 
	{
		"DataAccessMode": "Write"
	} 
#MetaEnd 
*/
CREATE PROCEDURE [api].[KeyValue_CreateByJson]
    @Context TCONTEXT OUTPUT, @Json TJSON
WITH EXECUTE AS OWNER
AS
BEGIN
    -- Verify Context
    EXEC dsp.Context_Verify @Context = @Context OUTPUT, @ProcId = @@PROCID;

    -- Check access
    EXEC dbo.Context_SimpleCheckAccess @Context = @Context OUTPUT;

    -- Clean expired values
    EXEC dbo.KeyValue_Flush;

    DECLARE @KeyValyeInfo TABLE (KeyName TSTRING,
        TextValue TBIGSTRING,
        TimeToLife INT,
        IsOverwrite BIT);

    INSERT INTO @KeyValyeInfo (KeyName, TextValue, TimeToLife, IsOverwrite)
    SELECT  KV.KeyName, KV.TextValue, KV.TimeToLife, KV.IsOverwrite
      FROM
        OPENJSON(@Json)
        WITH (KeyName TSTRING, TextValue TBIGSTRING, TimeToLife INT, IsOverwrite BIT) AS KV;

    DECLARE @TranCount INT = @@TRANCOUNT;
    IF (@TranCount = 0)
        BEGIN TRANSACTION;
    BEGIN TRY
        -- delete Old key
        DELETE  dbo.KeyValue
            FROM    @KeyValyeInfo AS KV
         WHERE  KV.IsOverwrite = 1 AND  KV.KeyName = KeyValue.KeyName;

        -- Insert new Key	
        INSERT INTO dbo.KeyValue (KeyName, TextValue, ExpirationTime)
        SELECT  KVI.KeyName, KVI.TextValue, DATEADD(SECOND, KVI.TimeToLife, GETDATE())
          FROM  @KeyValyeInfo AS KVI;

        IF (@TranCount = 0) COMMIT;
    END TRY
    BEGIN CATCH
        IF (CHARINDEX('KeyName', ERROR_MESSAGE()) > 0 AND   ERROR_NUMBER() = 2627) --
            EXEC err.ThrowObjectAlreadyExists @ProcId = @@PROCID, @Message = 'Object already exist!';

        IF (@TranCount = 0)
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;