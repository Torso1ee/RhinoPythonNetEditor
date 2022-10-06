using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhinoPythonNetEditor.Interface
{
    public interface IScriptComponent
    {
        void SetSource(string code);

        void CloseEditor();
    }
}
