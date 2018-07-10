using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace DirectSp.Core.Infrastructure
{
    public interface IDbLayer
    {

        Task<IDataReader> ExecuteReaderAsync(SqlCommand command);

        int ExcuteNonQuery(SqlCommand command);

        void OpenConnection(SqlConnection connection);

        void CloseConnection(SqlConnection connection);
    }
}
