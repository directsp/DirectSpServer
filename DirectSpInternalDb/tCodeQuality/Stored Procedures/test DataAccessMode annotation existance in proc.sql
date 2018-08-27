CREATE PROCEDURE [tCodeQuality].[test DataAccessMode annotation existance in proc]
AS
BEGIN
    DECLARE @msg TBIGSTRING = --
            (   SELECT  CHAR(10) + VPD.SchemaName + '.' + VPD.ObjectName
                  FROM  dsp.Metadata_ProceduresDefination() AS VPD
                 WHERE  VPD.Type = 'P' AND  VPD.SchemaName = 'api' AND --
                        JSON_VALUE(dsp.Metadata_StoreProcedureAnnotation(VPD.SchemaName + '.' + VPD.ObjectName), '$.DataAccessMode') IS NULL
                FOR XML PATH(''));

    DECLARE @msg2 TBIGSTRING = --
            (   SELECT  CHAR(10) + VPD.SchemaName + '.' + VPD.ObjectName
                  FROM  dsp.Metadata_ProceduresDefination() AS VPD
                 WHERE  VPD.Type = 'P' AND  VPD.SchemaName = 'api' --
                    AND JSON_VALUE(dsp.Metadata_StoreProcedureAnnotation(VPD.SchemaName + '.' + VPD.ObjectName), '$.DataAccessMode') IS NOT NULL --
                    AND JSON_VALUE(dsp.Metadata_StoreProcedureAnnotation(VPD.SchemaName + '.' + VPD.ObjectName), '$.DataAccessMode') NOT IN ( 'Read', 'Write',
        'ReadSnapshot' )
                FOR XML PATH(''));

    IF (@msg IS NOT NULL) --		
        EXEC tSQLt.Fail @Message0 = @msg;

	IF (@msg2 IS NOT NULL)
	    EXEC tSQLt.Fail @Message0 = N'Procedures with wrong value of DataAccessMode', @Message1 = @msg2
END;


