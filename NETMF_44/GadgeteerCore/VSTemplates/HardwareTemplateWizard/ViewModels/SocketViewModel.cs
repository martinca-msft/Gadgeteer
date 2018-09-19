// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace Microsoft.Gadgeteer.HardwareTemplateWizard
{
    public class SocketViewModel : SocketBaseViewModel
    {
        public static readonly DependencyProperty TypesLabelProperty = DependencyProperty.Register("TypesLabel", typeof(string), typeof(SocketViewModel), new PropertyMetadata("XY", MarkDirty));
        public static readonly DependencyProperty OptionalProperty = DependencyProperty.Register("Optional", typeof(bool), typeof(SocketViewModel), new PropertyMetadata(false, MarkDirty));
        public static readonly DependencyProperty Pin3Property = DependencyProperty.Register("Pin3", typeof(bool?), typeof(SocketBaseViewModel), new PropertyMetadata(false, MarkDirty));
        public static readonly DependencyProperty Pin4Property = DependencyProperty.Register("Pin4", typeof(bool?), typeof(SocketBaseViewModel), new PropertyMetadata(false, MarkDirty));
        public static readonly DependencyProperty Pin5Property = DependencyProperty.Register("Pin5", typeof(bool?), typeof(SocketBaseViewModel), new PropertyMetadata(false, MarkDirty));
        public static readonly DependencyProperty Pin6Property = DependencyProperty.Register("Pin6", typeof(bool?), typeof(SocketBaseViewModel), new PropertyMetadata(false, MarkDirty));
        public static readonly DependencyProperty Pin7Property = DependencyProperty.Register("Pin7", typeof(bool?), typeof(SocketBaseViewModel), new PropertyMetadata(false, MarkDirty));
        public static readonly DependencyProperty Pin8Property = DependencyProperty.Register("Pin8", typeof(bool?), typeof(SocketBaseViewModel), new PropertyMetadata(false, MarkDirty));
        public static readonly DependencyProperty Pin9Property = DependencyProperty.Register("Pin9", typeof(bool?), typeof(SocketBaseViewModel), new PropertyMetadata(false, MarkDirty));

        public string TypesLabel
        {
            get { return (string)GetValue(TypesLabelProperty); }
            set { SetValue(TypesLabelProperty, value); }
        }

        public bool Optional
        {
            get { return (bool)GetValue(OptionalProperty); }
            set { SetValue(OptionalProperty, value); }
        }

        public bool? Pin3 { get { return (bool?)GetValue(Pin3Property); } set { SetValue(Pin3Property, value); } }
        public bool? Pin4 { get { return (bool?)GetValue(Pin4Property); } set { SetValue(Pin4Property, value); } }
        public bool? Pin5 { get { return (bool?)GetValue(Pin5Property); } set { SetValue(Pin5Property, value); } }
        public bool? Pin6 { get { return (bool?)GetValue(Pin6Property); } set { SetValue(Pin6Property, value); } }
        public bool? Pin7 { get { return (bool?)GetValue(Pin7Property); } set { SetValue(Pin7Property, value); } }
        public bool? Pin8 { get { return (bool?)GetValue(Pin8Property); } set { SetValue(Pin8Property, value); } }
        public bool? Pin9 { get { return (bool?)GetValue(Pin9Property); } set { SetValue(Pin9Property, value); } }
    }
}
