using RhinoPythonNetEditor.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace RhinoPythonNetEditor.Styling.Converters
{
    public class KindToColorConverter : IValueConverter
    {
        public static Geometries Geometries => KindToGeometryConverter.Geometries;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var kind = value.ToString();
            switch (kind)
            {
                case "Class":
                    return Geometries["IconColor1"];
                case "Function":
                    return Geometries["IconColor2"];
                case "Keyword":
                    return Geometries["IconColor3"];
                case "Variable":
                case "Reference":
                case "TypeParameter":
                    return Geometries["IconColor4"];
                case "Module":
                case "Property":
                    return Geometries["IconColor5"];
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
