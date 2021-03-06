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
using Amib.Threading;
using DotNetWorkQueue.TaskScheduling;
using NSubstitute;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoNSubstitute;
using Xunit;
namespace DotNetWorkQueue.Tests.TaskScheduling
{
    public class WorkGroupWithItemTests
    {
        [Fact]
        public void Test()
        {
            var fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            var group = fixture.Create<IWorkGroup>();
            var threadGroup = fixture.Create<IWorkItemsGroup>();
            var counter = fixture.Create<ICounter>();
            group.ConcurrencyLevel.Returns(5);
            group.MaxQueueSize.Returns(1);
            fixture.Inject(group);
            fixture.Inject(threadGroup);
            fixture.Inject(counter);

            var test = fixture.Create<WorkGroupWithItem>();

            Assert.Equal(group, test.GroupInfo);
            Assert.Equal(threadGroup, test.Group);
            Assert.Equal(counter, test.MetricCounter);
            Assert.Equal(group.ConcurrencyLevel + group.MaxQueueSize, test.MaxWorkItems);
            test.CurrentWorkItems = 0;
            Assert.Equal(0, test.CurrentWorkItems);

            test.CurrentWorkItems = 2;
            Assert.Equal(2, test.CurrentWorkItems);
        }
    }
}
