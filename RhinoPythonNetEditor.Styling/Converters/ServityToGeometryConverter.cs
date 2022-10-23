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
    internal class ServityToGeometryConverter : IValueConverter
    {
        public static Geometries Geometries => KindToGeometryConverter.Geometries;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var kind = value.ToString();
            switch (kind)
            {
                case "Error":
                    return Geometries["CodeErrorGeometry"];
                case "Warning":
                    return Geometries["CodeWarningGeometry"];
                case "Information":
                case "Hint":
                    return Geometries["CodeInfoGeometry"];
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
