

CREATE PROCEDURE [dsp].[Table_InitTemporal]
	@Schema TSTRING = 'dbo', @TableName TSTRING, @Enable BIT = 1
AS
BEGIN
	DECLARE @Msg TSTRING = ERROR_MESSAGE();

	-- Check SystemVersion is On over the table
	DECLARE @temporal_type INT = (SELECT temporal_type FROM sys.tables WHERE name = @TableName);

	DECLARE @TableNameHistory TSTRING = @TableName + 'History';

	-- Enable System Versioning
	IF (@Enable = 1)
	BEGIN
		-- do not enable if it was already on
		IF (@temporal_type != 0)
			RETURN;

		-- check if table is empty
		DECLARE @RowCount INT = 0;
		DECLARE @Query TSTRING = 'SELECT @Exists = COUNT(*) FROM ' + @Schema + '.' + @TableName + '';
		EXEC sys.sp_executesql @Query, N'@Exists int out', @RowCount OUT;
		IF (@RowCount > 1)
		BEGIN
			SET @Msg = 'To InitTemporal table please empty the table (' + @Schema + '.' + @TableName + ')';
			EXEC err.ThrowGeneralException @ProcId = @@PROCID, @Message = @Msg;
		END;

		-- Drop if exits
		EXEC ('ALTER TABLE ' + @Schema + '.' + @TableName + ' DROP CONSTRAINT IF EXISTS DF_' + @TableName + 'SysStartTime;');
		EXEC ('ALTER TABLE ' + @Schema + '.' + @TableName + ' DROP CONSTRAINT IF EXISTS DF_' + @TableName + 'SysEndTime;');
		EXEC ('ALTER TABLE ' + @Schema + '.' + @TableName + ' DROP COLUMN IF EXISTS SysStartTime;');
		EXEC ('ALTER TABLE ' + @Schema + '.' + @TableName + ' DROP COLUMN IF EXISTS SysEndTime;');

		-- Apply Temporal
		EXEC ('ALTER TABLE ' + @Schema + '.' + @TableName + ' ADD SysStartTime DATETIME2(7) GENERATED ALWAYS AS ROW START HIDDEN constraint DF_' + @TableName + 'SysStartTime DEFAULT SYSUTCDATETIME(), SysEndTime DATETIME2(7) GENERATED ALWAYS AS ROW END HIDDEN constraint DF_' + @TableName + 'SysEndTime DEFAULT ''9999.12.31 23:59:59.99'' , PERIOD FOR SYSTEM_TIME (SysStartTime, SysEndTime);');
		EXEC ('ALTER TABLE ' + @Schema + '.' + @TableName + ' SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = ' + @Schema + '.' + @TableNameHistory + ')) ');
	END;
	ELSE
	BEGIN
		-- Set SYSTEM_VERSIONING off if is on
		EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Trying to SET the SYSTEM_VERSIONING OFF. Table: {0}', @Param0 = @TableName;
		BEGIN TRY
			EXEC ('ALTER TABLE ' + @Schema + '.' + @TableName + ' SET (SYSTEM_VERSIONING = OFF)');
		END TRY
		BEGIN CATCH
			SET @Msg = ERROR_MESSAGE();
			EXEC dsp.Log_Error @ProcId = @@PROCID, @Message = @Msg;
		END CATCH;

		--  Drop PERIOD life time
		EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Dropping PERIOD FOR SYSTEM_TIME; Table: {0}', @Param0 = @TableName;
		BEGIN TRY
			EXEC ('ALTER TABLE ' + @Schema + '.' + @TableName + ' DROP PERIOD FOR SYSTEM_TIME;');
		END TRY
		BEGIN CATCH
			SET @Msg = ERROR_MESSAGE();
			EXEC dsp.Log_Error @ProcId = @@PROCID, @Message = @Msg;
		END CATCH;

	-- Drop TableHistory
	-- EXEC ('DROP TABLE ' + @Schema + '.' + @TableNameHistory);

	-- Drop SysStartTime constraint
	-- EXEC ('ALTER TABLE ' + @Schema + '.' + @TableName + ' DROP CONSTRAINT DF_' + @TableName + 'SysStartTime;');

	-- Drop SysEndTime constraint
	-- EXEC('ALTER TABLE ' + @Schema + '.' + @TableName + ' DROP CONSTRAINT DF_'+ @TableName+ 'SysEndTime;');

	-- Drop columns SysStartTime in disbale mode
	-- EXEC ('ALTER TABLE ' + @Schema + '.' + @TableName + ' DROP COLUMN SysStartTime;');

	-- Drop columns SysEndTime
	-- EXEC('ALTER TABLE ' + @Schema + '.' + @TableName + ' DROP COLUMN SysEndTime;');
	END;


END;