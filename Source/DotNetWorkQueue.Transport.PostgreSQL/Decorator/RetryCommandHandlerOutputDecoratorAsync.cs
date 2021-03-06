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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNetWorkQueue.Logging;
using DotNetWorkQueue.Transport.RelationalDatabase;
using DotNetWorkQueue.Validation;
using Npgsql;

namespace DotNetWorkQueue.Transport.PostgreSQL.Decorator
{
    internal class RetryCommandHandlerOutputDecoratorAsync<TCommand, TOutput> : ICommandHandlerWithOutputAsync<TCommand, TOutput>
    {
        private readonly ICommandHandlerWithOutputAsync<TCommand, TOutput> _decorated;
        private readonly ThreadSafeRandom _threadSafeRandom;
        private readonly ILog _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryCommandHandlerOutputDecorator{TCommand,TOutput}" /> class.
        /// </summary>
        /// <param name="decorated">The decorated.</param>
        /// <param name="log">The log.</param>
        /// <param name="threadSafeRandom">The random.</param>
        public RetryCommandHandlerOutputDecoratorAsync(ICommandHandlerWithOutputAsync<TCommand, TOutput> decorated,
            ILogFactory log,
            ThreadSafeRandom threadSafeRandom)
        {
            Guard.NotNull(() => decorated, decorated);
            Guard.NotNull(() => log, log);
            Guard.NotNull(() => threadSafeRandom, threadSafeRandom);

            _decorated = decorated;
            _log = log.Create();
            _threadSafeRandom = threadSafeRandom;
        }

        /// <summary>
        /// Handles the specified command, retrying up to count for specific errors
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        public async Task<TOutput> HandleAsync(TCommand command)
        {
            Guard.NotNull(() => command, command);
            return await HandleWithCountDownAsync(command, RetryConstants.RetryCount).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the with count down.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        private async Task<TOutput> HandleWithCountDownAsync(TCommand command, int count)
        {
            try
            {
                return await _decorated.HandleAsync(command).ConfigureAwait(false);
            }
            catch (PostgresException sqlEx)
            {
                if (!RetryablePostGreErrors.Errors.Contains(sqlEx.SqlState))
                    throw;

                if (count <= 0)
                    throw;

                var wait = _threadSafeRandom.Next(RetryConstants.MinWait, RetryConstants.MaxWait);
                _log.WarnException($"An error has occurred; we will try to re-run the transaction in {wait} ms", sqlEx);
                Thread.Sleep(wait);

                return await HandleWithCountDownAsync(command, count - 1).ConfigureAwait(false);
            }
        }
    }
}
