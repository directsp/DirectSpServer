﻿
CREATE FUNCTION err.BatchIsNotAllowedId()
RETURNS INT WITH SCHEMABINDING
AS
BEGIN
	RETURN 55023;  
END