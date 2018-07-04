using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using DirectSp.Core.Infrastructure;

namespace DirectSp.Core.Test.Mock
{
    class CommandExecuter : ICommandExecuter
    {
        public int ExcuteNonQuery(SqlCommand command)
        {
            switch (command.CommandText)
            {
                case "api.System_Api":
                    {
                        command.Parameters[0].Value = Data.AppContext();
                        command.Parameters[1].Value = Data.SystemApi();
                        return 1;
                    }
                default:
                    throw new System.NotImplementedException($" test command for {command.CommandText}");
            }
        }

        public Task<IDataReader> ExecuteReaderAsync(SqlCommand command)
        {
            switch (command.CommandText)
            {
                case "api.TestApi":
                    return Task.Factory.StartNew(() => Data.DataReaderForTestSp());

                case "api.SignJwtToken":
                        command.Parameters["@JwtToken"].Value = Data.JwtToken();
                        return Task.Factory.StartNew(() => Data.EmptyDataReader());

                case "api.SignJwtTokenChecking":
                        return Task.Factory.StartNew(() => Data.EmptyDataReader());

                case "api.ParallelSp":
                    command.Parameters["@Param1"].Value = "ResultValue";
                    return Task.Factory.StartNew(() => Data.EmptyDataReader());

                default:
                    throw new System.NotImplementedException($" test command for {command.CommandText}");
            }
        }

    }

}
