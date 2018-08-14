CREATE FUNCTION [dsp].[Database_IsReadOnly] (@DatabaseName TSTRING)
RETURNS BIT
AS
BEGIN
    RETURN IIF(DATABASEPROPERTYEX(@DatabaseName, 'Updateability') = 'READ_ONLY', 1, 0);
END;

