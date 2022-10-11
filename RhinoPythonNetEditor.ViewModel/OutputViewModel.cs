using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using RhinoPythonNetEditor.DataModels.Business;

namespace RhinoPythonNetEditor.ViewModel
{
    public class OutputViewModel:ObservableRecipient
    {
        public ViewModelLocator Locator { get; set; }
        public OutputViewModel(ViewModelLocator locator)
        {
            Locator = locator;
            IsActive = true;
        }

        public ObservableCollection<OutputResult> Results { get; set; } = new ObservableCollection<OutputResult>();
    }
}
