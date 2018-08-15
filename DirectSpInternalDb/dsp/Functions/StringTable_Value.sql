
CREATE FUNCTION [dsp].[StringTable_Value] (@StringId TSTRING)
RETURNS TSTRING
AS
BEGIN
    DECLARE @Value TSTRING;

    SELECT  @Value = ST.StringValue
      FROM  dsp.StringTable AS ST
     WHERE  ST.StringId = @StringId;

    RETURN dsp.String_ReplaceEnter(@Value);
END;