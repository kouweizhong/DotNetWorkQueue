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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DotNetWorkQueue.Validation;

namespace DotNetWorkQueue.Interceptors
{
    /// <summary>
    /// A collection of IInterceptor, and the registration holder
    /// </summary>
    internal class MessageInterceptors : List<IMessageInterceptor>, IMessageInterceptorRegistrar
    {
        private readonly IInterceptorFactory _interceptorFactory;
        private readonly ConcurrentDictionary<Type, IMessageInterceptor> _createdInterceptors; 
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageInterceptors" /> class.
        /// </summary>
        /// <param name="interceptors">The interceptors.</param>
        /// <param name="interceptorFactory">The interceptor factory.</param>
        public MessageInterceptors(IEnumerable<IMessageInterceptor> interceptors,
            IInterceptorFactory interceptorFactory)
        {
            _interceptorFactory = interceptorFactory;
            _createdInterceptors = new ConcurrentDictionary<Type, IMessageInterceptor>();
            AddInterceptors(interceptors.ToList());
        }
        /// <summary>
        /// Runs all <see cref="IMessageInterceptor"/> in the collection on the input, in order
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public MessageInterceptorsResult MessageToBytes(byte[] input)
        {
            if (Count == 0) return new MessageInterceptorsResult {Output = input};
            var output = new MessageInterceptorsResult();
            var bytes = input;
            foreach (var interceptor in this)
            {
                var temp = interceptor.MessageToBytes(bytes);
                if (temp.AddToGraph)
                {
                    output.Graph.Add(temp.BaseType);
                }
                bytes = temp.Output;
            }
            output.Output = bytes;
            return output;
        }

        /// <summary>
        /// Runs all <see cref="IMessageInterceptor" /> in the collection on the input, in reverse order
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="graph">The graph.</param>
        /// <returns></returns>
        public byte[] BytesToMessage(byte[] input, MessageInterceptorsGraph graph)
        {
            if (!graph.Types.Any())  //an empty graph means no interceptors where used
            {
                return input;
            }

            var temp = graph.Types.Reverse();
            return temp.Select(GetInterceptor).Aggregate(input, (current, interceptor) => interceptor.BytesToMessage(current));
        }
        /// <summary>
        /// Adds the interceptors.
        /// </summary>
        /// <param name="interceptors">The interceptors.</param>
        private void AddInterceptors(List<IMessageInterceptor> interceptors)
        {
            if (interceptors.Count == 0)
                return;

            Guard.IsValid(() => Count, Count, i => i == 0,
               "AddInterceptors cannot be called more than once");

            foreach (var interceptor in interceptors)
            {
                Add(interceptor);
            }
        }

        private IMessageInterceptor GetInterceptor(Type type)
        {
            foreach (var interceptor in this.Where(interceptor => interceptor.GetType() == type))
            {
                return interceptor;
            }
            if (_createdInterceptors.ContainsKey(type)) return _createdInterceptors[type];
            var newInterceptor = _interceptorFactory.Create(type);
            _createdInterceptors.TryAdd(type, newInterceptor);
            return _createdInterceptors[type];
        }
    }
}
