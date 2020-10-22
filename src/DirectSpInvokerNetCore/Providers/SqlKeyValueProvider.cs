using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Data;
using System.Data.SqlClient;
using DirectSp.Exceptions;

namespace DirectSp.Providers
{
    public class SqlKeyValueProvider : IKeyValueProvider
    {
        private readonly string _connectionString;

        public SqlKeyValueProvider(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<KeyValueItem>> All(string keyNamePattern = null)
        {
            using var sqlConnection = new SqlConnection(_connectionString);
            using var sqlCommand = new SqlCommand("api.KeyValue_All", sqlConnection);
            sqlConnection.Open();
            sqlCommand.CommandType = CommandType.StoredProcedure;
            sqlCommand.Parameters.AddWithValue("@KeyNamePattern", keyNamePattern);
            sqlCommand.Parameters.Add(new SqlParameter("@Context", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.InputOutput, Value = "$$" });

            var dataReader = await sqlCommand.ExecuteReaderAsync();
            var keyValueItems = new List<KeyValueItem>();

            while (dataReader.Read())
            {
                var keyValueItem = new KeyValueItem
                {
                    KeyName = (string)dataReader["KeyName"],
                    ModifiedTime = (DateTime?)dataReader["ModifiedTime"],
                    TextValue = (string)dataReader["TextValue"]
                };

                keyValueItems.Add(keyValueItem);
            }
            return keyValueItems;
        }

        public async Task SetValue(string keyName, string value, int timeToLife = 0, bool isOverwrite = true)
        {
            using var sqlConnection = new SqlConnection(_connectionString);
            using var sqlCommand = new SqlCommand("api.KeyValue_ValueSet", sqlConnection);
            sqlConnection.Open();
            sqlCommand.CommandType = CommandType.StoredProcedure;
            sqlCommand.Parameters.Add(new SqlParameter("Context", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.InputOutput, Value = "$$" });
            sqlCommand.Parameters.AddWithValue("KeyName", keyName);
            sqlCommand.Parameters.AddWithValue("TextValue", value);
            sqlCommand.Parameters.AddWithValue("TimeToLife", timeToLife);
            sqlCommand.Parameters.AddWithValue("IsOverwrite", isOverwrite);

            try
            {
                await sqlCommand.ExecuteNonQueryAsync();
            }
            catch (SqlException ex)
            {
                if (ex.Errors[0].Number == 55004)
                    throw new SpObjectAlreadyExists();
                throw ex;
            }
        }

        public async Task<object> GetValue(string keyName)
        {
            using var sqlConnection = new SqlConnection(_connectionString);
            using var sqlCommand = new SqlCommand("api.KeyValue_Value", sqlConnection);
            sqlConnection.Open();
            sqlCommand.CommandType = CommandType.StoredProcedure;
            sqlCommand.Parameters.AddWithValue("KeyName", keyName);
            sqlCommand.Parameters.Add(new SqlParameter("TextValue", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output, Value = "$$" });
            sqlCommand.Parameters.Add(new SqlParameter("ModifiedTime", SqlDbType.DateTime, -1) { Direction = ParameterDirection.Output, Value = "$$" });
            sqlCommand.Parameters.Add(new SqlParameter("Context", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.InputOutput, Value = "$$" });
            try
            {
                await sqlCommand.ExecuteNonQueryAsync();
            }
            catch (SqlException ex)
            {
                if (ex.Errors[0].Number == 55002)
                    throw new SpAccessDeniedOrObjectNotExistsException();
                throw ex;
            }
            return new KeyValueItem
            {
                KeyName = keyName,
                TextValue = (string)sqlCommand.Parameters["TextValue"].Value,
                ModifiedTime = (DateTime?)sqlCommand.Parameters["ModifiedTime"].Value
            };
        }

        public async Task<bool> Delete(string keyNamePattern)
        {
            using var sqlConnection = new SqlConnection(_connectionString);
            using var sqlCommand = new SqlCommand("api.KeyValue_Delete", sqlConnection);
            sqlConnection.Open();
            sqlCommand.CommandType = CommandType.StoredProcedure;
            sqlCommand.Parameters.AddWithValue("KeyNamePattern", keyNamePattern);
            sqlCommand.Parameters.Add(new SqlParameter("AffectedCount", SqlDbType.Int) { Direction = ParameterDirection.Output });
            sqlCommand.Parameters.AddWithValue("Context", "$$");

            await sqlCommand.ExecuteNonQueryAsync();
            return ((int)sqlCommand.Parameters["AffectedCount"].Value) > 0;
        }

    }
}