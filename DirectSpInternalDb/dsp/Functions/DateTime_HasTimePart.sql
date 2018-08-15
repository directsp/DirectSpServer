create FUNCTION [dsp].[DateTime_HasTimePart] (@Time DATETIME)
RETURNS BIT
AS
BEGIN
	RETURN IIF(@Time = CAST(@Time AS DATE), 0, 1);
END;