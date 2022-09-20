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
    public class TextEditorViewModel : ObservableRecipient
    {

        public TextEditorViewModel()
        {
            IsActive = true;
        }
      
    }
}
