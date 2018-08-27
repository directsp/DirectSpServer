
/*
Creating DirectSp required Procedures

dbo.Init_Cleanup
dbo.Init_FillExceptions
dbo.Init_FillStrings
dbo.Init_FillLookups
dbo.Init_FillData
dbo.Context_Update
api.System_Api
api.User_CreateOrUpdateByAuthClaims
dbo.User_UserIdByAuthUserId
*/

CREATE PROC [dsp].[Init_$SetUp]
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Sql TSTRING;
    DECLARE @Schema TSTRING;
    DECLARE @ObjectName TSTRING;

    -------------------------
    -- Schemas
    -------------------------

    -- try to create err schema
    IF SCHEMA_ID('err') IS NULL
    BEGIN
        PRINT 'Creating schema: err ';
        EXEC sys.sp_executesql N'CREATE SCHEMA [err]';
    END;

    -- try to create str schema
    IF SCHEMA_ID('str') IS NULL
    BEGIN
        PRINT 'Creating schema: str';
        EXEC sys.sp_executesql N'CREATE SCHEMA [str]';
    END;

    -- try to create const schema
    IF SCHEMA_ID('const') IS NULL
    BEGIN
        PRINT 'Creating schema: const';
        EXEC sys.sp_executesql N'CREATE SCHEMA [const]';

    END;

    -----------------------------------
    -- Create dbo.Init_Cleanup
    -----------------------------------
    SET @Schema = 'dbo';
    SET @ObjectName = 'Init_Cleanup';
    IF (dsp.Metadata_IsObjectExists(@Schema, @ObjectName, 'P') = 0)
    BEGIN
        EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = 'Creating procedure: {0}.{1}', @Param0 = @Schema, @Param1 = @ObjectName;
        SET @Sql =
            '
CREATE PROC dbo.Init_Cleanup
AS
BEGIN
    SET NOCOUNT ON;

	-- Protect production environment
	EXEC dsp.Util_ProtectProductionEnvironment
			
	-- Delete Junction Tables 

	-- Delete base tables 

	-- Delete Lookup tables (may not required)

END
		';
        EXEC (@Sql);
    END;


    -----------------------------------
    -- Create dbo.Init_FillStrings
    -----------------------------------
    SET @Schema = 'dbo';
    SET @ObjectName = 'Init_FillStrings';
    IF (dsp.Metadata_IsObjectExists(@Schema, @ObjectName, 'P') = 0)
    BEGIN
        EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = 'Creating procedure: {0}.{1}', @Param0 = @Schema, @Param1 = @ObjectName;
        SET @Sql =
            '
CREATE PROC dbo.Init_FillStrings
AS
BEGIN
    SET NOCOUNT ON;

	-- Delclare your application strings here
	INSERT	dsp.StringTable (StringId, StringValue)
	VALUES 
		(N''String1'', N''string 1''),
		(N''String2'', N''string 2'');

END
		';
        EXEC (@Sql);
    END;

    -----------------------------------
    -- Create dbo.Init_FillExceptions
    -----------------------------------
    SET @Schema = 'dbo';
    SET @ObjectName = 'Init_FillExceptions';
    IF (dsp.Metadata_IsObjectExists(@Schema, @ObjectName, 'P') = 0)
    BEGIN
        EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = 'Creating procedure: {0}.{1}', @Param0 = @Schema, @Param1 = @ObjectName;
        SET @Sql =
            '
CREATE PROC dbo.Init_FillExceptions
AS
BEGIN
    SET NOCOUNT ON;

	-- delclare your application exceptions here. NOTE: ExceptionId must be started from 56000
	INSERT	dsp.Exception (ExceptionId, ExceptionName, Description)
	VALUES (56001, N''Error1'', N''Error1 description''),
		(56002, N''Error2'', N''Error2 description'');

END
		';
        EXEC (@Sql);
    END;

    -----------------------------------
    -- Create dbo.Init_FillLookups
    -----------------------------------
    SET @Schema = 'dbo';
    SET @ObjectName = 'Init_FillLookups';
    IF (dsp.Metadata_IsObjectExists(@Schema, @ObjectName, 'P') = 0)
    BEGIN
        EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = 'Creating procedure: {0}.{1}', @Param0 = @Schema, @Param1 = @ObjectName;
        SET @Sql =
            '
CREATE PROC dbo.Init_FillLookups
AS
BEGIN
    SET NOCOUNT ON;
	DECLARE @TableName TSTRING;

	-- Init lookup tables in this format

	-- LookupTable
	-- SET @TableName = N''LookupTable'';
	-- SELECT * INTO #LookupTable FROM dbo.LookupTable WHERE 1 = 0;
	-- INSERT INTO #LookupTable (LookupKey, LookupName)
	-- VALUES 
	--	(const.Enum_Name1(), N''Male''),
	--	(const.Enum_Name2(), N''Female'');

	-- EXEC dsp.Table_CompareData @DestinationTableName = @TableName;

END
		';
        EXEC (@Sql);
    END;

    -----------------------------------
    -- Create dbo.Init_FillData
    -----------------------------------
    SET @Schema = 'dbo';
    SET @ObjectName = 'Init_FillData';
    IF (dsp.Metadata_IsObjectExists(@Schema, @ObjectName, 'P') = 0)
    BEGIN
        EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = 'Creating procedure: {0}.{1}', @Param0 = @Schema, @Param1 = @ObjectName;
        SET @Sql =
            '
CREATE PROC dbo.Init_FillData
AS
BEGIN
    SET NOCOUNT ON;

	-- TODO: Initialize the app 
	EXEC dsp.Setting_PropsSet @AppName = ''MyAppName'', @AppVersion = ''1.0.*'';

	-- TODO: A proper SystemUserId must be set here
	EXEC dsp.Setting_PropsSet @SystemUserId = 1; -- 

	-- Initialized startup data if they are not initialized yet
END
'       ;
        EXEC (@Sql);
    END;

    -----------------------------------
    -- Create dbo.Context_Update
    -----------------------------------
    SET @Schema = 'dbo';
    SET @ObjectName = 'Context_Update';
    IF (dsp.Metadata_IsObjectExists(@Schema, @ObjectName, 'P') = 0)
    BEGIN
        EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = 'Creating procedure: {0}.{1}', @Param0 = @Schema, @Param1 = @ObjectName;
        SET @Sql =
            '
-- This procedure will be called for every api
CREATE PROCEDURE dbo.Context_Update
	@Context TCONTEXT OUTPUT, @ProcId INT
AS
BEGIN
	SET NOCOUNT ON;

	-- Update Context here
	
END;
'       ;
        EXEC (@Sql);
    END;

    -----------------------------------
    -- Create api.System_Api
    -----------------------------------
    SET @Schema = 'api';
    SET @ObjectName = 'System_Api';
    IF (dsp.Metadata_IsObjectExists(@Schema, @ObjectName, 'P') = 0)
    BEGIN
        EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = 'Creating procedure: {0}.{1}', @Param0 = @Schema, @Param1 = @ObjectName;
        SET @Sql =
            '
/*
#MetaStart
{
	"DataAccessMode": "ReadSnapshot"
} 
#MetaEnd
*/
CREATE PROCEDURE [api].[System_Api]
	@Context TCONTEXT OUTPUT, @Api TJSON = NULL OUT
WITH EXECUTE AS OWNER
AS
BEGIN
	SET NOCOUNT ON;
	EXEC dsp.Context_Verify @Context = @Context OUT, @ProcId = @@PROCID;

	-- Any user should have access to this procedure

	-- Call dsp
	EXEC dsp.System_Api @Api = @Api OUTPUT;
END;
'       ;
        EXEC (@Sql);
    END;

    -----------------------------------
    -- Create dbo.User_UserIdByAuthUserId
    -----------------------------------
    SET @Schema = 'dbo';
    SET @ObjectName = 'User_UserIdByAuthUserId';
    IF (dsp.Metadata_IsObjectExists(@Schema, @ObjectName, 'P') = 0)
    BEGIN
        EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = 'Creating procedure: {0}.{1}', @Param0 = @Schema, @Param1 = @ObjectName;
        SET @Sql =
            '
CREATE PROCEDURE dbo.User_UserIdByAuthUserId
    @AuthUserId INT, @UserId TSTRING OUT
AS
BEGIN
    SET @UserId = NULL;

    -- find the UserId from your own user table
    -- SELECT  @UserId = U.UserId FROM  dbo.Users AS U WHERE  U.AuthUserId = @AuthUserId;

    IF (@UserId IS NULL) --
        EXEC err.ThrowAuthUserNotFound @ProcId = @@PROCID, @Message = ''AuthUserId: {0}'', @Param0 = @AuthUserId;
END;
'       ;
        EXEC (@Sql);
    END;

    ------------------
    -- Create api.User_CreateOrUpdateByAuthClaims
    ------------------
    SET @Schema = 'api';
    SET @ObjectName = 'User_CreateOrUpdateByAuthClaims';
    IF (dsp.Metadata_IsObjectExists(@Schema, @ObjectName, 'P') = 0)
    BEGIN
        EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = 'Creating procedure: {0}.{1}', @Param0 = @Schema, @Param1 = @ObjectName;
        SET @Sql =
            '
/*
#MetaStart
{
	"DataAccessMode": "Write"
} 
#MetaEnd
*/
CREATE PROCEDURE api.User_CreateOrUpdateByAuthClaims
    @Context TCONTEXT OUT, @AuthUserClaims TJSON
WITH EXECUTE AS OWNER
AS
BEGIN
    EXEC dsp.Context_Verify @Context = @Context OUTPUT, @ProcId = @@PROCID;
    EXEC err.ThrowNotSupported @ProcId = @@PROCID;
END
'       ;
        EXEC (@Sql);
    END;

    ------------------
    -- Create dbo.Settings
    ------------------
    SET @Schema = 'dbo';
    SET @ObjectName = 'Setting';
    IF (dsp.Metadata_IsObjectExists(@Schema, @ObjectName, 'U') = 0)
    BEGIN
        EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = 'Creating Table: {0}.{1}', @Param0 = @Schema, @Param1 = @ObjectName;

        CREATE TABLE dbo.Setting (SettingId INT NOT NULL,
            AppSetting1 INT NOT NULL,
            AppSetting2 INT NOT NULL,
            CONSTRAINT PK_Setting PRIMARY KEY CLUSTERED (SettingId ASC) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF,
                                      ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY];

        ALTER TABLE dbo.Setting ADD CONSTRAINT DF_Setting_SettingId DEFAULT ((1)) FOR SettingId;
        ALTER TABLE dbo.Setting ADD CONSTRAINT DF_Setting_AppSetting1 DEFAULT ((0)) FOR AppSetting1;
        ALTER TABLE dbo.Setting ADD CONSTRAINT DF_Setting_AppSetting2 DEFAULT ((0)) FOR AppSetting2;
    END;

    ------------------
    -- Create dbo.Setting_Props
    ------------------
    SET @Schema = 'dbo';
    SET @ObjectName = 'Setting_Props';
    IF (dsp.Metadata_IsObjectExists(@Schema, @ObjectName, 'P') = 0)
    BEGIN
        EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = 'Creating procedure: {0}.{1}', @Param0 = @Schema, @Param1 = @ObjectName;
        SET @Sql =
            '
-- Retrieve Application settings
CREATE PROCEDURE dbo.Setting_Props
    @AppSetting1 INT = -1 OUT, @AppSetting2 INT = -1 OUT
AS
BEGIN
	SET NOCOUNT ON;

    SELECT  @AppSetting1 = AppSetting1, @AppSetting2 = AppSetting2
      FROM  dbo.Setting;

END;
'       ;
        EXEC (@Sql);
    END;

    ------------------
    -- Create dbo.Setting_PropsSet
    ------------------
    SET @Schema = 'dbo';
    SET @ObjectName = 'Setting_PropsSet';
    IF (dsp.Metadata_IsObjectExists(@Schema, @ObjectName, 'P') = 0)
    BEGIN
        EXEC dsp.Log_Trace @ProcId = @@PROCID, @Message = 'Creating procedure: {0}.{1}', @Param0 = @Schema, @Param1 = @ObjectName;
        SET @Sql =
            '
-- Retrieve Application settings
CREATE PROCEDURE dbo.Setting_PropsSet
    @AppSetting1 INT = -1, @AppSetting2 INT = -1
AS
BEGIN
    SET NOCOUNT ON;

    IF (dsp.Param_IsSet(@AppSetting1) = 1)
        UPDATE  dbo.Setting
           SET  AppSetting1 = @AppSetting1;

    IF (dsp.Param_IsSet(@AppSetting2) = 2)
        UPDATE  dbo.Setting
           SET  AppSetting2 = @AppSetting2;
END;
'       ;
        EXEC (@Sql);
    END;



END;

















