using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace DirectSp.Core.Infrastructure
{
    public interface ICommandExecuter
    {

        Task<IDataReader> ExecuteReaderAsync(SqlCommand command);

        int ExcuteNonQuery(SqlCommand command);
    }
}
