CREATE TABLE [dbo].[Setting] (
    [SettingId]       INT CONSTRAINT [DF_Setting_SettingId] DEFAULT ((1)) NOT NULL,
    [CaptchaLifetime] INT CONSTRAINT [DF_Setting_CaptchaLifetime] DEFAULT ((10)*(60)) NOT NULL,
    CONSTRAINT [PK_Setting] PRIMARY KEY CLUSTERED ([SettingId] ASC)
);

