CREATE PROCEDURE [dsp].[Crypt_CreatePassword]
    @Password TSTRINGA, -- NO Unicode Support
    @Algorithm TSTRING = NULL,
    @Iteration INT = 30000,
    @Salt TSTRINGA = NULL , -- NO Unicode Support
    @PasswordString TSTRING OUT
AS
BEGIN
    SET @Algorithm = LOWER(ISNULL(@Algorithm, dsp.Const_Algorithm_PBKDF2_SHA512()));
    IF ( @Algorithm = LOWER(dsp.Const_Algorithm_PBKDF2_SHA512()) )
    BEGIN
    
        IF ( @Salt IS NULL )
            EXEC dsp.Util_CreateRandomString @Length = 20, @randomString = @Salt OUTPUT;
        DECLARE @PasswordHash TSTRING;
        SET @PasswordHash = dsp.Convert_BinaryToBase64(dsp.CRYPT_PBKDF2_VARBINARY_SHA512(CAST(@Password AS VARBINARY(MAX)), CAST(@Salt AS VARBINARY(MAX)),
                                                                                           @Iteration, 64));
	   SET @PasswordString = @Algorithm + '$' + dsp.Convert_ToString(@Iteration) + '$' + @Salt + '$' + @PasswordHash;
    END;
END;