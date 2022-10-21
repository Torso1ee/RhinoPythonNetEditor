using ICSharpCode.AvalonEdit.CodeCompletion;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
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
        public OverloadProvider(SignatureHelp help)
        {
            Help = help.Signatures.ToArray();
            SelectedIndex = 0;
        }

        public SignatureInformation[] Help { get; set; }

        private int _selectedIndex;

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentHeader));
                OnPropertyChanged(nameof(CurrentContent));
            }
        }

        public int Count => Help.Length;

        public string CurrentIndexText => null;

        public object CurrentHeader => Help[_selectedIndex].Label;

        public object CurrentContent => Help[_selectedIndex].Documentation;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
