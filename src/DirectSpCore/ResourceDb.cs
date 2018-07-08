using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json;
using DirectSp.Core.Entities;
using DirectSp.Core.SpSchema;
using DirectSp.Core.DI;
using DirectSp.Core.Infrastructure;

namespace DirectSp.Core
{
    static class ResourceDb
    {
        public static SpInfo[] System_Api(SqlConnection connection, out string context)
        {
            connection.Open();
            using (var command = new SqlCommand("api.System_Api", connection))
            {
                var sqlParameters = new List<SqlParameter>()
                {
                    new SqlParameter("@Context",SqlDbType.NVarChar, -1) { Direction = ParameterDirection.InputOutput, Value = "$$" },
                    new SqlParameter("@Api", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output},
                };

                //create command and run it
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddRange(sqlParameters.ToArray());
                var dbLayer = Resolver.Instance.Resolve<IDbLayer>();
                dbLayer.OpenConnection(connection);
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

