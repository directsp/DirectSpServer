/*
#MetaStart 
	{
		"DataAccessMode": "ReadSnapshot"
	} 
#MetaEnd 
*/
CREATE PROCEDURE [api].[KeyValue_Value]
    @Context TCONTEXT OUTPUT, @KeyName TSTRING, @ThrowErrorIfNotExists BIT = 1, @TextValue TBIGSTRING = NULL OUT, @ModifiedTime DATETIME = NULL OUT,
    @IsExist BIT = NULL OUT
WITH EXECUTE AS OWNER
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dsp.Context_Verify @Context = @Context OUTPUT, @ProcId = @@PROCID;

    -- Check permission
    EXEC dbo.Context_SimpleCheckAccess @Context = @Context OUT;

    SET @KeyName = dsp.Formatter_FormatString(@KeyName);
    SET @IsExist = 0;
    SET @TextValue = NULL;
    SET @ModifiedTime = NULL;

    -- Find key
    SELECT  @TextValue = KV.TextValue, @ModifiedTime = KV.ModifiedTime, @IsExist = 1
      FROM  dbo.KeyValue AS KV
     WHERE  KV.KeyName = @KeyName AND   (KV.ExpirationTime IS NULL OR   KV.ExpirationTime > GETDATE());

    IF (@IsExist = 0 AND @ThrowErrorIfNotExists = 1) --
        EXEC dsp.ThrowAppException @ProcId = @@PROCID, @ExceptionId = 55002;
END;



