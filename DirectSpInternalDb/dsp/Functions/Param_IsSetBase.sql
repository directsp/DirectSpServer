CREATE	FUNCTION [dsp].[Param_IsSetBase] (@Value SQL_VARIANT,
	@NullAsNotSet BIT)
RETURNS BIT WITH SCHEMABINDING
AS
BEGIN
	IF (@Value IS NULL AND @NullAsNotSet = 1)
		RETURN 0;

	DECLARE @Type NVARCHAR(/*NoCodeChecker*/ 20) = CAST(SQL_VARIANT_PROPERTY(@Value, 'BaseType') AS NVARCHAR(/*NoCodeChecker*/ 20));

	IF (@Type LIKE '%int%' AND CAST(@Value AS INT) = -1)
		RETURN 0;

	IF (@Type LIKE '%char%' AND (CAST(@Value AS NVARCHAR(/*NoCodeChecker*/ 10)) = '<notset>' OR CAST(@Value AS NVARCHAR(/*NoCodeChecker*/ 10)) = '<noaccess>'))
		RETURN 0;

	IF (@Type LIKE '%date%' AND CAST(@Value AS DATETIME) = '1753-01-01')
		RETURN 0;

	IF (@Type LIKE '%decimal%' AND CAST(@Value AS DECIMAL) = -1)
		RETURN 0;

	IF (@Type LIKE '%money%' AND CAST(@Value AS MONEY) = -1)
		RETURN 0;

	IF (@Type LIKE '%float%' AND CAST(@Value AS FLOAT) = -1)
		RETURN 0;

	RETURN 1;
END;