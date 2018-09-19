// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;

namespace Microsoft.Gadgeteer.AppTemplateWizard
{
    internal class MainboardNamingFirstComparer : StringComparer
    {
        private const string ModulePrefix = "GTM.";

        public override int Compare(string x, string y)
        {
            if (x == null || y == null)
                return string.Compare(x, y);

            if (object.ReferenceEquals(x, y))
                return 0;

            if (x.StartsWith(ModulePrefix))
            {
                if (y.StartsWith(ModulePrefix))
                    return string.Compare(x, y);

                else
                    return 1;
            }
            else
            {
                if (y.StartsWith(ModulePrefix))
                    return -1;

                else
                    return string.Compare(x, y);
            }
        }

        public override bool Equals(string x, string y)
        {
            return string.Equals(x, y);
        }

        public override int GetHashCode(string obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            return obj.GetHashCode();
        }
    }
}
