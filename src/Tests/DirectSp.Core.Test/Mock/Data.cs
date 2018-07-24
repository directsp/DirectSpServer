using DirectSp.Core.SpSchema;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;

namespace DirectSp.Core.Test.Mock
{
    static class Data
    {
        internal static string SystemApi()
        {
            return JsonConvert.SerializeObject(new List<SpInfo>
            {
                new SpInfo
                {
                 SchemaName="api",
                 Params=new SpParam[]
                 {
                     new SpParam
                     {
                         ParamName="Test",
                         IsOutput=true,
                         Length=0,
                          SystemTypeName=SqlDbType.NVarChar,
                          UserTypeName="TSTRING"
                     }
                 },
                 ProcedureName="TestApi"
                },
                new SpInfo
                {
                 SchemaName="api",
                 Params=new SpParam[]
                 {
                     new SpParam
                     {
                         ParamName="@JwtToken",
                         IsOutput=true,
                         Length=0,
                          SystemTypeName=SqlDbType.NVarChar,
                          UserTypeName="TSTRING"
                     }
                 },
                 ProcedureName="SignJwtToken",
                 ExtendedProps=new SpInfoEx{
                     CaptchaMode=SpCaptchaMode.Manual,
                     ExecuteMode=SpExecuteMode.NotSet,
                     Params=new Dictionary<string, SpParamEx>
                     {
                         {"@JwtToken",new SpParamEx{ SignType=SpSignMode.JwtByCertThumb} }
                     }
                 }
                },
                new SpInfo
                {
                 SchemaName="api",
                 Params=new SpParam[]
                 {
                     new SpParam
                     {
                         ParamName="@JwtToken",
                         IsOutput=false,
                         Length=0,
                          SystemTypeName=SqlDbType.NVarChar,
                          UserTypeName="TSTRING"
                     }
                 },
                 ProcedureName="SignJwtTokenChecking",
                 ExtendedProps=new SpInfoEx{
                     CaptchaMode=SpCaptchaMode.Manual,
                     ExecuteMode=SpExecuteMode.NotSet,
                     Params=new Dictionary<string, SpParamEx>
                     {
                         {"@JwtToken",new SpParamEx{ SignType=SpSignMode.JwtByCertThumb} }
                     }
                 }
                },
                new SpInfo
                {
                 SchemaName="api",
                 Params=new SpParam[]
                 {
                     new SpParam
                     {
                         ParamName="@Param1",
                         IsOutput=true,
                         Length=0,
                          SystemTypeName=SqlDbType.NVarChar,
                          UserTypeName="TSTRING"
                     }
                 },
                 ProcedureName="ParallelSp",
                 ExtendedProps=new SpInfoEx
                 {
                     IsBatchAllowed=true
                 }
                }

            });
        }

        internal static string SignedJwtToken()
        {
            return @"eyJhbGciOiAiSFMyNTYiLCAidHlwIjogIkpXVCJ9.ew0KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgInVzZXJuYW1lIjoiYmVobmFtIiwNCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICJyb2xlIjoiYWRtaW4iLA0KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgImNlcnRpZmljYXRlVGh1bWIiOiJBNTAyNTdENzUzMzI4MDAwRThFOThBMTZCREM4RkI5MkQxN0RGQ0RFIg0KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9.GxUF0RaDfl7VFE7sF42ATMPsrLO87N7c9uklwtWAuIZu1+YrpkDi5BEol2XTmdh8PtWPBxVJBjAByIc8GLACmvbzq1glwFZVjzs0kFAGgRvMqYRwiyiN7dA3R4XAQPqe9FXQrLTV0LjO4YCRLXJIlc1e04S72V0TEw12c0FkiSexoP1Y0T9BmK6YKkkbQNrXZJWAAaUc6VhO5Q0vf7+VrFs+a1FiuHtIrV78oAtieW78ucU/cqbWqI5/BhejmvSDRohEH5K30/L+eSi6lb7bFgSrAkBymN6Vdl+XAIjxw6dBRSyXnClbmvlgngsC0+ukV57ukV8Sl7ACnCxaKYW/pA==";
        }

        internal static string AppContext()
        {
            return "{'AppName':'IcLoyalty','AppVersion':'2.0.14','UserId':'21','InvokeOptions':{'IsCaptcha':true}}".Replace("'", "\"");
        }

        internal static string JwtToken()
        {
            return @"{
                                    ""username"":""behnam"",
                                    ""role"":""admin"",
                                    ""certificateThumb"":""A50257D753328000E8E98A16BDC8FB92D17DFCDE""
                                }";
        }

        internal static IDataReader DataReaderForTestSp()
        {
            var mockDataReader = new Mock<IDataReader>();
            bool readToggle = true;

            mockDataReader.Setup(x => x.Read()).Returns(() => readToggle)
                .Callback(() => readToggle = false);

            mockDataReader.Setup(dr => dr["TestColumn"])
                .Returns("Nothing else matter");

            return mockDataReader.Object;
        }

        internal static IDataReader EmptyDataReader()
        {
            var mockDataReader = new Mock<IDataReader>();

            mockDataReader.Setup(x => x.Read()).Returns(() => false);
            return mockDataReader.Object;
        }

    }
}
