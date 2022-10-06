using ICSharpCode.AvalonEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RhinoPythonNetEditor.ViewModel
{
    public class TextEditorViewModel : ObservableRecipient
    {
        private TextDocument document = new TextDocument();

        public TextDocument Document
        {
            get { return document; }
            set { SetProperty(ref document, value); }
        }

        public ViewModelLocator Locator { get; set; }

        public TextEditorViewModel(ViewModelLocator locator)
        {
            Locator = locator;
            IsActive = true;
        }

        public void SetCode(string text)
        {
            Document.Text = text;
        }

        //public bool CheckCode()
        //{

        //}

    }
}
