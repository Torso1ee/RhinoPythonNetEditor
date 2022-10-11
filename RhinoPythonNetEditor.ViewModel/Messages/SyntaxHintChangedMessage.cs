using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging.Messages;
using RhinoPythonNetEditor.DataModels.Business;

namespace RhinoPythonNetEditor.ViewModel.Messages
{
    public class SyntaxHintChangedMessage : ValueChangedMessage<List<SyntaxInfo>>
    {
        public SyntaxHintChangedMessage(List<SyntaxInfo> value) : base(value)
        {
        }
    }
}
