using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using RhinoPythonNetEditor.CustomControls;
using RhinoPythonNetEditor.ViewModel.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
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
        public DebugSetting(IEnumerable<string> refs)
        {
            InitializeComponent();
            References = new ObservableCollection<string>(refs);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Parent is Dialog dialog) dialog.Close();
        }

        public static readonly DependencyProperty ReferencesProperty = DependencyProperty.Register("References", typeof(ObservableCollection<string>), typeof(DebugSetting));


        public ObservableCollection<string> References
        {
            get => (ObservableCollection<string>)GetValue(ReferencesProperty);
            set => SetValue(ReferencesProperty, value);
        }

        public ICommand AddReference => new RelayCommand(() =>
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Please select assembly file",
                Filter = "Assembly File(*.dll)|*dll"
            };
            if (dialog.ShowDialog() == true)
            {
                if (!References.Contains(dialog.FileName))
                {
                    References.Add(dialog.FileName);
                }
            }
        });

        public ICommand RemoveReference => new RelayCommand<string>(str =>
        {
            if (References.Contains(str))
            {
                References.Remove(str);
            }
        });

    }
}
