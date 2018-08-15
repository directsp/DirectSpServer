CREATE PROCEDURE [tCodeQuality].[test API must have WITH EXECUTE AS OWNER]
AS
BEGIN
    -- Declaring pattern
    DECLARE @Pattern_WithExecuteASOwner TSTRING = dsp.String_RemoveWhitespaces('WITH EXECUTE AS OWNER');
    DECLARE @Pattern_WithExecASOwner TSTRING = dsp.String_RemoveWhitespaces('WITH EXEC AS OWNER');
    DECLARE @msg TSTRING;

    -- Getting list all procedures with pagination
    EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Getting list all procedures with pagination';
    DECLARE @t TABLE (SchemaName TSTRING,
        ProcedureName TSTRING,
        Script TBIGSTRING);

    INSERT INTO @t
    SELECT  PD.SchemaName, PD.ObjectName AS ProcedureName, dsp.String_RemoveWhitespacesBig(PD.Script)
      FROM  dsp.Metadata_ProceduresDefination() AS PD
     WHERE  PD.SchemaName = 'api';

    -- Looking for "With Execute AS Owner" phrase
    EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = N'Looking for "With Execute AS Owner" phrase';
    SET @msg = (   SELECT   SchemaName + '.' + ProcedureName AS ProcedureName
                     FROM   @t
                    WHERE   CHARINDEX(@Pattern_WithExecuteASOwner, Script) = 0 AND  CHARINDEX(@Pattern_WithExecASOwner, Script) = 0
                   FOR JSON AUTO);

    IF (@msg IS NOT NULL) --
        EXEC tSQLt.Fail @Message0 = @msg;
END;











