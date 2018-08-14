CREATE TABLE [dsp].[LogUser] (
    [UserName]  NVARCHAR (100) NOT NULL,
    [IsEnabled] BIT            CONSTRAINT [DF_LogUser_IsEnabled] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK__LogUser__C9F28457D1B75C41] PRIMARY KEY CLUSTERED ([UserName] ASC)
);

