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
    public class ProvidedSocketViewModel : SocketBaseViewModel
    {
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("Label", typeof(string), typeof(ProvidedSocketViewModel), new PropertyMetadata("", MarkDirty));
        public static readonly DependencyProperty TypesProperty = DependencyProperty.Register("Types", typeof(string), typeof(SocketViewModel), new PropertyMetadata("XY", MarkDirty));

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public string Types
        {
            get { return (string)GetValue(TypesProperty); }
            set { SetValue(TypesProperty, value); }
        }

        public static readonly DependencyProperty Net3Property = DependencyProperty.Register("Net3", typeof(string), typeof(SocketBaseViewModel), new PropertyMetadata(MarkDirty));
        public static readonly DependencyProperty Net4Property = DependencyProperty.Register("Net4", typeof(string), typeof(SocketBaseViewModel), new PropertyMetadata(MarkDirty));
        public static readonly DependencyProperty Net5Property = DependencyProperty.Register("Net5", typeof(string), typeof(SocketBaseViewModel), new PropertyMetadata(MarkDirty));
        public static readonly DependencyProperty Net6Property = DependencyProperty.Register("Net6", typeof(string), typeof(SocketBaseViewModel), new PropertyMetadata(MarkDirty));
        public static readonly DependencyProperty Net7Property = DependencyProperty.Register("Net7", typeof(string), typeof(SocketBaseViewModel), new PropertyMetadata(MarkDirty));
        public static readonly DependencyProperty Net8Property = DependencyProperty.Register("Net8", typeof(string), typeof(SocketBaseViewModel), new PropertyMetadata(MarkDirty));
        public static readonly DependencyProperty Net9Property = DependencyProperty.Register("Net9", typeof(string), typeof(SocketBaseViewModel), new PropertyMetadata(MarkDirty));

        public string Net3 { get { return (string)GetValue(Net3Property); } set { SetValue(Net3Property, value); } }
        public string Net4 { get { return (string)GetValue(Net4Property); } set { SetValue(Net4Property, value); } }
        public string Net5 { get { return (string)GetValue(Net5Property); } set { SetValue(Net5Property, value); } }
        public string Net6 { get { return (string)GetValue(Net6Property); } set { SetValue(Net6Property, value); } }
        public string Net7 { get { return (string)GetValue(Net7Property); } set { SetValue(Net7Property, value); } }
        public string Net8 { get { return (string)GetValue(Net8Property); } set { SetValue(Net8Property, value); } }
        public string Net9 { get { return (string)GetValue(Net9Property); } set { SetValue(Net9Property, value); } }

        public static readonly DependencyProperty Shared3Property = DependencyProperty.Register("Shared3", typeof(bool), typeof(SocketBaseViewModel), new PropertyMetadata(true, MarkDirty));
        public static readonly DependencyProperty Shared4Property = DependencyProperty.Register("Shared4", typeof(bool), typeof(SocketBaseViewModel), new PropertyMetadata(true, MarkDirty));
        public static readonly DependencyProperty Shared5Property = DependencyProperty.Register("Shared5", typeof(bool), typeof(SocketBaseViewModel), new PropertyMetadata(true, MarkDirty));
        public static readonly DependencyProperty Shared6Property = DependencyProperty.Register("Shared6", typeof(bool), typeof(SocketBaseViewModel), new PropertyMetadata(true, MarkDirty));
        public static readonly DependencyProperty Shared7Property = DependencyProperty.Register("Shared7", typeof(bool), typeof(SocketBaseViewModel), new PropertyMetadata(true, MarkDirty));
        public static readonly DependencyProperty Shared8Property = DependencyProperty.Register("Shared8", typeof(bool), typeof(SocketBaseViewModel), new PropertyMetadata(true, MarkDirty));
        public static readonly DependencyProperty Shared9Property = DependencyProperty.Register("Shared9", typeof(bool), typeof(SocketBaseViewModel), new PropertyMetadata(true, MarkDirty));

        public bool Shared3 { get { return (bool)GetValue(Shared3Property); } set { SetValue(Shared3Property, value); } }
        public bool Shared4 { get { return (bool)GetValue(Shared4Property); } set { SetValue(Shared4Property, value); } }
        public bool Shared5 { get { return (bool)GetValue(Shared5Property); } set { SetValue(Shared5Property, value); } }
        public bool Shared6 { get { return (bool)GetValue(Shared6Property); } set { SetValue(Shared6Property, value); } }
        public bool Shared7 { get { return (bool)GetValue(Shared7Property); } set { SetValue(Shared7Property, value); } }
        public bool Shared8 { get { return (bool)GetValue(Shared8Property); } set { SetValue(Shared8Property, value); } }
        public bool Shared9 { get { return (bool)GetValue(Shared9Property); } set { SetValue(Shared9Property, value); } }
    }
}
