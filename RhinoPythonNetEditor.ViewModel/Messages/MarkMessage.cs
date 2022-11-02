using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhinoPythonNetEditor.ViewModel.Messages
{
    public class MarkMessage : ValueChangedMessage<MarkBehaviors>
    {
        public MarkMessage(MarkBehaviors value) : base(value)
        {
        }
    }

    public enum MarkBehaviors
    {
        Mark,
        Unmark,
        UnmarkAll
    }
}
