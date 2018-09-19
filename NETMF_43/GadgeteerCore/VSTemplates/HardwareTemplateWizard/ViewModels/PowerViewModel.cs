// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using System.Windows;

namespace Microsoft.Gadgeteer.HardwareTemplateWizard
{
    public class PowerViewModel : DependencyObject
    {
        public static readonly DependencyProperty VoltageProperty = DependencyProperty.Register("Voltage", typeof(double), typeof(PowerViewModel), new PropertyMetadata(3.3), ValidateNonZero);
        public static readonly DependencyProperty TypicalCurrentProperty = DependencyProperty.Register("TypicalCurrent", typeof(double), typeof(PowerViewModel), null, ValidateNonNegative);
        public static readonly DependencyProperty MaximumCurrentProperty = DependencyProperty.Register("MaximumCurrent", typeof(double), typeof(PowerViewModel), null, ValidateNonNegative);

        private static bool ValidateNonZero(object value)
        {
            double d = (double)value;
            return d != 0.0 && !double.IsInfinity(d) && !double.IsNaN(d);
        }
        private static bool ValidateNonNegative(object value)
        {
            return (double)value >= 0.0;
        }

        public double Voltage
        {
            get { return (double)GetValue(VoltageProperty); }
            set { SetValue(VoltageProperty, value); }
        }

        public double TypicalCurrent
        {
            get { return (double)GetValue(TypicalCurrentProperty); }
            set { SetValue(TypicalCurrentProperty, value); }
        }

        public double MaximumCurrent
        {
            get { return (double)GetValue(MaximumCurrentProperty); }
            set { SetValue(MaximumCurrentProperty, value); }
        }
    }
}
