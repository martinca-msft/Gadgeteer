// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using Microsoft.SPOT;
using System.Collections;
using System.Threading;

namespace Gadgeteer
{
    /// <summary>
    /// Provides the thread scheduling services.
    /// </summary>
    public interface IDispatcher
    {
        /// <summary>
        /// Starts dispatching the operation queue.
        /// </summary>
        void Run();
        /// <summary>
        /// Checks that the calling thread has access to this object.
        /// </summary>
        /// <returns>true if the calling thread has access to this object; false otherwise.</returns>
        bool CheckAccess();
        /// <summary>
        /// Executes the specified delegate asynchronously with the specified arguments, on the thread that the Dispatcher was created on. 
        /// </summary>
        /// <param name="method">A delegate to a method that takes parameters of the same number and type that are contained in the args parameter.</param>
        /// <param name="args">An object to pass as the argument to the given method. This can be null if no arguments are needed.</param>
        void BeginInvoke(Delegate method, params object[] args);
    }

    internal class SingleFrameDispatcher : IDispatcher
    {
        private Thread _thread;
        private AutoResetEvent _event;
        private object _instanceLock;
        private Queue _queue;

        public SingleFrameDispatcher()
        {
            _thread = Thread.CurrentThread;
            _queue = new Queue();
            _event = new AutoResetEvent(false);
            _instanceLock = new object();
        }

        public bool CheckAccess()
        {
            return _thread == Thread.CurrentThread;
        }

        public void Run()
        {
            while (true)
            {
                Operation op = null;

                if (_queue.Count > 0)
                    op = (Operation)_queue.Dequeue();

                if (op != null)
                {
                    try
                    {
                        op.Method.Method.Invoke(op.Method.Target, op.Args);
                    }
                    catch
                    {
                        Debug.Print("Error invoking method \"" + op.Method.Method.DeclaringType + "\" (check arguments to Program.BeginInvoke are correct)");
                    }
                }
                else
                    _event.WaitOne();
            }
        }

        private class Operation
        {
            public Operation(Delegate method, object[] args)
            {
                Method = method;
                Args = args;
            }
            public Delegate Method;
            public object[] Args;
        }

        public void BeginInvoke(Delegate method, params object[] args)
        {
            Operation operation = new Operation(method, args);
            _queue.Enqueue(operation);
            _event.Set();
        }
    }
}
