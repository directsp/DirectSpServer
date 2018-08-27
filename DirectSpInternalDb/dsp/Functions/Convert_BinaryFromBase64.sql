
-- #Inliner {"InlineMode":"none"} 
CREATE FUNCTION [dsp].[Convert_BinaryFromBase64](@Base64 TSTRING)
RETURNS VARBINARY(MAX)
AS
BEGIN
    DECLARE @Bin VARBINARY(MAX)
    /*
        SELECT CONVERT(TSTRING, dbo.f_Base64ToBinary('Q29udmVydGluZyB0aGlzIHRleHQgdG8gQmFzZTY0Li4u'))
    */
    SET @Bin = CAST(N'' AS XML).value('xs:base64Binary(sql:variable("@Base64"))', 'VARBINARY(MAX)')
    RETURN @Bin
END