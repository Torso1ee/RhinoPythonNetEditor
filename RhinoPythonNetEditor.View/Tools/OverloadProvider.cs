using CommunityToolkit.Mvvm.Messaging;
using ICSharpCode.AvalonEdit.CodeCompletion;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using RhinoPythonNetEditor.ViewModel.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RhinoPythonNetEditor.View.Tools
{
    public class OverloadProvider : IOverloadProvider
    {
        public OverloadProvider(SignatureHelp help, WeakReferenceMessenger messager)
        {
            Messager = messager;
            Help = help.Signatures.ToArray();
            SelectedIndex = 0;
            IsShowPanel = Count > 1;
        }

        private WeakReferenceMessenger Messager;
        public SignatureInformation[] Help { get; set; }

        private int _selectedIndex;

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                Messager.Send(new SetDocumentMessage(Help[_selectedIndex].Documentation?.ToString()));
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentHeader));
                OnPropertyChanged(nameof(CurrentContent));
                OnPropertyChanged(nameof(Text));
            }
        }


        public int Count => Help.Length;

        private bool isShowPanel;

        public bool IsShowPanel
        {
            get { return isShowPanel; }
            set { isShowPanel = value; OnPropertyChanged(); }
        }


        public string CurrentIndexText => null;

        public string Text => $"{_selectedIndex + 1}/{Count}";
        public object CurrentHeader => Help[_selectedIndex].Label;

        public object CurrentContent => Help[_selectedIndex].Documentation;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
