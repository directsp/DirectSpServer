-- Create Function String
CREATE FUNCTION [dsp].[String_ReplaceEnter] (@Value TSTRING)
RETURNS TSTRING
AS
BEGIN
	RETURN REPLACE(@Value, N'\n', CHAR(/*No Codequality Error*/13) + CHAR(/*No Codequality Error*/10));
END;