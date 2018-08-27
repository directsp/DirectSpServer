CREATE PROCEDURE [dsp].[String_Tokenize]
	@Expression TSTRING, @Delimeter TSTRING, @Position INT OUTPUT, @Token TSTRING OUTPUT
AS
BEGIN
	IF (@Position = 0)
	BEGIN
		SET @Token = NULL;
		RETURN;
	END;
	
	SET @Delimeter = ISNULL(@Delimeter, '/');
	
	SET @Position = ISNULL(@Position, 1);
	DECLARE @DelimiterIndex INT = CHARINDEX(@Delimeter, @Expression, @Position);
	IF (@DelimiterIndex = 0)
		SET @DelimiterIndex = LEN(@Expression) - @DelimiterIndex + 1;

	SET @Token = SUBSTRING(@Expression, @Position, @DelimiterIndex - @Position);
	
	SET @Position = @DelimiterIndex + 1;
	IF (@Position > LEN(@Expression))
		SET @Position = 0;
END;