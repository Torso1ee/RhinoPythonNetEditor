using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhinoPythonNetEditor.ViewModel.Messages
{
    public class ReplaceRequestMessage : RequestMessage<bool>
    {
        public string ReplaceText { get; set; }
        public bool IsAll { get; set; }
        public int Index { get; set; }
    }
}
