
/*
#MetaStart
{
	"DataAccessMode": "Write"
} 
#MetaEnd
*/
CREATE PROCEDURE [api].[User_CreateOrUpdateByAuthClaims]
    @Context TCONTEXT OUT, @AuthUserClaims TJSON
WITH EXECUTE AS OWNER
AS
BEGIN
    EXEC dsp.Context_Verify @Context = @Context OUTPUT, @ProcId = @@PROCID;
    EXEC err.ThrowNotSupported @ProcId = @@PROCID;
END