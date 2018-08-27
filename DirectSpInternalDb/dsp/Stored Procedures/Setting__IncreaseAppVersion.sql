CREATE PROC [dsp].[Setting_$IncreaseAppVersion]
    @AppVersion TSTRING OUT, @ForceIncrease INT
AS
BEGIN
    DECLARE @Position INT;
    DECLARE @Part1 TSTRING;
    DECLARE @Part2 TSTRING;
    DECLARE @Part3 TSTRING;
    EXEC dsp.String_Tokenize @Expression = @AppVersion, @Delimeter = N'.', @Position = @Position OUTPUT, @Token = @Part1 OUTPUT;
    EXEC dsp.String_Tokenize @Expression = @AppVersion, @Delimeter = N'.', @Position = @Position OUTPUT, @Token = @Part2 OUTPUT;
    EXEC dsp.String_Tokenize @Expression = @AppVersion, @Delimeter = N'.', @Position = @Position OUTPUT, @Token = @Part3 OUTPUT;

    SET @Position = NULL;
    DECLARE @OldPart1 TSTRING;
    DECLARE @OldPart2 TSTRING;
    DECLARE @OldPart3 TSTRING;
    DECLARE @OldAppVersion TSTRING;
    EXEC dsp.Setting_Props @AppVersion = @OldAppVersion OUT;
    EXEC dsp.String_Tokenize @Expression = @OldAppVersion, @Delimeter = N'.', @Position = @Position OUTPUT, @Token = @OldPart1 OUTPUT;
    EXEC dsp.String_Tokenize @Expression = @OldAppVersion, @Delimeter = N'.', @Position = @Position OUTPUT, @Token = @OldPart2 OUTPUT;
    EXEC dsp.String_Tokenize @Expression = @OldAppVersion, @Delimeter = N'.', @Position = @Position OUTPUT, @Token = @OldPart3 OUTPUT;

	-- Check Star
    IF (@Part3 = N'*' OR @ForceIncrease = 1)
	BEGIN
		SET @Part3 = ISNULL(@OldPart3, N'0');
        SET @Part3 = dsp.Convert_ToString(CAST(@Part3 AS INT) + 1);
	END

    -- Check format
    SET @AppVersion = @Part1 + '.' + @Part2 + '.' + @Part3;
END;