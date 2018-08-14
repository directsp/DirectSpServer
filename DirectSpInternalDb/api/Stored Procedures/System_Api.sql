/*
#MetaStart
{
	"DataAccessMode": "ReadSnapshot"
} 
#MetaEnd
*/
CREATE PROCEDURE [api].[System_Api]
	@Context TCONTEXT OUTPUT, @Api TJSON = NULL OUT
WITH EXECUTE AS OWNER
AS
BEGIN
	SET NOCOUNT ON;
	EXEC dsp.Context_Verify @Context = @Context OUT, @ProcId = @@PROCID;

	-- Any user should have access to this procedure

	-- Call dsp
	EXEC dsp.System_Api @Api = @Api OUTPUT;
END;
