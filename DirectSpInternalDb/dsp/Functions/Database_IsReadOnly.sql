CREATE FUNCTION [dsp].[Database_IsReadOnly] (@DatabaseName TSTRING)
RETURNS BIT
AS
BEGIN
    RETURN IIF(DATABASEPROPERTYEX(ISNULL(@DatabaseName, DB_NAME()), 'Updateability') = 'READ_ONLY', 1, 0);
END;


