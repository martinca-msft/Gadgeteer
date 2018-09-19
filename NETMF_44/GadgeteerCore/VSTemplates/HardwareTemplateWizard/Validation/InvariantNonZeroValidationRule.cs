// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using System.Globalization;
using System.Windows.Controls;

namespace Microsoft.Gadgeteer.HardwareTemplateWizard
{
    public class InvariantNonZeroValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            bool success = false;
            object error = "Value must be finite and non-zero.";

            try
            {
                double d = (double)Convert.ChangeType(value, typeof(double), CultureInfo.InvariantCulture);
                success = d != 0.0 && !double.IsInfinity(d) && !double.IsNaN(d);
            }
            catch (Exception e)
            {
                error = e;
            }

            return new ValidationResult(success, error);
        }
    }
}
