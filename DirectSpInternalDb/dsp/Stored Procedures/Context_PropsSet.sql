CREATE PROC [dsp].[Context_PropsSet]
    @Context TCONTEXT OUT, @AppName TSTRING = N'<notset>', @AppVersion TSTRING = N'<notset>', @AuthUserId TSTRING = N'<notset>', @UserId TSTRING = N'<notset>',
    @Audience TSTRING = N'<notset>', @IsCaptcha BIT = NULL, @RecordCount INT = -1, @RecordIndex INT = -1, @ClientVersion TSTRING = N'<notset>'
AS
BEGIN
    IF (@Context IS NULL OR @Context = '')
        SET @Context = '{}';

    -- Fix built-in users
    IF (@UserId = N'$' OR   @UserId = N'$$')
    BEGIN
        DECLARE @SystemUserId TSTRING;
        DECLARE @AppUserId TSTRING;
        EXEC dsp.Setting_Props @SystemUserId = @SystemUserId OUT, @AppUserId = @AppUserId OUT;

        IF (@UserId = N'$')
            SET @UserId = @SystemUserId;

        IF (@UserId = N'$$')
            SET @UserId = ISNULL(@AppUserId, @SystemUserId);
    END;

    IF (@AppName IS NULL OR @AppName <> N'<notset>')
        SET @Context = JSON_MODIFY(@Context, '$.AppName', @AppName);

    IF (@AppVersion IS NULL OR  @AppVersion <> N'<notset>')
        SET @Context = JSON_MODIFY(@Context, '$.AppVersion', @AppVersion);

    IF (@AuthUserId IS NULL OR  @AuthUserId <> N'<notset>')
        SET @Context = JSON_MODIFY(@Context, N'$.AuthUserId', @AuthUserId);

    IF (@UserId IS NULL OR  @UserId <> N'<notset>')
        SET @Context = JSON_MODIFY(@Context, N'$.UserId', @UserId);

    IF (@Audience IS NULL OR @Audience <> N'<notset>')
        SET @Context = JSON_MODIFY(@Context, N'$.Audience', @Audience);

    -- update InvokeOptions
    IF (JSON_QUERY(@Context, N'$.InvokeOptions') IS NULL)
        SET @Context = JSON_MODIFY(@Context, N'$.InvokeOptions', JSON_QUERY('{}', '$'));

    IF (@IsCaptcha IS NOT NULL)
        SET @Context = JSON_MODIFY(@Context, N'$.InvokeOptions.IsCaptcha', @IsCaptcha);

    IF (@RecordCount IS NULL OR @RecordCount <> -1)
        SET @Context = JSON_MODIFY(@Context, N'$.InvokeOptions.RecordCount', @RecordCount);

    IF (@RecordIndex IS NULL OR @RecordIndex <> -1)
        SET @Context = JSON_MODIFY(@Context, N'$.InvokeOptions.RecordIndex', @RecordIndex);

    IF (@ClientVersion IS NULL OR   @ClientVersion <> N'<notset>')
        SET @Context = JSON_MODIFY(@Context, N'$.InvokeOptions.ClientVersion', @ClientVersion);
END;








