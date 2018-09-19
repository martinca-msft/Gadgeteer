// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using System.Globalization;
using System.Windows.Data;

namespace Microsoft.Gadgeteer.HardwareTemplateWizard
{
    internal class MillimeterConverter : IValueConverter
    {
        private const int Pixels = 5;
        private static string[] UnitStrings = { "mm", "cm", "in", "mil", "mils", "px" };
        private static double[] UnitFactors =
        {
            1.0,
            10.0,
            25.4,
            0.0254,
            0.0254,
            25.4 / 96.0,
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double length = (double)value;

            if (targetType == typeof(string))
                return length.ToString(CultureInfo.InvariantCulture);

            if (targetType == typeof(double))
            {
                string unit = parameter as string;

                for (int i = 0; i < UnitStrings.Length; i++)
                    if (string.Equals(UnitStrings[i], unit, StringComparison.OrdinalIgnoreCase))
                        return length / UnitFactors[i];

                return length / UnitFactors[Pixels];
            }

            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
                return Parse((string)value, CultureInfo.InvariantCulture);

            if (value is double)
            {
                double length = (double)value;
                string unit = parameter as string;

                for (int i = 0; i < UnitStrings.Length; i++)
                    if (string.Equals(UnitStrings[i], unit, StringComparison.OrdinalIgnoreCase))
                        return length * UnitFactors[i];

                return length * UnitFactors[Pixels];
            }

            throw new NotSupportedException();
        }

        internal static bool TryParse(string s, IFormatProvider provider, out double value)
        {
            string valueString = s.Trim();
            int valueLength = valueString.Length;

            int unitLength = 0;
            int unitIndex = 0;
            double unitFactor = 1.0;

            for (int i = 0; i < UnitStrings.Length; i++)
                if (valueString.EndsWith(UnitStrings[i], StringComparison.OrdinalIgnoreCase))
                {
                    unitLength = UnitStrings[i].Length;
                    unitFactor = UnitFactors[i];
                    unitIndex = i;
                    break;
                }

            valueString = valueString.Substring(0, valueLength - unitLength);

            if (!double.TryParse(valueString, NumberStyles.Float, provider, out value))
                return false;

            value *= unitFactor;
            return true;
        }

        internal static double Parse(string s, IFormatProvider provider)
        {
            double value;
            if (!TryParse(s, provider, out value))
                throw new FormatException();

            return value;
        }
    }
}
