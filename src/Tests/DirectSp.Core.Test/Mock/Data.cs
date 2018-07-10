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
            return @"eyJhbGciOiAiSFMyNTYiLCAidHlwIjogIkpXVCJ9.ew0KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgInVzZXJuYW1lIjoiYmVobmFtIiwNCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICJyb2xlIjoiYWRtaW4iLA0KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIkNlcnRpZmljYXRlVGh1bWIiOiJBNTAyNTdENzUzMzI4MDAwRThFOThBMTZCREM4RkI5MkQxN0RGQ0RFIg0KICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICB9.vvKBxjqnkl/G7DK3TW2UjsM4Mp562xB6+F2dowhlvlfjkwylOIDZsy5Foc6jCq6Yo9uTFMZ2e3rCzohRznPev1pnHjNVWdC75nOJe63LsI/DugILGHYPdGEaGMdAzdIaQhaRfmXD2d0TC/y9R9VkRZny4hOtsd7e3mwn430QxeTx55IQs6OuufgOs1yYsEJK0dHSBIYPVzPv2gKPpBth/6Rlar8UDcchfyMx6qqPvJglOgjsBx5xAfJlA/UfS9n3kjy+kG0dSJq2/lenjJV/7RMryH+tdiR4vKM7yvMovqN3CSjB7IL4fxQHxhBQYHIHdXpdXpIAWKODvWW343NcFg==";
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
                                    ""CertificateThumb"":""A50257D753328000E8E98A16BDC8FB92D17DFCDE""
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
