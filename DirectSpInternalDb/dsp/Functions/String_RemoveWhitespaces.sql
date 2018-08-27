CREATE FUNCTION [dsp].[String_RemoveWhitespaces] ( @String TSTRING )
RETURNS TSTRING
AS
BEGIN
    RETURN REPLACE(REPLACE(REPLACE(REPLACE(@String, ' ', ''), CHAR(13), ''), CHAR(10), ''), CHAR(9), '');
END;