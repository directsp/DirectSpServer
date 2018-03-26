using DirectSpLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json;
using DirectSpLib.Entities;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace DirectSpLib
{
    static class ResourceDb
    {

        public static string System_AppUserContext(SqlConnection connection)
        {
            using (var command = new SqlCommand("api.System_AppUserContext", connection))
            {
                var AppUserContextOut = new SqlParameter("@AppUserContext", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(AppUserContextOut);
                var res = command.ExecuteNonQuery();
                return AppUserContextOut.Value.ToString();
            }
        }

        public static SpInfo[] System_Api(SqlConnection connection, string context)
        {
            using (var command = new SqlCommand("api.System_Api", connection))
            {
                var sqlParameters = new List<SqlParameter>()
                {
                    new SqlParameter("@Context",SqlDbType.NVarChar, -1) { Direction = ParameterDirection.InputOutput, Value = context },
                    new SqlParameter("@Api", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output},
                };

                //create command and run it
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddRange(sqlParameters.ToArray());
                var res = command.ExecuteNonQuery();

                context = sqlParameters.Find(x => x.ParameterName == "@Context").Value as string; //context
                var Metadata = sqlParameters.Find(x => x.ParameterName == "@Api").Value as string;
                Metadata = Metadata.Replace("\"sql_variant\"", "\"variant\"");
                var ret = JsonConvert.DeserializeObject<SpInfo[]>(Metadata);
                return ret;
            }
        }
    }
}

