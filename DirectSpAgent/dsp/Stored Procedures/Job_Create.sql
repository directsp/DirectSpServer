CREATE PROC [dsp].[Job_Create]
	@Command TSTRING, @JobName TSTRING, @Owner TSTRING = NULL, @DataBaseName TSTRING = NULL
AS
BEGIN
	BEGIN TRANSACTION;
	
	SET @Owner = ISNULL(@Owner, SYSTEM_USER);
	SET @DataBaseName = DB_NAME();
	DECLARE @ReturnCode INT;
	SELECT	@ReturnCode = 0;
	    
	IF NOT EXISTS (SELECT	name
						FROM msdb.dbo.syscategories
					WHERE	name = N'[Uncategorized (Local)]' AND	category_class = 1)
	BEGIN
		EXEC @ReturnCode = msdb.dbo.sp_add_category @class = N'JOB', @type = N'LOCAL', @name = N'[Uncategorized (Local)]';
		IF (@@ERROR <> 0 OR @ReturnCode <> 0)
			GOTO QuitWithRollback;
	END;
	
	DECLARE @jobId BINARY(16);
	EXEC @ReturnCode = msdb.dbo.sp_add_job @job_name = @JobName, @enabled = 1, @notify_level_eventlog = 0, @notify_level_email = 0, @notify_level_netsend = 0,
		@notify_level_page = 0, @delete_level = 1, @description = N'No description available.', @category_name = N'[Uncategorized (Local)]',
		@owner_login_name = @Owner, @job_id = @jobId OUTPUT;
	IF (@@ERROR <> 0 OR @ReturnCode <> 0)
		GOTO QuitWithRollback;
	EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id = @jobId, @step_name = N'FirstStep', @step_id = 1, @cmdexec_success_code = 0, @on_success_action = 1,
		@on_success_step_id = 0, @on_fail_action = 2, @on_fail_step_id = 0, @retry_attempts = 0, @retry_interval = 0, @os_run_priority = 0,
		@subsystem = N'TSQL', @command = @Command, @database_name = @DataBaseName, @flags = 0;
	IF (@@ERROR <> 0 OR @ReturnCode <> 0)
		GOTO QuitWithRollback;
	EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1;
	IF (@@ERROR <> 0 OR @ReturnCode <> 0)
		GOTO QuitWithRollback;

	EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @jobId, @server_name = N'(local)';
	IF (@@ERROR <> 0 OR @ReturnCode <> 0)
		GOTO QuitWithRollback;
	COMMIT TRANSACTION;
	GOTO EndSave;
	QuitWithRollback:
	IF (@@TRANCOUNT > 0)
		ROLLBACK TRANSACTION;
	EndSave:
END;