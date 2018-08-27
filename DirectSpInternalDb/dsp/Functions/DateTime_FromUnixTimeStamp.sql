CREATE FUNCTION [dsp].[DateTime_FromUnixTimeStamp] (
@UnixTime BIGINT
)
RETURNS datetime
AS
BEGIN

  RETURN DATEADD(S,@UnixTime,'1970-01-01')

END
