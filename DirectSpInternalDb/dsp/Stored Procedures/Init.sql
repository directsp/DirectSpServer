CREATE PROC [dsp].[Init]
    @IsProductionEnvironment BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TranCount INT = @@TRANCOUNT;
    IF (@TranCount = 0)
        BEGIN TRANSACTION;
    BEGIN TRY
        EXEC dsp.[Init_$Start] @IsProductionEnvironment = @IsProductionEnvironment, @IsWithCleanup = NULL, @Reserved = NULL;

        IF (@TranCount = 0) COMMIT;
    END TRY
    BEGIN CATCH
        IF (@TranCount = 0)
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;

