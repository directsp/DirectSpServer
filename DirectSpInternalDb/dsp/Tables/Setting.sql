CREATE TABLE [dsp].[Setting] (
    [SettingId]                    INT           NOT NULL,
    [AppName]                      NVARCHAR (50) CONSTRAINT [DF_Setting_AppName] DEFAULT (N'NewApplicationName') NOT NULL,
    [AppVersion]                   VARCHAR (50)  CONSTRAINT [DF_Setting_AppVersion] DEFAULT ('1.0.0') NOT NULL,
    [IsUnitTestMode]               BIT           CONSTRAINT [DF_Setting_IsUnitTestMode] DEFAULT ((0)) NOT NULL,
    [IsProductionEnvironment]      BIT           CONSTRAINT [DF_Setting_IsProductionEnvironment] DEFAULT ((0)) NOT NULL,
    [PaginationDefaultRecordCount] INT           CONSTRAINT [DF_Setting_PaginationDefaultRecordCount] DEFAULT ((50)) NOT NULL,
    [PaginationMaxRecordCount]     INT           CONSTRAINT [DF_Setting_PaginationMaxRecordCount] DEFAULT ((200)*(1000000)) NOT NULL,
    [AppUserId]                    NVARCHAR (50) NULL,
    [SystemUserId]                 NVARCHAR (50) NULL,
    [MaintenanceMode]              TINYINT       CONSTRAINT [DF_Setting_MaintenanceMode] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Setting] PRIMARY KEY CLUSTERED ([SettingId] ASC),
    CONSTRAINT [CK_Setting_MaintenaceMode] CHECK ([MaintenanceMode]=(2) OR [MaintenanceMode]=(1) OR [MaintenanceMode]=(0))
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Midware uses this UserId if it is set otherwise the midware use SystemUserId', @level0type = N'SCHEMA', @level0name = N'dsp', @level1type = N'TABLE', @level1name = N'Setting', @level2type = N'COLUMN', @level2name = N'AppUserId';

