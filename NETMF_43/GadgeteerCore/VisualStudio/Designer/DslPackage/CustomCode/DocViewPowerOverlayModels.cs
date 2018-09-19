// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using System.Collections.Generic;
using System.Windows;

namespace Microsoft.Gadgeteer.Designer
{
    // needs some work to make it localizable

    internal class OverlayModel
    {
        public SortedDictionary<double, PowerRequirementsModel> Requirements { get; set; }
        public List<string> MissingModels { get; set; }
        public int MissingMainboardCount { get; set; }
        public int MissingModuleCount { get; set; }

        public string MissingModelsString { get { return string.Join(Environment.NewLine, MissingModels); } }
        public string Note
        {
            get
            {
                string mainboard = MissingMainboardCount == 1 ? "the mainboard" : string.Format("{0} mainboards", MissingMainboardCount);
                string module = MissingModuleCount == 1 ? "1 module" : string.Format("{0} modules", MissingModuleCount);

                return string.Format("The power requirements of {0}{2}{1} are unknown.",
                    MissingMainboardCount > 0 ? mainboard : "",
                    MissingModuleCount > 0 ? module : "",
                    MissingMainboardCount > 0 && MissingModuleCount > 0 ? " and " : "");
            }
        }
        public Visibility NoteVisibility
        {
            get
            {
                if (MissingMainboardCount > 0 || MissingModuleCount > 0)
                {
                    return System.Windows.Visibility.Visible;
                }
                else
                {
                    return System.Windows.Visibility.Collapsed;
                }
            }
        }

        public OverlayModel()
        {
            Requirements = new SortedDictionary<double, PowerRequirementsModel>();
            MissingModels = new List<string>();
        }
    }

    internal class PowerRequirementsModel
    {
        private const double MilliLimit = 2;

        public double Voltage { get; set; }
        public double Typical { get; set; }
        public double Maximum { get; set; }
        public bool TypicalMore { get; set; }
        public bool MaximumMore { get; set; }
        public string VoltageString { get { return Voltage + " V"; } }
        public string TypicalString { get { return FormatValue(Typical, TypicalMore, "A"); } }
        public string MaximumString { get { return FormatValue(Maximum, MaximumMore, "A"); } }

        private string FormatValue(double value, bool more, string unit)
        {
            if (double.IsInfinity(value))
                return "∞";

            if (value == 0.0)
                return "—";

            string valueFormat = "{0}{1:#,0.##} {2}";
            if (value < MilliLimit)
            {
                value *= 1000;
                valueFormat = "{0}{1:N0} m{2}";
            }

            return string.Format(valueFormat, more ? "≥ " : "", value, unit);
        }
    }
}
