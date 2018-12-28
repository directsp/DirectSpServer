using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json;
using DirectSp.Core.ProcedureInfos;
using DirectSp.Core.Infrastructure;

namespace DirectSp.Core
{
    static class ResourceDb
    {
        public static SpInfo[] System_Api(IDbLayer dbLayer, SqlConnection sqlConnection, out string context)
        {
            using (var command = new SqlCommand("api.System_Api", sqlConnection))
            {
                var sqlParameters = new List<SqlParameter>()
                {
                    new SqlParameter("@Context",SqlDbType.NVarChar, -1) { Direction = ParameterDirection.InputOutput, Value = "$$" },
                    new SqlParameter("@Api", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output},
                };

                //create command and run it
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddRange(sqlParameters.ToArray());
                dbLayer.OpenConnection(sqlConnection);
                var res = dbLayer.ExcuteNonQuery(command);

                context = sqlParameters.Find(x => x.ParameterName == "@Context").Value as string; //context
                var api = sqlParameters.Find(x => x.ParameterName == "@Api").Value as string;
                api = api.Replace("'sql_variant'", "'variant'");
                var ret = JsonConvert.DeserializeObject<SpInfo[]>(api);
                return ret;
            }

        }
    }
}

