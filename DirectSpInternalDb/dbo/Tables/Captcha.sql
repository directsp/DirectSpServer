CREATE TABLE [dbo].[Captcha] (
    [CaptchaId]   UNIQUEIDENTIFIER CONSTRAINT [DF_Captcha_CaptchaId] DEFAULT (newsequentialid()) NOT NULL,
    [Code]        NVARCHAR (10)    NOT NULL,
    [CreatedTime] DATETIME         CONSTRAINT [DF_Captcha_ExpirationTime] DEFAULT (getdate()) NOT NULL,
    [IsUsed]      BIT              CONSTRAINT [DF_Captcha_IsUsed] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Captcha] PRIMARY KEY CLUSTERED ([CaptchaId] ASC)
);

