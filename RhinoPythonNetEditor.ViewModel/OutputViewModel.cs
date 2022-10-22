using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using RhinoPythonNetEditor.DataModels.Business;
using RhinoPythonNetEditor.ViewModel.Messages;

namespace RhinoPythonNetEditor.ViewModel
{
    public class OutputViewModel : ObservableRecipient
    {
        private WeakReferenceMessenger Messenger => Locator?.Messenger;

        private ViewModelLocator Locator { get; set; }
        public OutputViewModel(ViewModelLocator locator)
        {
            Locator = locator;
            Locator.ConfigureFinished += (s, e) =>
            {
                Messenger.Register<SyntaxHintChangedMessage>(this, Receive);
                Messenger.Register<SetDocumentMessage>(this, Receive);
            };
            IsActive = true;
        }

        public ObservableCollection<OutputResult> Results { get; set; } = new ObservableCollection<OutputResult>();

        public ObservableCollection<SyntaxInfo> SyntaxHints { get; set; } = new ObservableCollection<SyntaxInfo>();

        private string documentation;

        public string Documentation
        {
            get { return documentation; }
            set { SetProperty(ref documentation, value); }
        }


        void Receive(object recipient, SyntaxHintChangedMessage message)
        {
            SyntaxHints.Clear();
            foreach (var m in message.Value) SyntaxHints.Add(m);
        }

        void Receive(object recipient, SetDocumentMessage message)
        {
            Documentation = message.Value;
        }


    }
}
