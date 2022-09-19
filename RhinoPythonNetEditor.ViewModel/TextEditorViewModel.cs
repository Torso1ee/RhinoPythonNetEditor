using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using RhinoPythonNetEditor.Managers;
using RhinoPythonNetEditor.ViewModel.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RhinoPythonNetEditor.ViewModel
{
    public class TextEditorViewModel : ObservableRecipient, IRecipient<BreakPointValueChangedMessage>,IRecipient<AllBreakPointInformationsRequestMessage>
    {

        internal List<int> BreakPointIndices { get; set; } = new List<int>();
        public TextEditorViewModel()
        {
            IsActive = true;
        }
        public void Receive(BreakPointValueChangedMessage message)
        {
            if (message.Value) BreakPointIndices.Add(message.Line);
            else BreakPointIndices.Remove(message.Line);
        }

        public void Receive(AllBreakPointInformationsRequestMessage message)
        {
            message.Reply(BreakPointIndices);
        }
    }
}
