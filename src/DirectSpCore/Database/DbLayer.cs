using DirectSp.Core.Infrastructure;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace DirectSp.Core.Database
{
    class DbLayer : IDbLayer
    {
        public async Task<IDataReader> ExecuteReaderAsync(SqlCommand command)
        {
            return await command.ExecuteReaderAsync();
        }

        public int ExcuteNonQuery(SqlCommand command)
        {
            return command.ExecuteNonQuery();
        }

        public void OpenConnection(SqlConnection connection)
        {
            connection.Open();
        }

        public void OpenConnection(SqlConnection connection)
        {
            connection.Open();
        }
    }
}
