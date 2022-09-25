using RhinoPythonNetEditor.CustomControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RhinoPythonNetEditor.View.Dialogs
{
    /// <summary>
    /// DebugSetting.xaml 的交互逻辑
    /// </summary>
    public partial class DebugSetting : UserControl
    {
        public DebugSetting()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Parent is Dialog dialog) dialog.Close();
        }
    }
}
