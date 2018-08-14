
CREATE PROC [dbo].[Init_FillStrings]
AS
BEGIN
    SET NOCOUNT ON;

    -- Delclare your application strings here
    INSERT  dsp.StringTable (StringId, StringValue)
    VALUES (N'IsDirectSpInternal', N'1');

END;

