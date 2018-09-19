// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using Microsoft.VisualStudio.Modeling;
using Microsoft.VisualStudio.Modeling.Design;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Gadgeteer.Designer
{
    public class StartAtTwoNameProvider : ElementNameProvider
    {
        protected override void SetUniqueNameCore(ModelElement element, string baseName, IDictionary<string, ModelElement> siblingNames)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            if (baseName == null)
                throw new ArgumentNullException("baseName");

            if (siblingNames == null)
                throw new ArgumentNullException("siblingNames");

            if (DomainProperty.PropertyType != typeof(string))
                throw new NotSupportedException();

            string currentName = DomainProperty.GetValue(element) as string;

            if (!string.IsNullOrEmpty(currentName) && StringComparer.OrdinalIgnoreCase.Compare(currentName, baseName) != 0)
            {
                if (!siblingNames.ContainsKey(currentName))
                    return;

                baseName = currentName;
            }

            for (ulong i = 2; i < ulong.MaxValue; i++)
            {
                string generatedName = string.Format(CultureInfo.InvariantCulture, "{0}{1}", baseName, i);

                if (!siblingNames.ContainsKey(generatedName))
                {
                    ElementPropertyDescriptor.SetProperty(element, DomainProperty, generatedName);
                    return;
                }
            }

            throw new NotSupportedException();
        }
    }
}
