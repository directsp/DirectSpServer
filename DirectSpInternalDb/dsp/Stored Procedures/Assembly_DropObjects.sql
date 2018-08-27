CREATE PROCEDURE [dsp].[Assembly_DropObjects]
	@AssemblyName TSTRING
AS
BEGIN
	DECLARE @sql TBIGSTRING = N'';
	DECLARE @assembly_name TBIGSTRING = 'IcDbHelper';
	SELECT	@sql += 'DROP ' + CASE
								WHEN O.type = 'PC'
									THEN 'PROCEDURE ' ELSE 'FUNCTION '
							END + QUOTENAME(O.name) + ';'
	FROM	sys.assemblies AS A
			INNER JOIN sys.assembly_modules AS AM ON A.assembly_id = AM.assembly_id
			INNER JOIN sys.objects AS O ON AM.object_id = O.object_id
	WHERE	A.name = @assembly_name;
	SET @sql = @sql + 'DROP ASSEMBLY ' + QUOTENAME(@assembly_name);

	EXEC sys.sp_executesql @sql;

END;