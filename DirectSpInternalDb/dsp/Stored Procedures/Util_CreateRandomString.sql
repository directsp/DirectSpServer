CREATE PROCEDURE [dsp].[Util_CreateRandomString]
    @Length INT,
    @IncludeLetter BIT = 1 ,
    @IncludeDigit BIT = 1 ,
    @randomString TSTRING OUT
AS
BEGIN
    DECLARE @counter INT;
    DECLARE @nextChar CHAR(1);
    SET @counter = 1;
    SET @randomString = '';

    WHILE @counter <= @Length
    BEGIN
	   IF (@IncludeLetter=1 AND @IncludeDigit=1)  SET  @nextChar = CHAR(48 + CONVERT(INT, ( 122 - 48  + 1 ) * RAND())); -- 0 to z
	   ELSE IF (@IncludeLetter=1)  SET  @nextChar = CHAR(65 + CONVERT(INT, ( 122 - 65 + 1 ) * RAND())); -- a to z
	   ELSE IF (@IncludeDigit=1)  SET  @nextChar = CHAR(48 + CONVERT(INT, ( 57 - 48 + 1 ) * RAND())); -- 0 to 9
	   ELSE RETURN NULL

        IF ASCII(@nextChar) NOT IN ( 58, 59, 60, 61, 62, 63, 64, 91, 92, 93, 94, 95, 96 )
        BEGIN
            SELECT  @randomString = @randomString + @nextChar;
            SET @counter = @counter + 1;
        END;
    END;
END;