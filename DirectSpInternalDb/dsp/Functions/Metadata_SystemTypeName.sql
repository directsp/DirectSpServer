CREATE FUNCTION [dsp].[Metadata_SystemTypeName] (@TypeName TSTRING)
RETURNS TSTRING
AS
BEGIN
	DECLARE @SystemName TSTRING = @TypeName;

	IF EXISTS (SELECT		1
					FROM	sys.types AS T
				WHERE	T.name = @SystemName AND T.is_user_defined = 1)
	BEGIN
		SELECT	DISTINCT @SystemName = bt.name
		FROM	sys.syscolumns c
				INNER JOIN sys.systypes st ON st.xusertype = c.xusertype
				INNER JOIN sys.systypes bt ON bt.xusertype = c.xtype
		WHERE	st.name = @TypeName;
	END;

	RETURN @SystemName;
END;