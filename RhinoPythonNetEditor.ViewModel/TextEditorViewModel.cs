using ICSharpCode.AvalonEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RhinoPythonNetEditor.ViewModel.Messages;
using CommunityToolkit.Mvvm.Messaging;
using RhinoPythonNetEditor.Interface;

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

        public async Task<bool> CheckCode()
        {
            if (Locator.ComponentHost is IScriptComponent sc)
            {
                if (sc.GetCode() != Document.Text)
                {
                    var result = await Locator.Messenger.Send(new ConfirmDialogRequestMessage { Message = $"代码发生变更，是否不应用就退出。", Title = "警告" });
                    return (bool)result;
                }
            }
            return true;
        }

    }
}
