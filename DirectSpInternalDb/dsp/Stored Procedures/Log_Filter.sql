CREATE PROCEDURE [dsp].[Log_Filter]
	@Filter TSTRING = NULL
AS
BEGIN
	SET @Filter = NULLIF(@Filter, '');

	-- remove all old filters
	EXEC dsp.Log_RemoveFilter @Filter = NULL;
	IF (@Filter IS NULL)
		RETURN;

	-- set new filter
	EXEC dsp.Log_AddFilter @Filter = @Filter, @IsExclude = 0;
END;