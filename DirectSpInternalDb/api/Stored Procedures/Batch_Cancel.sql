/*
#MetaStart 
	{
		"DataAccessMode": "Write"
	} 
#MetaEnd 
*/
CREATE PROCEDURE [api].[Batch_Cancel]
	@Context TCONTEXT OUTPUT, @BatchId INT
WITH EXECUTE AS OWNER
AS
BEGIN
	SET NOCOUNT ON;
	EXEC dsp.Context_Verify @Context = @Context OUTPUT, @ProcId = @@PROCID;

	-- Validating 
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = 'Validating';
	DECLARE @OwnerUserId INT;
	SELECT	@OwnerUserId = B.OwnerUserId
	FROM	dbo.Batch AS B
	WHERE	B.BatchId = @BatchId;

	-- Check access
	EXEC dbo.Context_SimpleCheckAccess @Context = @Context OUTPUT, @UserId = @OwnerUserId	

	-- Update batch
	EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = 'Update batch';

	UPDATE	dbo.Batch
	SET IsCanceled = 1
	WHERE	BatchId = @BatchId;


END;