CREATE FUNCTION [dsp].[Setting_SystemUserId] ()
RETURNS INT
AS
BEGIN
    RETURN (SELECT  S.SystemUserId FROM dsp.Setting AS S);
END;