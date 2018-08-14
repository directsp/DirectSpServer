
CREATE PROC [dbo].[Init_FillExceptions]
AS
BEGIN
    SET NOCOUNT ON;

    -- delclare your application exceptions here. NOTE: ExceptionId must be started from 56000
    INSERT  dsp.Exception (ExceptionId, ExceptionName, Description)
    VALUES (55700, N'BatchItemResultIsTooLarge', N'');
END;

