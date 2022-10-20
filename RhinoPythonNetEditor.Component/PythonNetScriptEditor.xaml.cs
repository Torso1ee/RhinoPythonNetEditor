using CommunityToolkit.Mvvm.Messaging;
using RhinoPythonNetEditor.Managers;
using RhinoPythonNetEditor.View.Pages;
using RhinoPythonNetEditor.ViewModel;
using RhinoPythonNetEditor.ViewModel.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
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


        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Locator = DataContext as ViewModelLocator;
            var helper = new WindowInteropHelper(this);
            IntPtr windowHandle = helper.Handle; //Get the handle of this window
            IntPtr hmenu = GetSystemMenu(windowHandle, 0);
            int cnt = GetMenuItemCount(hmenu);
            for (int i = cnt - 1; i >= 0; i--)
            {
                RemoveMenu(hmenu, i, MF_DISABLED | MF_BYPOSITION);
            }
        }


        [DllImport("user32.dll", EntryPoint = "GetSystemMenu")]
        private static extern IntPtr GetSystemMenu(IntPtr hwnd, int revert);

        [DllImport("user32.dll", EntryPoint = "GetMenuItemCount")]
        private static extern int GetMenuItemCount(IntPtr hmenu);

        [DllImport("user32.dll", EntryPoint = "RemoveMenu")]
        private static extern int RemoveMenu(IntPtr hmenu, int npos, int wflags);

        [DllImport("user32.dll", EntryPoint = "DrawMenuBar")]
        private static extern int DrawMenuBar(IntPtr hwnd);

        private const int MF_BYPOSITION = 0x0400;
        private const int MF_DISABLED = 0x0002;
    }
}
