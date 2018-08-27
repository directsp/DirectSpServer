
CREATE PROC [dbo].[Init_FillLookups]
AS
BEGIN
    SET NOCOUNT ON;
	DECLARE @TableName TSTRING;

	-- Init lookup tables in this format

	-- BatchInfoResultType
	SET @TableName = N'BatchItemResultType';
	SELECT * INTO #BatchItemResultType FROM dbo.BatchItemResultType WHERE 1 = 0;
	INSERT	#BatchItemResultType (BatchItemResultTypeId, BatchItemResultTypeName)
	VALUES (const.BatchItemResultType_NotProccessed(), 'NotExecuted'),
		(const.BatchItemResultType_Success(), 'Success'),
		(const.BatchItemResultType_Warning(), 'Warning'),
		(const.BatchItemResultType_Error(), 'Error');

	EXEC dsp.Table_CompareData @DestinationTableName = @TableName;

END