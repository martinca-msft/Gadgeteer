// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
namespace Gadgeteer.Modules
{
    using Microsoft.SPOT;
    using Microsoft.SPOT.Hardware;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Threading;
    /// <summary>
    /// Abstract class to provide common methods, properties, and events for modules.
    /// </summary>
    /// <remarks>
    /// All module classes derive from this class, either directly or indirectly. Classes that represent
    /// a module that can be chained together with other modules on the same socket
    /// derive from <see cref="T:Gadgeteer.Modules.DaisyLinkModule"/>,
    /// which dervies from this class.
    /// </remarks>
    public abstract partial class Module
    {
        /// <summary>
        /// Gets a reference to the Mainboard API used by the current Gadgeteer Program.
        /// </summary>
        protected internal static Mainboard Mainboard;

        /// <summary>
        /// Stores a list of modules so that they will not be garbage collected even if user code stops referring to them. (Because some modules are interrupt-driven, they can receive events even then.)
        /// </summary>
        private static ArrayList modules = new ArrayList();
        

        // Note: A constructor summary is auto-generated by the doc builder.
        /// <summary></summary>
        protected Module() 
        {
            if (!Program.Dispatcher.CheckAccess()) throw new Exception("Modules must be created on the Program's Dispatcher thread");
            Module.modules.Add(this);
        }

        /// <summary>
        /// Returns the name of this module class as a string.
        /// </summary>
        /// <returns>The name of this module class as a string.</returns>
        public override string ToString()
        {
            string shortName = this.GetType().Name;
            return shortName;
        }

        /// <summary>
        /// Gets or sets a value that determines whether debug printing is enabled.
        /// </summary>
        /// <remarks>
        ///  If this property is <b>false</b>, the default, calling the <see cref="DebugPrint"/> method has no effect.
        /// </remarks>
        public bool DebugPrintEnabled;
  
        /// <summary>
        /// If <see cref="DebugPrintEnabled"/> is <b>true</b>, prints the name of this module and a specified message to the debug output window.
        /// </summary>
        /// <param name="message">The message to append to the name of this module.</param>
        /// <remarks>
        ///  If <see cref="DebugPrintEnabled"/> is <b>false</b>, the default, calling this method has no effect.
        /// </remarks>
        protected void DebugPrint(string message)
        {
            if (DebugPrintEnabled) Debug.Print(this.ToString() + " : " + message); 
        }

        /// <summary>
        /// Prints the name of this module and a specified error message to the debug output window.
        /// </summary>
        /// <param name="message">The message to append to the name of this module.</param>
        protected void ErrorPrint(string message)
        {
            Debug.Print(this.ToString() + " ERROR : " + message);
        }
    }
}