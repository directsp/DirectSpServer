using Newtonsoft.Json;
using DirectSp.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using DirectSp.Core.Infrastructure;
using System.Data.SqlClient;
using System;

namespace DirectSp.Core.InternalDb
{
    public class DspSqlKeyValue : IDspKeyValue
    {
        private string _connectionString;

        public DspSqlKeyValue(string connectionString)
        {
            _connectionString = connectionString;
        }

        public SpInvoker SpInvoker { get; private set; }

        public async Task<List<DspKeyValueItem>> All(string keyNamePattern = null)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            using (var sqlCommand = new SqlCommand("KeyValue_All", sqlConnection))
            {
                sqlConnection.Open();
                sqlCommand.Parameters.AddWithValue("KeyNamePattern", keyNamePattern);
                sqlCommand.Parameters.AddWithValue("Context", "$$");

                var dataReader = await sqlCommand.ExecuteReaderAsync();
                var keyValueItems = new List<DspKeyValueItem>();

                while (dataReader.Read())
                {
                    var keyValueItem = new DspKeyValueItem
                    {
                        KeyName = (string)dataReader["KeyName"],
                        ModifiedTime = (DateTime?)dataReader["ModifiedTime"],
                        TextValue = (string)dataReader["TextValue"]
                    };

                    keyValueItems.Add(keyValueItem);
                }
                return keyValueItems;
            }
        }

        public async Task SetValue(string keyName, string value, int timeToLife = 0, bool isOverwrite = true)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            using (var sqlCommand = new SqlCommand("KeyValue_ValueSet", sqlConnection))
            {
                sqlConnection.Open();
                sqlCommand.Parameters.AddWithValue("KeyName", keyName);
                sqlCommand.Parameters.AddWithValue("TextValue", value);
                sqlCommand.Parameters.AddWithValue("TimeToLife", timeToLife);
                sqlCommand.Parameters.AddWithValue("IsOverwrite", isOverwrite);
                sqlCommand.Parameters.AddWithValue("Context", "$$");

                await sqlCommand.ExecuteNonQueryAsync();

            }
        }

        public async Task<object> GetValue(string keyName)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            using (var sqlCommand = new SqlCommand("KeyValue_Value", sqlConnection))
            {
                sqlConnection.Open();
                sqlCommand.Parameters.AddWithValue("KeyName", keyName);
                sqlCommand.Parameters.AddWithValue("Context", "$$");

                await sqlCommand.ExecuteNonQueryAsync();
                return sqlCommand.Parameters["TextValue"].Value;
            }
        }

        public async Task Delete(string keyNamePattern)
        {
            using (var sqlConnection = new SqlConnection(_connectionString))
            using (var sqlCommand = new SqlCommand("KeyValue_Delete", sqlConnection))
            {
                sqlConnection.Open();
                sqlCommand.Parameters.AddWithValue("KeyNamePattern", keyNamePattern);
                sqlCommand.Parameters.AddWithValue("Context", "$$");

                await sqlCommand.ExecuteNonQueryAsync();
            }
        }

    }
}