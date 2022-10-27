using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhinoPythonNetEditor.ViewModel.Messages
{
    public class EditorEditMessage : ValueChangedMessage<EditBehaviors>
    {
        public EditorEditMessage(EditBehaviors value) : base(value)
        {
        }
    }

    public enum EditBehaviors
    {
        Copy,
        Paste,
        Cut,
        SelectAll,
        Undo,
        Redo
    }
}
