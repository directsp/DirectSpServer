CREATE FUNCTION [dsp].[Log_FormatMessage] (
	@ProcId INT,
	@Message TSTRING,
	@Elipsis BIT = 0,
	@Param0 TSTRING = '<notset>',
	@Param1 TSTRING = '<notset>',
	@Param2 TSTRING = '<notset>',
	@Param3 TSTRING = '<notset>')
RETURNS TSTRING
AS
BEGIN
	-- Validate Inputs
	SET @Elipsis = ISNULL(@Elipsis, 0);

	-- Format Message
	SET @Message = dsp.Formatter_FormatMessage(@Message, @Param0, @Param1, @Param2, @Param3);

	-- Put Elipsis
	IF (@Elipsis = 1)
	BEGIN
		DECLARE @LastString TSTRING = SUBSTRING(@Message, LEN(@Message), 1);
		IF (@LastString NOT LIKE '[.?!:>;]')
			SET @Message = @Message + ' ...';
	END;

	-- Set Schema and ProcName
	DECLARE @ProcName TSTRING = ISNULL(OBJECT_NAME(@ProcId), '(NoSP)');
	DECLARE @SchemaName TSTRING = OBJECT_SCHEMA_NAME(@ProcId);
	IF (@SchemaName IS NOT NULL)
		SET @ProcName = @SchemaName + '.' + @ProcName;

	-- Format message 
	DECLARE @msg TSTRING = @ProcName + '> ' + @Message;
	RETURN @msg;
END;