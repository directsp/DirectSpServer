
CREATE FUNCTION [dsp].[Metadata_StoreProcedureAnnotation]
(
  @StoreProcedureName TSTRING
)
RETURNS TSTRING
AS
BEGIN
	--format: 
	/*
	#MetaStart 
		{
			"ExecuteMode": "Write|ReadSync|ReadSnapshot|ReadWise", 
			"CommandTimeout": -1,
			"IsBatchAllowed": false, 
			"CaptchaMode": "Manual|Always|Auto",
			"Params": {"ParamName": {"IsUseMoneyConversionRate": true}},
			"Fields": {"FieldName": {"IsUseMoneyConversionRate": true}}
		} 
	#MetaEnd 
	*/
	
	-- Get script
    DECLARE @Script TSTRING;
    SELECT  @Script = definition
    FROM    sys.sql_modules
    WHERE   [object_id] = OBJECT_ID(@StoreProcedureName);

	--find start of text
    DECLARE @MetaStartIndex INT = CHARINDEX('#MetaStart', @Script);
    IF ( @MetaStartIndex = 0 )
        RETURN NULL;
    SET @MetaStartIndex = @MetaStartIndex + LEN('#MetaStart');

    DECLARE @MetaEndIndex INT = CHARINDEX('#MetaEnd', @Script, @MetaStartIndex);
    IF ( @MetaEndIndex = 0 )
        RETURN NULL;

    RETURN SUBSTRING(@Script, @MetaStartIndex, @MetaEndIndex-@MetaStartIndex);
END;
 










