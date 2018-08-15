CREATE PROC [dsp].[Job_Delete]
	@JobNamePattern TSTRING
AS
BEGIN
	DECLARE DeleteJob_Cursor CURSOR FAST_FORWARD FORWARD_ONLY FORWARD_ONLY LOCAL FOR
	SELECT	S.name AS JobName
	FROM msdb.dbo.sysjobs AS S
	WHERE	S.name LIKE @JobNamePattern;

	OPEN DeleteJob_Cursor;

	DECLARE @JobName TSTRING;
	FETCH NEXT FROM DeleteJob_Cursor
	INTO @JobName;

	WHILE (@@FETCH_STATUS = 0)
	BEGIN
		BEGIN TRY
			EXEC msdb.dbo.sp_delete_job @job_name = @JobName;
		END TRY
		BEGIN CATCH

		END CATCH;

		FETCH NEXT FROM DeleteJob_Cursor
		INTO @JobName;
	END;
	CLOSE DeleteJob_Cursor
	DEALLOCATE DeleteJob_Cursor
END;