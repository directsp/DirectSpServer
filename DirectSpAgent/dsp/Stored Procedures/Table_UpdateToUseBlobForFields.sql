CREATE PROCEDURE [dsp].[Table_UpdateToUseBlobForFields]
AS
BEGIN
	DECLARE @TableName TSTRING;

	DECLARE Cursor_TableName CURSOR LOCAL FAST_FORWARD READ_ONLY FOR
	SELECT	T2.name
	FROM	sys.tables AS T2
			INNER JOIN sys.columns c ON c.object_id = T2.object_id
			INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
			LEFT OUTER JOIN sys.index_columns ic ON ic.object_id = c.object_id AND	ic.column_id = c.column_id
			LEFT OUTER JOIN sys.indexes i ON ic.object_id = i.object_id AND ic.index_id = i.index_id
			INNER JOIN sys.schemas AS S ON S.schema_id = T2.schema_id
	WHERE	c.max_length = -1 AND	t.name = 'nvarchar' AND S.name = 'dbo'
	GROUP BY T2.name;
	OPEN Cursor_TableName;
	FETCH NEXT FROM Cursor_TableName
	INTO @TableName;

	WHILE (@@FETCH_STATUS = 0)
	BEGIN
		IF (@TableName IS NOT NULL) --
			EXEC sys.sp_tableoption @TableName, 'large value types out of row', 'ON';
		FETCH NEXT FROM Cursor_TableName
		INTO @TableName;
	END;
	CLOSE Cursor_TableName;
	DEALLOCATE Cursor_TableName;
END;