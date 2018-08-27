CREATE PROCEDURE [dsp].[Log_AddFilter]
    @Filter TSTRING = NULL,
    @IsExclude BIT = 0
AS
BEGIN
	SET NOCOUNT ON;
	SET @IsExclude = ISNULL(@IsExclude, 0);

	-- Enable the Log System
	IF (dsp.Log_IsEnabled() = 0)
		EXEC dsp.Log_Enable;
	
	-- Clear Filters
	IF (@Filter IS NULL AND @IsExclude = 1)
	BEGIN
		DELETE dsp.LogFilterSetting WHERE UserName = SYSTEM_USER AND IsExludedFilter = 1;
		PRINT 'LogSystem> All exclude filters have been removed.';
		RETURN;
	END

	IF (@Filter IS NULL AND @IsExclude = 0)
	BEGIN
		DELETE dsp.LogFilterSetting WHERE UserName = SYSTEM_USER AND IsExludedFilter = 0;
		PRINT 'LogSystem> All include filters have been removed.';
		RETURN;
	END

	-- Insert or Update Filters
	IF EXISTS( SELECT 1 FROM dsp.LogFilterSetting AS LFS WHERE LFS.Log_Filter = @Filter)
	BEGIN
		UPDATE dsp.LogFilterSetting SET IsExludedFilter = @IsExclude WHERE Log_Filter = @Filter AND UserName = SYSTEM_USER;
		PRINT 'LogSystem> Filter has been updated.';
	END
	ELSE 
	BEGIN
		INSERT dsp.LogFilterSetting ( UserName, IsExludedFilter, Log_Filter )
		VALUES  (SYSTEM_USER, @IsExclude, @Filter);
		PRINT 'LogSystem> Filter has been added.';
	END

END