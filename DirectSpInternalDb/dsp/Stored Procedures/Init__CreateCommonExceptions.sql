
-- Create common exception
CREATE PROC [dsp].[Init_$CreateCommonExceptions]
AS
BEGIN

	INSERT dsp.Exception (ExceptionId, ExceptionName) VALUES (55001, N'GeneralException');
	INSERT dsp.Exception (ExceptionId, ExceptionName) VALUES (55002, N'AccessDeniedOrObjectNotExists');
	INSERT dsp.Exception (ExceptionId, ExceptionName) VALUES (55004, N'ObjectAlreadyExists');
	INSERT dsp.Exception (ExceptionId, ExceptionName) VALUES (55005, N'ObjectIsInUse');
	INSERT dsp.Exception (ExceptionId, ExceptionName) VALUES (55009, N'PageSizeTooLarge');
	INSERT dsp.Exception (ExceptionId, ExceptionName) VALUES (55011, N'InvalidArgument');
	INSERT dsp.Exception (ExceptionId, ExceptionName) VALUES (55012, N'FatalError');
	INSERT dsp.Exception (ExceptionId, ExceptionName) VALUES (55013, N'LockFailed');
	INSERT dsp.Exception (ExceptionId, ExceptionName) VALUES (55015, N'ValidationError');
	INSERT dsp.Exception (ExceptionId, ExceptionName) VALUES (55016, N'InvalidOperation');
	INSERT dsp.Exception (ExceptionId, ExceptionName) VALUES (55017, N'NotSupported');
	INSERT dsp.Exception (ExceptionId, ExceptionName) VALUES (55018, N'NotImplemeted');
	INSERT dsp.Exception (ExceptionId, ExceptionName) VALUES (55019, N'UserIsDisabled');
	INSERT dsp.Exception (ExceptionId, ExceptionName) VALUES (55020, N'AmbiguousException');
	INSERT dsp.Exception (ExceptionId, ExceptionName) VALUES (55021, N'NoOperation');
	INSERT dsp.Exception (ExceptionId, ExceptionName) VALUES (55022, N'InvalidCaptcha');
	INSERT dsp.Exception (ExceptionId, ExceptionName) VALUES (55023, N'BatchIsNotAllowed');
	INSERT dsp.Exception (ExceptionId, ExceptionName) VALUES (55024, N'TooManyRequest');
	INSERT dsp.Exception (ExceptionId, ExceptionName) VALUES (55025, N'AuthUserNotFound');
	INSERT dsp.Exception (ExceptionId, ExceptionName) VALUES (55026, N'InvokerAppVersion');
	INSERT dsp.Exception (ExceptionId, ExceptionName) VALUES (55027, N'Maintenance');
	INSERT dsp.Exception (ExceptionId, ExceptionName) VALUES (55028, N'MaintenanceReadOnly');
	INSERT dsp.Exception (ExceptionId, ExceptionName) VALUES (55029, N'InvalidParamSignature');

END












