CREATE PROC [dsp].[Init]
    @IsProductionEnvironment BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
	EXEC dsp.[Init_$Start] @IsProductionEnvironment = @IsProductionEnvironment, @IsWithCleanup = NULL, @Reserved = NULL
    
END;

