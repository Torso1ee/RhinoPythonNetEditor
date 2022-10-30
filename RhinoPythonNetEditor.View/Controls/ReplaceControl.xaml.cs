using CommunityToolkit.Mvvm.Messaging;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RhinoPythonNetEditor.View.Controls
{
    /// <summary>
    /// ReplaceControl.xaml 的交互逻辑
    /// </summary>
    public partial class ReplaceControl : UserControl
    {
        private WeakReferenceMessenger Messenger;
        public ReplaceControl()
        {
            InitializeComponent();
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue && DataContext is TextEditorViewModel vm) vm.SearchInternal(input);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (DataContext is TextEditorViewModel vm) vm.SearchInternal(input);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (Messenger == null && DataContext is TextEditorViewModel vm)
            {
                Messenger = vm.Locator.Messenger;
                Messenger.Register<NotifySearchMessage>(this, (r, m) =>
                {
                    if (DataContext is TextEditorViewModel tm) tm.SearchInternal(input);
                });
            }
        }
    }
}
