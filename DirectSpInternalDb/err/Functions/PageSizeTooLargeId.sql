﻿
CREATE FUNCTION err.PageSizeTooLargeId()
RETURNS INT WITH SCHEMABINDING
AS
BEGIN
	RETURN 55009;  
END