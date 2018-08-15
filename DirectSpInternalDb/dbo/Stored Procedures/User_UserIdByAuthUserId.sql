
CREATE PROCEDURE [dbo].[User_UserIdByAuthUserId]
    @AuthUserId INT, @UserId TSTRING OUT
AS
BEGIN
    SET @UserId = NULL;

    -- find the UserId from your own user table
    -- SELECT @UserId = U.UserId FROM Users WHERE U.AuthUserId = @AuthUserId;

    IF (@UserId IS NULL) --
        EXEC err.ThrowAuthUserNotFound @ProcId = @@PROCID;
END;