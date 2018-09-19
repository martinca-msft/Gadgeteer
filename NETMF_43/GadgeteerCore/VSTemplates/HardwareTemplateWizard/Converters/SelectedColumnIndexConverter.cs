// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace Microsoft.Gadgeteer.HardwareTemplateWizard
{
    public class SelectedColumnIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DataGrid grid = value as DataGrid;

            if (grid == null || grid.SelectedCells.Count < 1)
                return -1;

            return grid.SelectedCells[0].Column.DisplayIndex;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
