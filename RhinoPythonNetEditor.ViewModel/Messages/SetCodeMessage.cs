using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhinoPythonNetEditor.ViewModel.Messages
{
    public class SetCodeMessage : ValueChangedMessage<string>
    {
        public SetCodeMessage(string value) : base(value)
        {
        }
    }
}
