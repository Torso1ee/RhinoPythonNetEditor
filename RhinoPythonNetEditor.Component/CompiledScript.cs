using Grasshopper.Kernel;
using System;
using System.Runtime.CompilerServices;

namespace RhinoPythonNetEditor.Component
{

    internal class CompiledScript
    {

        public CompiledScript(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            Type = type;
            object obj = Activator.CreateInstance(type);
            if (obj != null)
            {
                Instance = obj as IGH_ScriptInstance;
            }
        }

        public Type Type { get; set; }

        public IGH_ScriptInstance Instance { get; set; }
    }

}
