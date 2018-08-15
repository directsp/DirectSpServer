CREATE PROC [dsp].[Util_VerifyServerName]
	@ConfirmServerName TSTRING
AS
BEGIN
	DECLARE @ServerName TSTRING = @@SERVERNAME;
	IF (@ConfirmServerName IS NULL OR	LOWER(@ConfirmServerName) != LOWER(@@SERVERNAME))
		EXEC err.ThrowGeneralException @ProcId = @@PROCID, @Message = N'ConfirmServerName must be set to ''{0}''!',
			@Param0 = @ServerName;
END;