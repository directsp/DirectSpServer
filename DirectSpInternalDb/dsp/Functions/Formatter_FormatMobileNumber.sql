CREATE FUNCTION [dsp].[Formatter_FormatMobileNumber] ( @MobileNumber TSTRING )
RETURNS TSTRING
BEGIN
    SET @MobileNumber = REPLACE(@MobileNumber, ' ', '');
    SET @MobileNumber = REPLACE(@MobileNumber, '-', '');
    SET @MobileNumber = REPLACE(@MobileNumber, '.', '');
    
	-- 9124445566
    IF ( LEN(@MobileNumber) = 10 )
        SET @MobileNumber = '+98' + @MobileNumber;

	-- 09124445566
    IF ( LEN(@MobileNumber) = 11 AND SUBSTRING(@MobileNumber, 1, 1) = '0' )
        SET @MobileNumber = '+98' + SUBSTRING(@MobileNumber, 2, 10);

	-- 989124445566
    IF ( LEN(@MobileNumber) > 10 AND SUBSTRING(@MobileNumber, 1, 1) <> '+' )
        SET @MobileNumber = '+' + @MobileNumber;

	-- Remove plus
    SET @MobileNumber = SUBSTRING(@MobileNumber, 2, LEN(@MobileNumber) - 1);

	-- checking length >=11 amd <=13
    IF ( LEN(@MobileNumber) NOT BETWEEN 11 AND 13 )
        RETURN NULL;

    IF ( @MobileNumber LIKE '%[^0-9]%'  )
        SET @MobileNumber = NULL;

    RETURN @MobileNumber;
END;