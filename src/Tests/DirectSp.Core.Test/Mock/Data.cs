using DirectSp.Core.SpSchema;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;

namespace DirectSp.Core.Test.Mock
{
    internal static class Data
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
                     DataAccessMode=SpDataAccessMode.Write,
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
                     DataAccessMode=SpDataAccessMode.Write,
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
                },
                new SpInfo
                {
                 SchemaName="api",
                 Params=new SpParam[]
                 {
                     new SpParam
                     {
                         ParamName="@KeyName",
                         Length=0,
                          SystemTypeName=SqlDbType.NVarChar,
                          UserTypeName="TSTRING"
                     },
                     new SpParam
                     {
                         ParamName="@TextValue",
                         Length=0,
                          SystemTypeName=SqlDbType.NVarChar,
                          UserTypeName="TSTRING"
                     },
                     new SpParam
                     {
                         ParamName="@TimeToLife",
                         Length=0,
                          SystemTypeName=SqlDbType.NVarChar,
                          UserTypeName="INT"
                     },
                     new SpParam
                     {
                         ParamName="@IsOverwrite",
                         Length=0,
                          SystemTypeName=SqlDbType.NVarChar,
                          UserTypeName="BIT"
                     }

                 },
                 ProcedureName="KeyValue_ValueSet"
                }

            });
        }

        internal static string SignedJwtToken()
        {
            return @"eyJhbGciOiAiU0hBMjU2IiwgInR5cCI6ICJKV1QifQ==.eydPcmRlck51bWJlcic6MiwnUmVjZWl2ZXJMb3lhbHR5QWNjb3VudElkJzoxMTAzNzkzMSwnQ2x1Yk5hbWUnOidOaWtlKNio2KfYtNqv2KfZhyknLCdBbW91bnQnOjEsJ1BvaW50VHlwZUlkJzoyMDIwLCdQb2ludFR5cGVOYW1lJzon2YbZgtiv24wnLCdQYXllZUxveWFsdHlBY2NvdW50SWQnOjExODMsJ1BheWVlTG95YWx0eUFjY291bnROYW1lJzon2KjZh9mG2KfZhSDYuduM2YjYtiDZvtmI2LEnLCdleHAnOjE1MzQ4Mjg4MjEuMTY2MTM3OSwnUmV0dXJuVXJsJzonaHR0cDovL3d3dy5nb29nbGUuY29tJywnQ2VydGlmaWNhdGVUaHVtYic6Jzc3IDVhIDkyIDkyIGYxIDlmIDE0IGZjIDJiIDliIDgwIGFiIDA2IDJhIDA2IGJlIDg2IDIzIDYxIDljJ30=.ij7o7uTSQrxNDAKZNc/7ZASNM/EqE/7V+h2iokDhQ1eKgM+bgwSMstAtHEio+QpT9H5aoWvFGdXexdruwHnNHIqFYfRziH7f2CgmYDuBdN3OenzCaQ+YAxr90iebcM+ZUiSOYJDmUDpY3ZUE3A/i9LNMy1Ua4pSLiOMqVWLvPZLcq8Ffe9rjFbegfX/hIF83QKJ6CTWgh9AHSUQrgG2RDXXr1P/1LmEHXsj4WGJOXXRbAXx70p2ebwEMsWHqr1we+/uYr/qtbkcHC7NYpQ/niXqkngXE/giTf4DLhDfKufU/LJDzj9fqT21vQTNeuKglOy7lVRkYRPAYVKfFhkC4WA==";
        }

        internal static string AppContext()
        {
            return "{'AppName':'IcLoyalty','AppVersion':'2.0.14','UserId':'21','InvokeOptions':{'IsCaptcha':true}}".Replace("'", "\"");
        }

        internal static string JwtToken()
        {
            return @"{'OrderNumber':2,'ReceiverLoyaltyAccountId':11037931,'ClubName':'Nike(باشگاه)','Amount':1,'PointTypeId':2020,'PointTypeName':'نقدی','PayeeLoyaltyAccountId':1183,'PayeeLoyaltyAccountName':'بهنام عیوض پور','exp':1534828821.1661379,'ReturnUrl':'http://www.google.com','CertificateThumb':'77 5a 92 92 f1 9f 14 fc 2b 9b 80 ab 06 2a 06 be 86 23 61 9c'}";
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

        internal static IDataReader DataReader_KeyValue_All()
        {
            var mockDataReader = new Mock<IDataReader>();
            bool readToggle = true;

            mockDataReader.Setup(x => x.Read()).Returns(() => readToggle)
                .Callback(() => readToggle = false);

            mockDataReader.Setup(dr => dr["KeyName"])
                .Returns("43046f12-5a0a-446d-9f79-3199e1768f55");

            mockDataReader.Setup(dr => dr["TextValue"])
                .Returns("Value01");

            mockDataReader.Setup(dr => dr["ModifiedDate"])
                .Returns(DateTime.Now);

            mockDataReader.Setup(x => x.Read()).Returns(() => readToggle)
    .Callback(() => readToggle = false);

            mockDataReader.Setup(dr => dr["KeyName"])
    .Returns("e93cbf30-9303-4b11-bd97-fe5fbd4a00e5");

            mockDataReader.Setup(dr => dr["TextValue"])
                .Returns("Value02");

            mockDataReader.Setup(dr => dr["ModifiedDate"])
                .Returns(DateTime.Now);

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
