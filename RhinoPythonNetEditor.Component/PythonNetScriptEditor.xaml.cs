using CommunityToolkit.Mvvm.Messaging;
using RhinoPythonNetEditor.View.Pages;
using RhinoPythonNetEditor.ViewModel;
using RhinoPythonNetEditor.ViewModel.Messages;
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
using System.Windows.Shapes;

namespace RhinoPythonNetEditor.Component
{
    /// <summary>
    /// PythonNetScriptEditor.xaml 的交互逻辑
    /// </summary>
    public partial class PythonNetScriptEditor : Window
    {
        private ViewModelLocator Locator { get; set; }
        public PythonNetScriptEditor(PythonNetScriptComponent comp)
        {
            InitializeComponent();
            Owner = comp;
        }

        internal PythonNetScriptComponent Owner { get; set; }
   
        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (WindowState == WindowState.Minimized) WindowState = WindowState.Normal;
        }

  
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Locator = DataContext as ViewModelLocator;
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue)
            {
                var code = Locator.Messenger.Send<CodeRequestMessage>();
                Owner.ScriptSource.PythonCode = code;
            }
        }
    }
}
