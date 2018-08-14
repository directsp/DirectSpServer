/*
#MetaStart 
	{
		"DataAccessMode": "Write"
	} 
#MetaEnd 
*/
CREATE PROCEDURE [api].[KeyValue_ValueSet]
    @Context TCONTEXT OUTPUT, @KeyName TSTRING, @TextValue TBIGSTRING, @TimeToLife INT = 0, @IsOverwrite BIT = 1
WITH EXECUTE AS OWNER
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dsp.Context_Verify @Context = @Context OUTPUT, @ProcId = @@PROCID;

    -- Check access
    EXEC dbo.Context_SimpleCheckAccess @Context = @Context OUTPUT;

    SET @KeyName = dsp.Formatter_FormatString(@KeyName);
    SET @TimeToLife = IIF(@TimeToLife = 0, NULL, @TimeToLife);
    SET @IsOverwrite = ISNULL(@IsOverwrite, 1);

    -- Set @ExpirationTime
    DECLARE @ExpirationTime DATETIME;
    IF (@TimeToLife IS NOT NULL)
        SET @ExpirationTime = DATEADD(SECOND, @TimeToLife, GETDATE());

    -- Clean expired values
    EXEC dbo.KeyValue_Flush;

    -- Check if the key is already exist
    IF (@IsOverwrite = 1)
    BEGIN
        UPDATE  dbo.KeyValue
           SET  TextValue = @TextValue, ExpirationTime = @ExpirationTime, ModifiedTime = GETDATE()
         WHERE  KeyName = @KeyName;

        IF (@@ROWCOUNT > 0)
            RETURN;
    END;

    -- Insert New
    BEGIN TRY
        INSERT INTO dbo.KeyValue (KeyName, TextValue, ExpirationTime)
        VALUES (@KeyName, @TextValue, @ExpirationTime);
    END TRY
    BEGIN CATCH
        IF (CHARINDEX('KeyName', ERROR_MESSAGE()) > 0 AND   ERROR_NUMBER() = 2627) --
            EXEC err.ThrowObjectAlreadyExists @ProcId = @@PROCID, @Message = 'Object already exist!';
    END CATCH;
END;



