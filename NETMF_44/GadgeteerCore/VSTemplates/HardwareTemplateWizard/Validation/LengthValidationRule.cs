// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System.Globalization;
using System.Windows.Controls;

namespace Microsoft.Gadgeteer.HardwareTemplateWizard
{
    public class LengthValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string valueString = (string)value;

            double length;
            bool success = MillimeterConverter.TryParse(valueString, cultureInfo, out length) && length > 0;

            return new ValidationResult(success, "Enter value greater than zero.");
        }
    }
}
