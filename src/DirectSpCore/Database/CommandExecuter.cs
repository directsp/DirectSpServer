﻿using DirectSp.Core.Infrastructure;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace DirectSp.Core.Database
{
    class CommandExecuter : ICommandExecuter
    {
        public async Task<IDataReader> ExecuteReaderAsync(SqlCommand command)
        {
            try
            {
                command.Connection.Open();
                return await command.ExecuteReaderAsync();
            }
            finally
            {
                command.Connection.Close();
            }
        }

        public int ExcuteNonQuery(SqlCommand command)
        {
            try
            {
                command.Connection.Open();
                return command.ExecuteNonQuery();
            }
            finally
            {
                command.Connection.Close();
            }
        }
    }
}