CREATE PROCEDURE [dsp].[Database_InitInMemoryDatabase]
    @DbName TSTRING
AS
BEGIN
    SET @DbName = ISNULL(@DbName, DB_NAME());

    DECLARE @dbFile TSTRING;

    SELECT  @dbFile = mf.physical_name
      FROM  sys.master_files mf
     WHERE  mf.name = @DbName;

    DECLARE @InMemoryFile TSTRING = dsp.Path_RemoveExtension(@dbFile) + '_mod';

    -- Add FileGroup if not exists
    IF NOT EXISTS (SELECT   1 FROM  sys.filegroups WHERE name = 'InMemory_mod')
		ALTER DATABASE CURRENT ADD FILEGROUP InMemory_mod CONTAINS MEMORY_OPTIMIZED_DATA;

    -- Add File to file group if not added
    DECLARE @InMemoryName TSTRING = @DbName + '_mod1';
    DECLARE @Sql TSTRING = 'ALTER DATABASE CURRENT ADD FILE (name=''{InMemoryName}'', filename=''{InMemoryFile}'') TO FILEGROUP InMemory_mod';
    SET @Sql = REPLACE(@Sql, '{InMemoryFile}', @InMemoryFile);
    SET @Sql = REPLACE(@Sql, '{InMemoryName}', @InMemoryName);

    IF NOT EXISTS (SELECT   * FROM  sys.database_files WHERE name = @InMemoryName)
		EXEC sys.sp_executesql @stmt = @Sql;
END;




