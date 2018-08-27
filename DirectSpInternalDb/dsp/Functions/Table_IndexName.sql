-- Returns:
-- 0: Ok
-- 1: Index not exists
-- 2: IndexName is incorrect 
CREATE FUNCTION [dsp].[Table_IndexName] (
	@TableName TSTRING,
	@ColumnName TSTRING)
RETURNS TSTRING
AS
BEGIN
	DECLARE @IndexName TSTRING;
	SELECT	@IndexName = ind.name
	FROM		sys.indexes ind
			INNER JOIN sys.index_columns ic ON ind.object_id = ic.object_id
										AND ind.index_id = ic.index_id
			INNER JOIN sys.columns col ON ic.object_id = col.object_id
									AND ic.column_id = col.column_id
			INNER JOIN sys.tables t ON ind.object_id = t.object_id
	WHERE	ind.is_primary_key = 0
			AND t.name = @TableName
			AND col.name = @ColumnName;

	RETURN @IndexName;
END;