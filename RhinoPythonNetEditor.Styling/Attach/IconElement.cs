using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace RhinoPythonNetEditor.Styling.Attach
{
    public class IconElement
    {

        public static readonly DependencyProperty GeometryProperty = DependencyProperty.RegisterAttached("Geometry", typeof(Geometry), typeof(IconElement), new PropertyMetadata(null));

        public static Geometry GetGeometry(DependencyObject dependencyObject) => dependencyObject.GetValue(GeometryProperty) as Geometry;

        public static void SetGeometry(DependencyObject dependencyObject, Geometry value) => dependencyObject.SetValue(GeometryProperty, value);

        public static readonly DependencyProperty HeightProperty = DependencyProperty.RegisterAttached("Height", typeof(double), typeof(IconElement), new PropertyMetadata(double.NaN));

        public static double GetHeight(DependencyObject dependencyObject) => (double)dependencyObject.GetValue(HeightProperty);

        public static void SetHeight(DependencyObject dependencyObject, double value) => dependencyObject.SetValue(HeightProperty, value);

        public static readonly DependencyProperty WidthProperty = DependencyProperty.RegisterAttached("Width", typeof(double), typeof(IconElement), new PropertyMetadata(double.NaN));

        public static double GetWidth(DependencyObject dependencyObject) => (double)dependencyObject.GetValue(WidthProperty);

        public static void SetWidth(DependencyObject dependencyObject, double value) => dependencyObject.SetValue(WidthProperty, value);

        public static readonly DependencyProperty AttachedKeyProperty = DependencyProperty.RegisterAttached("AttachedKey", typeof(string), typeof(IconElement), new PropertyMetadata(""));

        public static string GetAttachedKey(DependencyObject dependencyObject) => (string)dependencyObject.GetValue(AttachedKeyProperty);

        public static void SetAttachedKey(DependencyObject dependencyObject, string value) => dependencyObject.SetValue(AttachedKeyProperty, value);

        public static readonly DependencyProperty ActiveBackgroundProperty = DependencyProperty.RegisterAttached("ActiveBackground", typeof(SolidColorBrush), typeof(IconElement), new PropertyMetadata(Brushes.White));

        public static SolidColorBrush GetActiveBackground(DependencyObject dependencyObject) => (SolidColorBrush)dependencyObject.GetValue(ActiveBackgroundProperty);

        public static void SetActiveBackground(DependencyObject dependencyObject, SolidColorBrush value) => dependencyObject.SetValue(ActiveBackgroundProperty, value);


    }
}
