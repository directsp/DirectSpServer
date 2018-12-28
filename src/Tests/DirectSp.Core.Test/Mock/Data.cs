//using DirectSp.Core.ProcedureInfos;
//using Moq;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Data;

//namespace DirectSp.Core.Test.Mock
//{
//    internal static class Data
//    {
//        internal static string SystemApi()
//        {
//            return JsonConvert.SerializeObject(new List<SpInfo>
//            {
//                new SpInfo
//                {
//                 SchemaName="api",
//                 Params=new SpParam[]
//                 {
//                     new SpParam
//                     {
//                         ParamName="Test",
//                         IsOutput=true,
//                         Length=0,
//                          SystemTypeName=SqlDbType.NVarChar,
//                          UserTypeName="TSTRING"
//                     }
//                 },
//                 ProcedureName="TestApi"
//                },
//                new SpInfo
//                {
//                 SchemaName="api",
//                 Params=new SpParam[]
//                 {
//                     new SpParam
//                     {
//                         ParamName="@JwtToken",
//                         IsOutput=true,
//                         Length=0,
//                          SystemTypeName=SqlDbType.NVarChar,
//                          UserTypeName="TSTRING"
//                     }
//                 },
//                 ProcedureName="SignJwtToken",
//                 ExtendedProps=new SpInfoEx{
//                     CaptchaMode=SpCaptchaMode.Manual,
//                     DataAccessMode=SpDataAccessMode.Write,
//                     Params=new Dictionary<string, SpParamEx>
//                     {
//                         {"@JwtToken",new SpParamEx{ SignType=SpSignMode.JwtByCertThumb} }
//                     }
//                 }
//                },
//                new SpInfo
//                {
//                 SchemaName="api",
//                 Params=new SpParam[]
//                 {
//                     new SpParam
//                     {
//                         ParamName="@JwtToken",
//                         IsOutput=false,
//                         Length=0,
//                          SystemTypeName=SqlDbType.NVarChar,
//                          UserTypeName="TSTRING"
//                     }
//                 },
//                 ProcedureName="SignJwtTokenChecking",
//                 ExtendedProps=new SpInfoEx{
//                     CaptchaMode=SpCaptchaMode.Manual,
//                     DataAccessMode=SpDataAccessMode.Write,
//                     Params=new Dictionary<string, SpParamEx>
//                     {
//                         {"@JwtToken",new SpParamEx{ SignType=SpSignMode.JwtByCertThumb} }
//                     }
//                 }
//                },
//                new SpInfo
//                {
//                 SchemaName="api",
//                 Params=new SpParam[]
//                 {
//                     new SpParam
//                     {
//                         ParamName="@Param1",
//                         IsOutput=true,
//                         Length=0,
//                          SystemTypeName=SqlDbType.NVarChar,
//                          UserTypeName="TSTRING"
//                     }
//                 },
//                 ProcedureName="ParallelSp",
//                 ExtendedProps=new SpInfoEx
//                 {
//                     IsBatchAllowed=true
//                 }
//                },
//                new SpInfo
//                {
//                 SchemaName="api",
//                 Params=new SpParam[]
//                 {
//                     new SpParam
//                     {
//                         ParamName="@KeyName",
//                         Length=0,
//                          SystemTypeName=SqlDbType.NVarChar,
//                          UserTypeName="TSTRING"
//                     },
//                     new SpParam
//                     {
//                         ParamName="@TextValue",
//                         Length=0,
//                          SystemTypeName=SqlDbType.NVarChar,
//                          UserTypeName="TSTRING"
//                     },
//                     new SpParam
//                     {
//                         ParamName="@TimeToLife",
//                         Length=0,
//                          SystemTypeName=SqlDbType.NVarChar,
//                          UserTypeName="INT"
//                     },
//                     new SpParam
//                     {
//                         ParamName="@IsOverwrite",
//                         Length=0,
//                          SystemTypeName=SqlDbType.NVarChar,
//                          UserTypeName="BIT"
//                     }

//                 },
//                 ProcedureName="KeyValue_ValueSet"
//                }

//            });
//        }

//        internal static string SignedJwtToken()
//        {
//            return @"eyJhbGciOiAiU0hBMjU2IiwgInR5cCI6ICJKV1QifQ==.eyJPcmRlck51bWJlciI6MiwiUmVjZWl2ZXJMb3lhbHR5QWNjb3VudElkIjoxMTAzNzkzMSwiQ2x1Yk5hbWUiOiJOaWtlIiwiQW1vdW50IjoxLCJQb2ludFR5cGVJZCI6MjAyMCwiUG9pbnRUeXBlTmFtZSI6IlRlc3RUeXBlIiwiUGF5ZWVMb3lhbHR5QWNjb3VudElkIjoxMTgzLCJQYXllZUxveWFsdHlBY2NvdW50TmFtZSI6IkJlaG5hbSBFeXZhenBvb3IiLCJleHAiOjI3OTcxMzI3MTUuMTI4NDQyMywiUmV0dXJuVXJsIjoiaHR0cDovL3d3dy5nb29nbGUuY29tIiwiQ2VydGlmaWNhdGVUaHVtYiI6Ijc3IDVhIDkyIDkyIGYxIDlmIDE0IGZjIDJiIDliIDgwIGFiIDA2IDJhIDA2IGJlIDg2IDIzIDYxIDljIn0=.ELVhB5/a5rz0jI2WdIwnrzlOgm8s6eHz0yaCCAff1osfF4dhUWxUcDYVTBWadkHWelIh52qUsP0FVEV1075phsQDPuOPT7RR4BuP72nJzt/PsUoMb6fuKEygdutv3dyKEllZp7VAJny3PeSLf20aOy0MCXzdBDw7ZVF4kz/e62iwFHHqLwLDH1cfXaCAnRdEqtR6tkXwOYbvS1XJVw2fxVBBx1LLDLWD5q8gAtlVIGymI85AuveA477fcb0HzEz5ds9f3Wd0NkkGyolRSNcPlV6MHL/D2c6iF+nx6LDU9HTQ6jKPsKdjnbHRDwDo5Q1NeB8Z4FXHWutDpncRc+yCMA==";
//        }

//        internal static string AppContext()
//        {
//            return "{'AppName':'IcLoyalty','AppVersion':'2.0.14','UserId':'21','InvokeOptions':{'IsCaptcha':true}}".Replace("'", "\"");
//        }

//        internal static string JwtToken => "{'OrderNumber':2,'ReceiverLoyaltyAccountId':11037931,'ClubName':'Nike','Amount':1,'PointTypeId':2020,'PointTypeName':'TestType','PayeeLoyaltyAccountId':1183,'PayeeLoyaltyAccountName':'Behnam Eyvazpoor','exp':2797132715.1284423,'ReturnUrl':'http://www.google.com','CertificateThumb':'77 5a 92 92 f1 9f 14 fc 2b 9b 80 ab 06 2a 06 be 86 23 61 9c'}".Replace("'", "\"");

//        internal static IDataReader DataReaderForTestSp()
//        {
//            var mockDataReader = new Mock<IDataReader>();
//            bool readToggle = true;

//            mockDataReader.Setup(x => x.Read()).Returns(() => readToggle)
//                .Callback(() => readToggle = false);

//            mockDataReader.Setup(dr => dr["TestColumn"])
//                .Returns("Nothing else matter");

//            return mockDataReader.Object;
//        }

//        internal static IDataReader DataReader_KeyValue_All()
//        {
//            var mockDataReader = new Mock<IDataReader>();
//            bool readToggle = true;

//            mockDataReader.Setup(x => x.Read()).Returns(() => readToggle)
//                .Callback(() => readToggle = false);

//            mockDataReader.Setup(dr => dr["KeyName"])
//                .Returns("43046f12-5a0a-446d-9f79-3199e1768f55");

//            mockDataReader.Setup(dr => dr["TextValue"])
//                .Returns("Value01");

//            mockDataReader.Setup(dr => dr["ModifiedDate"])
//                .Returns(DateTime.Now);

//            mockDataReader.Setup(x => x.Read()).Returns(() => readToggle)
//    .Callback(() => readToggle = false);

//            mockDataReader.Setup(dr => dr["KeyName"])
//    .Returns("e93cbf30-9303-4b11-bd97-fe5fbd4a00e5");

//            mockDataReader.Setup(dr => dr["TextValue"])
//                .Returns("Value02");

//            mockDataReader.Setup(dr => dr["ModifiedDate"])
//                .Returns(DateTime.Now);

//            return mockDataReader.Object;
//        }

//        internal static IDataReader EmptyDataReader()
//        {
//            var mockDataReader = new Mock<IDataReader>();

//            mockDataReader.Setup(x => x.Read()).Returns(() => false);
//            return mockDataReader.Object;
//        }

//    }
//}
