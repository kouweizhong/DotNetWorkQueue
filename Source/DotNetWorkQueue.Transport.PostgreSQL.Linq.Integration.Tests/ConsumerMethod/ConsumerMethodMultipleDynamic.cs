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
using DotNetWorkQueue.IntegrationTests.Shared;
using DotNetWorkQueue.IntegrationTests.Shared.ConsumerMethod;
using DotNetWorkQueue.IntegrationTests.Shared.ProducerMethod;
using DotNetWorkQueue.Transport.PostgreSQL.Basic;
using DotNetWorkQueue.Transport.PostgreSQL.Integration.Tests;
using Xunit;

namespace DotNetWorkQueue.Transport.PostgreSQL.Linq.Integration.Tests.ConsumerMethod
{
    public class ConsumerMethodMultipleDynamic
    {
        [Collection("PostgreSQL")]
        public class SimpleMethodConsumer
        {
           [Theory]
           [InlineData(1000, 0, 240, 5, false),
           InlineData(2000, 0, 240, 25, false),
           InlineData(2000, 0, 240, 25, true),
           InlineData(1000, 0, 240, 5, true)]
            public void Run(int messageCount, int runtime, int timeOut, int workerCount, bool useTransactions)
            {
                var queueName = GenerateQueueName.Create();
                var logProvider = LoggerShared.Create(queueName, GetType().Name);
                using (
                    var queueCreator =
                        new QueueCreationContainer<PostgreSqlMessageQueueInit>(
                            serviceRegister => serviceRegister.Register(() => logProvider, LifeStyles.Singleton)))
                {
                    try
                    {
                        using (
                            var oCreation =
                                queueCreator.GetQueueCreation<PostgreSqlMessageQueueCreation>(queueName,
                                    ConnectionInfo.ConnectionString)
                            )
                        {
                            oCreation.Options.EnableDelayedProcessing = true;
                            oCreation.Options.EnableHeartBeat = !useTransactions;
                            oCreation.Options.EnableHoldTransactionUntilMessageCommitted = useTransactions;
                            oCreation.Options.EnableStatus = !useTransactions;
                            oCreation.Options.EnableStatusTable = true;

                            var result = oCreation.CreateQueue();
                            Assert.True(result.Success, result.ErrorMessage);

                            var id = Guid.NewGuid();
                            var producer = new ProducerMethodMultipleDynamicShared();
                            producer.RunTestDynamic<PostgreSqlMessageQueueInit>(queueName,
                                ConnectionInfo.ConnectionString, false, messageCount, logProvider, Helpers.GenerateData,
                                Helpers.Verify, false, id, GenerateMethod.CreateMultipleDynamic, runtime);

                            var consumer = new ConsumerMethodShared();
                            consumer.RunConsumer<PostgreSqlMessageQueueInit>(queueName, ConnectionInfo.ConnectionString,
                                false,
                                logProvider,
                                runtime, messageCount,
                                workerCount, timeOut, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(12), id);

                            new VerifyQueueRecordCount(queueName, oCreation.Options).Verify(0, false, false);
                        }
                    }
                    finally
                    {
                        using (
                            var oCreation =
                                queueCreator.GetQueueCreation<PostgreSqlMessageQueueCreation>(queueName,
                                    ConnectionInfo.ConnectionString)
                            )
                        {
                            oCreation.RemoveQueue();
                        }
                    }
                }
            }
        }
    }
}
