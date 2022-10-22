using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using RhinoPythonNetEditor.Resources;

namespace RhinoPythonNetEditor.Styling.Converters
{
    public class KindToGeometryConverter : IValueConverter
    {
        private static Geometries geometries;
        public static Geometries Geometries
        {
            get
            {
                if (geometries == null)
                {
                    geometries = new Geometries();
                    geometries.InitializeComponent();
                }
                return geometries;
            }
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var kind = value.ToString();
            switch (kind)
            {
                case "Class":
                    return Geometries["ClassGeometry"];
                case "Function":
                    return Geometries["FunctionGeometry"];
                case "Keyword":
                    return Geometries["KeywordGeometry"];
                case "Variable":
                case "Reference":
                case "TypeParameter":
                    return Geometries["VariableGeometry"];
                case "Module":
                    return Geometries["ModuleGeometry"];
                case "Property":
                    return Geometries["PropertyGeometry"];
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
