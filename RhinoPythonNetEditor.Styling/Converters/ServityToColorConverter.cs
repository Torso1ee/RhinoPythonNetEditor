using RhinoPythonNetEditor.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace RhinoPythonNetEditor.Styling.Converters
{

    public class ServityToColorConverter : IValueConverter
    {
        public static Geometries Geometries => KindToGeometryConverter.Geometries;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var kind = value.ToString();
            switch (kind)
            {
                case "Error":
                    return Geometries["IconColor8"];
                case "Warning":
                    return Geometries["IconColor6"];
                case "Information":
                case "Hint":
                    return Geometries["IconColor7"];
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
