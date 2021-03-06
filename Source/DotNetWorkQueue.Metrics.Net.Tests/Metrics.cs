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
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace DotNetWorkQueue.Metrics.Net.Tests
{
    public class Metrics
    {
        [Theory, AutoData]
        public void Create(string name, string name2, string name3)
        {
            using (var metric = new Net.Metrics(name))
            {
                Assert.NotNull(metric.Config);
                metric.Gauge(name, () => 1, Units.Bytes);
                Assert.NotNull(metric.Meter(name, Units.Bytes, TimeUnits.Seconds));
                Assert.NotNull(metric.Meter(name2, name2, TimeUnits.Seconds));
                Assert.NotNull(metric.Counter(name, Units.Bytes));
                Assert.NotNull(metric.Counter(name2, name2));
                Assert.NotNull(metric.Histogram(name, Units.Bytes, SamplingTypes.FavorRecent));
                Assert.NotNull(metric.Timer(name, Units.Bytes, SamplingTypes.FavorRecent, TimeUnits.Seconds, TimeUnits.Seconds));

                var context = metric.Context(name3);
                Assert.NotNull(context);
                metric.ShutdownContext(name3);
                Assert.NotNull(metric.CollectedMetrics);
            }
        }
    }
}
