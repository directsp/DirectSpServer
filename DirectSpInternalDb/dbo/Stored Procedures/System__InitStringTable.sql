
CREATE PROC [dbo].[System_$InitStringTable]
AS
BEGIN
	SET NOCOUNT ON;
	DELETE	dbo.StringTable;

	-- Recreating String Functions
	EXEC dsp.RecreateStringFunctions;
END;