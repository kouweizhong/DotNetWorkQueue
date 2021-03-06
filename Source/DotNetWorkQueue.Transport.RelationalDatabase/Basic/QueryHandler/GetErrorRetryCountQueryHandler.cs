﻿// ---------------------------------------------------------------------
//This file is part of DotNetWorkQueue
//Copyright © 2017 Brian Lehnen
//
//This library is free software; you can redistribute it and/or
//modify it under the terms of the GNU Lesser General Public
//License as published by the Free Software Foundation; either
//version 2.1 of the License, or (at your option) any later version.
//
//This library is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//Lesser General Public License for more details.
//
//You should have received a copy of the GNU Lesser General Public
//License along with this library; if not, write to the Free Software
//Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
// ---------------------------------------------------------------------
using System;
using System.Data;
using DotNetWorkQueue.Transport.RelationalDatabase.Basic.Query;
using DotNetWorkQueue.Validation;

namespace DotNetWorkQueue.Transport.RelationalDatabase.Basic.QueryHandler
{
    /// <summary>
    /// Returns the current retry count for a message and a specific exception type
    /// </summary>
    internal class GetErrorRetryCountQueryHandler : IQueryHandler<GetErrorRetryCountQuery, int>
    {
        private readonly CommandStringCache _commandCache;
        private readonly IDbConnectionFactory _connectionFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetErrorRetryCountQueryHandler" /> class.
        /// </summary>
        /// <param name="commandCache">The command cache.</param>
        /// <param name="connectionFactory">The connection factory.</param>
        public GetErrorRetryCountQueryHandler(CommandStringCache commandCache, 
            IDbConnectionFactory connectionFactory)
        {
            Guard.NotNull(() => commandCache, commandCache);
            Guard.NotNull(() => connectionFactory, connectionFactory);

            _commandCache = commandCache;
            _connectionFactory = connectionFactory;
        }
        /// <summary>
        /// Handles the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public int Handle(GetErrorRetryCountQuery query)
        {
            using (var connection = _connectionFactory.Create())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = _commandCache.GetCommand(CommandStringTypes.GetErrorRetryCount);

                    var queueid = command.CreateParameter();
                    queueid.ParameterName = "@QueueID";
                    queueid.DbType = DbType.Int64;
                    queueid.Value = query.QueueId;
                    command.Parameters.Add(queueid);

                    var exceptionType = command.CreateParameter();
                    exceptionType.ParameterName = "@ExceptionType";
                    exceptionType.DbType = DbType.AnsiString;
                    exceptionType.Size = 500;
                    exceptionType.Value = query.ExceptionType;
                    command.Parameters.Add(exceptionType);

                    var o = command.ExecuteScalar();
                    if (o != null && o != DBNull.Value)
                    {
                        return Convert.ToInt32(o);
                    }
                }
            }
            return 0;
        }
    }
}
