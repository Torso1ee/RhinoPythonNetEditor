using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using RhinoPythonNetEditor.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using RhinoPythonNetEditor.ViewModel.Messages;
using System.Diagnostics;
using System.Windows.Documents;
using System.IO;
using RhinoPythonNetEditor.Interface;
using Microsoft.Win32;


namespace RhinoPythonNetEditor.ViewModel
{

    public class MenuBarViewModel : ObservableRecipient
    {
        public MenuBarViewModel(ViewModelLocator locator)
        {
            IsActive = true;
            Locator = locator;
        }

        private WeakReferenceMessenger Messenger => Locator?.Messenger;

        private ViewModelLocator Locator { get; set; }

        public ICommand OpenWebsite => new RelayCommand<string>(uri =>
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start {uri}"));
        });

        public ICommand Run => new RelayCommand(() =>
        {
            if (Locator.ComponentHost != null && Locator.ComponentHost is IScriptComponent sc) sc.SetSource(Locator.TextEditorViewModel.Document.Text);
            Locator.ComponentHost.ExpireSolution(true);
        });

        public ICommand Confirm => new RelayCommand(() =>
        {
            if (Locator.ComponentHost != null && Locator.ComponentHost is IScriptComponent sc)
            {
                sc.SetSource(Locator.TextEditorViewModel.Document.Text);
                sc.CloseEditor();
            }
            Locator.ComponentHost.ExpireSolution(true);

        });

        public ICommand ImportFrom => new RelayCommand(() =>
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Please select file",
                Filter = "Python Code File(*.py)|*py"
            };
            if (dialog.ShowDialog() == true)
            {
                var text = File.ReadAllText(dialog.FileName);
                Messenger.Send(new SetCodeMessage(text));
            }
        });

        public ICommand ExportAs => new RelayCommand(() =>
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Title = "Save As",
                Filter = "Python Code File(*.py)|*py"
            };
            if (dialog.ShowDialog() == true)
            {
                File.WriteAllText(dialog.FileName + ".py", Locator.TextEditorViewModel.Document.Text);
            }
        });

        public ICommand Edit => new RelayCommand<string>(txt =>
        {
            if (Enum.TryParse(txt, out EditBehaviors behavior))
                Messenger.Send(new EditorEditMessage(behavior));

        });

    }
}
