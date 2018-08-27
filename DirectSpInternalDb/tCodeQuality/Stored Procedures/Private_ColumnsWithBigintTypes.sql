
CREATE PROC [tCodeQuality].[Private_ColumnsWithBigintTypes]
AS
BEGIN
    WITH BigIntColumns
        AS (SELECT  Tbl.name AS TableName, C.name AS ColumnName
              FROM  sys.columns AS C
                    INNER JOIN sys.types AS T ON T.user_type_id = C.user_type_id
                    INNER JOIN sys.tables AS Tbl ON Tbl.object_id = C.object_id
             WHERE  T.name = 'bigint')
    SELECT  DISTINCT S.name AS SchemaName, P.name AS ProcedureName, SM.definition, BC.ColumnName
      FROM  BigIntColumns AS BC, sys.procedures AS P
                                 INNER JOIN sys.schemas AS S ON S.schema_id = P.schema_id
                                 INNER JOIN sys.sql_modules AS SM ON SM.object_id = P.object_id
     WHERE  dsp.String_RemoveWhitespacesBig(REPLACE(SM.definition, 'INTO', '')) LIKE '%' + BC.ColumnName + 'INT%' --
        AND S.name <> 'tSQLt';
END;


