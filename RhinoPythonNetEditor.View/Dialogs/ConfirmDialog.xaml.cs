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
    /// ConfirmDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ConfirmDialog : UserControl
    {
        public ConfirmDialog(string title, string message)
        {
            InitializeComponent();
            tb.Text = message;
            lb.Content = title;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Parent is Dialog dialog)
            {
                dialog.Result = false;
                dialog.Close();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (Parent is Dialog dialog)
            {
                dialog.Result = true;
                dialog.Close();
            }
        }
    }
}
