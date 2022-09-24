using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Xml;
using RhinoPythonNetEditor.View.Controls;
using RhinoPythonNetEditor.View.Tools;
using CommunityToolkit.Mvvm.Messaging;
using RhinoPythonNetEditor.ViewModel.Messages;
using RhinoPythonNetEditor.ViewModel;

namespace RhinoPythonNetEditor.View.Pages
{
    /// <summary>
    /// CodeEditor.xaml 的交互逻辑
    /// </summary>
    public partial class CodeEditor : UserControl
    {
        private Window window;
        WeakReferenceMessenger messenger;

        public CodeEditor()
        {
            InitializeComponent();
            var resource = FindResource("windowProxy");
        }


        public static readonly DependencyProperty HostInRhinoProperty = DependencyProperty.Register("HostInRhino", typeof(bool), typeof(TitleBar), new PropertyMetadata(false));

        public bool HostInRhino
        {
            get { return (bool)GetValue(HostInRhinoProperty); }
            set { SetValue(HostInRhinoProperty, value); }
        }
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(500);
            if (window == null) window = (FindResource("windowProxy") as BindingProxy).Data as Window;
            if (messenger == null)
            {
                messenger = (DataContext as ViewModelLocator).Messenger;
                messenger.Register<MessageDialogRequestMessage>(this, async (r, m) =>
                {
                    var messageBox = new MessageDialog(m.Title, m.Message);
                    await Dialog.Show(window, messageBox).WaitingForClosed();
                    m.Reply(true);
                });
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            window?.DragMove();
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            window?.Hide();
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void WindowStateChangeCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void WindowStateChangeCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (window != null) window.WindowState = (WindowState)e.Parameter;
        }
    }
}
