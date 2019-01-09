CREATE	PROCEDURE [dsp].[Context_$ValidatePagination]
	@RecordCount INT OUT, @RecordIndex INT OUT
AS
BEGIN
	-- Get RecordCount and RecordIndex from Context
	SET @RecordIndex = ISNULL(@RecordIndex, 0);

	DECLARE @PaginationMaxRecordCount INT;
	DECLARE @PaginationDefaultRecordCount INT;
	EXEC dsp.Setting_Props @PaginationDefaultRecordCount = @PaginationDefaultRecordCount OUTPUT,
		@PaginationMaxRecordCount = @PaginationMaxRecordCount OUTPUT;

	-- Set Default
	IF (dsp.Param_IsSetOrNotNull(@RecordCount) = 0)
		SET @RecordCount = @PaginationDefaultRecordCount;

	-- Set Max
	IF (@RecordCount = -2)
		SET @RecordCount = @PaginationMaxRecordCount;

	IF (@RecordCount > @PaginationMaxRecordCount) --
		EXEC err.ThrowPageSizeTooLarge @ProcId = @@PROCID;
END;