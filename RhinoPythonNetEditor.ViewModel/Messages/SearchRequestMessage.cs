using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhinoPythonNetEditor.ViewModel.Messages
{
    public class SearchRequestMessage : RequestMessage<(bool, int, int)>
    {
        public string SearchText { get; set; }

        public bool SelectionOnly { get; set; }

        public bool ClarifyCase { get; set; }

        public bool AllMatch { get; set; }

        public bool UseRe { get; set; }

    }
}
