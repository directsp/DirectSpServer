CREATE PROCEDURE [dbo].[KeyValue_Flush]
AS
BEGIN
	DELETE	dbo.KeyValue
	WHERE	ExpirationTime IS NOT NULL AND	ExpirationTime <= GETDATE();
END;
