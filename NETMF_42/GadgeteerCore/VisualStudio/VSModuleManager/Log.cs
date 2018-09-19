// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using System.Diagnostics;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.Gadgeteer.Designer
{
    /// <summary>
    /// Helper class to centralize logging for the designer. Initialize must be called first with a pointer to the IVsActivityLog service.    
    /// </summary>
    internal static class Log
    {
        private static IVsActivityLog iVsLog;

        internal static void Initialize(IVsActivityLog ivsal)
        {
            if (iVsLog == null)
                iVsLog = ivsal;
        }

        internal static void WriteError(Exception e)
        {
            WriteError(e.ToString());
        }

        internal static void WriteErrorIf(bool condition, string message, params object[] args)
        {
            if (condition)
                WriteError(message, args);
        }

        internal static void WriteWarningIf(bool condition, string message, params object[] args)
        {
            if (condition)
                WriteWarning(message, args);
        }

        internal static void WriteError(string message, params object[] args)
        {
            Write(__ACTIVITYLOG_ENTRYTYPE.ALE_ERROR, message, args);
        }

        internal static void WriteWarning(string message, params object[] args)
        {
            Write(__ACTIVITYLOG_ENTRYTYPE.ALE_WARNING, message, args);
        }

        internal static void WriteInfo(string message, params object[] args)
        {
            Write(__ACTIVITYLOG_ENTRYTYPE.ALE_INFORMATION, message, args);            
        }

        private static void Write(__ACTIVITYLOG_ENTRYTYPE entryType, string message, object[] args)
        {
            string entry = string.Format(message, args);
            if(iVsLog!=null)
                iVsLog.LogEntry((uint)entryType, "Gadgeteer", entry);            
            Trace.WriteLine(entry);

            //Debug.Assert(entryType != __ACTIVITYLOG_ENTRYTYPE.ALE_ERROR, entry);
        }        
    }
}
