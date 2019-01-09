CREATE PROCEDURE [dsp].[Context_Props]
    @Context TCONTEXT OUT, @AppName TSTRING = N'<notset>' OUT, @AuthUserId TSTRING = N'<notset>' OUT, @UserId TSTRING = N'<notset>' OUT,
    @Audience TSTRING = N'<notset>' OUT, @IsCaptcha INT = -1 OUT, @RecordCount INT = -1 OUT, @RecordIndex INT = -1 OUT,
    @ClientVersion TSTRING = N'<notset>' OUT, @MoneyConversionRate FLOAT = -1 OUT, @InvokerAppVersion TSTRING = NULL OUT, @IsReadonlyIntent BIT = NULL OUT, @IsInvokedByMidware BIT = NULL OUT
AS
BEGIN
    -- General
    IF (@AppName IS NULL OR @AppName <> N'<notset>')
        SET @AppName = JSON_VALUE(@Context, N'$.AppName');

    IF (@AuthUserId IS NULL OR  @AuthUserId <> N'<notset>')
        SET @AuthUserId = JSON_VALUE(@Context, N'$.AuthUserId');

    IF (@UserId IS NULL OR  @UserId <> N'<notset>')
        SET @UserId = JSON_VALUE(@Context, N'$.UserId');

    IF (@Audience IS NULL OR @Audience <> N'<notset>')
        SET @Audience = JSON_VALUE(@Context, N'$.Audience');

    -- InvokeOptions
    DECLARE @InvokeOptions TJSON = JSON_QUERY(@Context, N'$.InvokeOptions');

    IF (@ClientVersion IS NULL OR   @ClientVersion <> N'<notset>')
        SET @ClientVersion = JSON_VALUE(@InvokeOptions, N'$.ClientVersion');

    IF (@MoneyConversionRate IS NULL OR @MoneyConversionRate <> -1)
        SET @MoneyConversionRate = ISNULL(JSON_VALUE(@InvokeOptions, N'$.MoneyConversionRate'), 1);

    IF (@IsCaptcha IS NULL OR   @IsCaptcha <> -1)
        SET @IsCaptcha = ISNULL(CAST(JSON_VALUE(@InvokeOptions, '$.IsCaptcha') AS BIT), 0);

    IF (@IsReadonlyIntent IS NULL OR   @IsReadonlyIntent <> -1)
        SET @IsReadonlyIntent = ISNULL(CAST(JSON_VALUE(@InvokeOptions, '$.IsReadonlyIntent') AS BIT), 0);

    IF ((@RecordCount IS NULL OR @RecordCount <> -1) OR (@RecordIndex IS NULL OR @RecordIndex <> -1))
    BEGIN
        SET @RecordCount = JSON_VALUE(@InvokeOptions, N'$.RecordCount');
        SET @RecordIndex = JSON_VALUE(@InvokeOptions, N'$.RecordIndex');
        EXEC dsp.[Context_$ValidatePagination] @RecordCount = @RecordCount OUTPUT, @RecordIndex = @RecordIndex OUTPUT;
    END;

    IF (@InvokerAppVersion IS NULL OR   @InvokerAppVersion <> -1)
        SET @InvokerAppVersion = JSON_VALUE(@InvokeOptions, N'$.InvokerAppVersion');

	SET @IsInvokedByMidware = IIF (@InvokerAppVersion IS NOT NULL, 1, 0);

END;

















