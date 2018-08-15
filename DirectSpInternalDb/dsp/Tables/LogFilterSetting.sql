CREATE TABLE [dsp].[LogFilterSetting] (
    [LogFilterSettingId] INT             IDENTITY (1, 1) NOT NULL,
    [UserName]           NVARCHAR (100)  NOT NULL,
    [IsExludedFilter]    BIT             CONSTRAINT [DF_LogFilter_IsExludedFilter] DEFAULT ((0)) NOT NULL,
    [Log_Filter]         NVARCHAR (2000) NULL,
    CONSTRAINT [PK__LogFilte__7B08B2A3AD695576] PRIMARY KEY CLUSTERED ([LogFilterSettingId] ASC),
    CONSTRAINT [FK__LogFilter__UserN__1D13DFA0] FOREIGN KEY ([UserName]) REFERENCES [dsp].[LogUser] ([UserName])
);

