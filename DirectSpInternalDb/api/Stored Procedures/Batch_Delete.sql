/*
#MetaStart 
	{
		"DataAccessMode": "Delete"
	} 
#MetaEnd 
*/
CREATE PROCEDURE [api].[Batch_Delete]
	@Context TCONTEXT OUTPUT, @BatchId INT
WITH EXECUTE AS OWNER
AS
BEGIN
	SET NOCOUNT ON;
	EXEC dsp.Context_Verify @Context = @Context OUTPUT, @ProcId = @@PROCID;

	-- Validating
	DECLARE @OwnerUserId INT;
	SELECT	@OwnerUserId = B.OwnerUserId
	FROM	dbo.Batch AS B
	WHERE	B.BatchId = @BatchId;

	-- Check access
	EXEC dbo.Context_SimpleCheckAccess @Context = @Context OUTPUT, @UserId = @OwnerUserId	

	-- Delete batch
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = 'Delete batch';

	DELETE	dbo.Batch
	WHERE	BatchId = @BatchId;

END;



