CREATE PROCEDURE [dsp].[Setting_Props]
    @AppName TSTRING = NULL OUT, @AppVersion TSTRING = NULL OUT, @SystemUserId TSTRING = NULL OUT, @AppUserId TSTRING = NULL OUT,
    @IsProductionEnvironment BIT = NULL OUT, @PaginationDefaultRecordCount INT = NULL OUT, @PaginationMaxRecordCount INT = NULL OUT,
    @IsUnitTestMode BIT = NULL OUT, @MaintenanceMode INT = NULL OUT
AS
BEGIN

    SELECT --
        @AppName = AppName, --
        @AppVersion = AppVersion, -- 
        @SystemUserId = SystemUserId, -- 
        @AppUserId = AppUserId, -- 
        @AppVersion = AppVersion, -- 
        @IsProductionEnvironment = IsProductionEnvironment, --
        @PaginationDefaultRecordCount = PaginationDefaultRecordCount, --
        @PaginationMaxRecordCount = PaginationMaxRecordCount, --
        @IsUnitTestMode = IsUnitTestMode, --
        @MaintenanceMode = MaintenanceMode --
      FROM  dsp.Setting;

END;