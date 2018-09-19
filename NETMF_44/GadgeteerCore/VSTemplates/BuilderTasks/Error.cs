// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICollection = System.Collections.ICollection;

namespace Microsoft.Gadgeteer.BuilderTasks
{
    internal class Error
    {
        // We might want to provide an environment variable for this one.
        private static readonly bool _throwExceptions = true;

        internal static T IfNull<T>(T parameter, string parameterName)
        {
            if (parameter == null && _throwExceptions)
                throw new ArgumentNullException(string.Format("Argument \"{0}\" cannot be null.", parameterName));

            return parameter;
        }

        internal static void EnsureSameVectorSize(ICollection u, ICollection v, string uName, string vName)
        {
            int uSize = 0;
            int vSize = 0;

            if (u != null) uSize = u.Count;
            if (v != null) vSize = v.Count;

            if (uSize != vSize)
            {
                throw new ArgumentException(string.Format("MSB3094: '{2}' refers to {0} item(s), and '{3}' refers to {1} item(s). They must have the same number of items.", uSize, vSize, uName, vName));
            }            
        }
    }
}
