// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System.CodeDom.Compiler;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Microsoft.Gadgeteer.HardwareTemplateWizard
{
    public class SafeCheck : DependencyObject
    {
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(SafeCheck), new PropertyMetadata(OnIsEnabledChanged));
        public static readonly DependencyProperty InvalidDecorationProperty = DependencyProperty.RegisterAttached("InvalidDecoration", typeof(TextDecorationCollection), typeof(SafeCheck), new PropertyMetadata(GetDefaultInvalidDecorations()));

        public static bool GetIsEnabled(TextBox textBox)
        {
            return (bool)textBox.GetValue(IsEnabledProperty);
        }
        public static void SetIsEnabled(TextBox textBox, bool value)
        {
            textBox.SetValue(IsEnabledProperty, value);
        }

        public static TextDecorationCollection GetInvalidDecoration(TextBox textBox)
        {
            return (TextDecorationCollection)textBox.GetValue(InvalidDecorationProperty);
        }
        public static void SetInvalidDecoration(TextBox textBox, TextDecorationCollection value)
        {
            textBox.SetValue(InvalidDecorationProperty, value);
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBox box = d as TextBox;
            if (box != null)
            {
                if ((bool)e.NewValue == true)
                    box.TextChanged += InvalidateTextBox;
                else
                    box.TextChanged -= InvalidateTextBox;
            }
        }

        private static void InvalidateTextBox(object sender, TextChangedEventArgs e)
        {
            TextBox box = sender as TextBox;
            if (box != null)
            {
                if (IsSafe(box.Text))
                    box.TextDecorations = EmptyDecorations;
                else
                    box.TextDecorations = GetInvalidDecoration(box);
            }
        }

        private static readonly TextDecorationCollection EmptyDecorations = new TextDecorationCollection(0);
        private static TextDecorationCollection GetDefaultInvalidDecorations()
        {
            DrawingGroup drawingGroup = new DrawingGroup();

            using (DrawingContext drawingContext = drawingGroup.Open())
            {
                Pen pen = new Pen(Brushes.Red, 0.33);

                drawingContext.DrawLine(pen, new Point(0.0, 0.0), new Point(0.5, 1.0));
                drawingContext.DrawLine(pen, new Point(0.5, 1.0), new Point(1.0, 0.0));
            }

            DrawingBrush brush = new DrawingBrush(drawingGroup);
            brush.TileMode = TileMode.Tile;
            brush.Viewport = new Rect(0, 0, 3, 3);
            brush.ViewportUnits = BrushMappingMode.Absolute;

            TextDecoration textDecoration = new TextDecoration(TextDecorationLocation.Underline, new Pen(brush, 3), 0, TextDecorationUnit.FontRecommended, TextDecorationUnit.Pixel);

            TextDecorationCollection decorationCollection = new TextDecorationCollection();
            decorationCollection.Add(textDecoration);
            decorationCollection.Freeze();

            return decorationCollection;
        }

        private static readonly CodeDomProvider CSharpCodeProvider = new CSharp.CSharpCodeProvider();
        public static bool IsSafe(string identifier)
        {
            // Beware that the language independent check allows identifiers reserved as keywords
            // return CodeGenerator.IsValidLanguageIndependentIdentifier(identifier);
            
            return CSharpCodeProvider.IsValidIdentifier(identifier);
        }
    }
}
