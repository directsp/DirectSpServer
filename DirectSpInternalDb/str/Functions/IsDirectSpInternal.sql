
CREATE FUNCTION str.IsDirectSpInternal() 
RETURNS TSTRING
AS 
BEGIN
	RETURN dsp.StringTable_Value('IsDirectSpInternal');
END