
CREATE PROC [dsp].[Init_$RecreateExceptionFunctions]
AS
BEGIN
    SET NOCOUNT ON;
    -- Drop err Functions and procedures
    EXEC dsp.Schema_DropObjects @SchemaName = 'err', @DropFunctions = 1, @DropProcedures = 1;

    -- Recreate err constant functions
    EXEC dsp.Init_RecreateEnumFunctions @SchemaName = 'err', @TableSchemaName = 'dsp', @TableName = 'Exception', @KeyColumnName = 'ExceptionId',
        @TextColumnName = 'ExceptionName', @FunctionNamePostfix = 'Id';

    -- Recreate Throw procedures
    DECLARE @ExceptionId INT;
    DECLARE @ExceptionName TSTRING;

    DECLARE CreateFunctionCursor CURSOR LOCAL FAST_FORWARD READ_ONLY FOR SELECT E.ExceptionName, E.ExceptionId FROM dsp.Exception AS E;
    OPEN CreateFunctionCursor;
    WHILE (1 = 1)
    BEGIN
        FETCH NEXT FROM CreateFunctionCursor
         INTO @ExceptionName, @ExceptionId;
        IF (@@FETCH_STATUS <> 0)
            BREAK;

        DECLARE @Sql TSTRING =
            '
CREATE PROC err.Throw{@ExceptionName} @ProcId INT, @Message TSTRING = NULL, @Param0 TSTRING = ''<notset>'', @Param1 TSTRING = ''<notset>'', @Param2 TSTRING = ''<notset>'', @Param3 TSTRING = ''<notset>''
AS
BEGIN
    DECLARE @ExceptionId INT = err.{@ExceptionName}Id();
    EXEC dsp.ThrowAppException @ProcId = @ProcId, @ExceptionId = @ExceptionId, @Message = @Message, @Param0 = @Param0, @Param1 = @Param1,
        @Param2 = @Param2, @Param3 = @Param3;
END'    ;
        SET @Sql = REPLACE(@Sql, '{@ExceptionName}', @ExceptionName);
        EXEC (@Sql);
    END;
    CLOSE CreateFunctionCursor;
    DEALLOCATE CreateFunctionCursor;

END;