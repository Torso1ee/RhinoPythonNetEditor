using GH_IO;
using GH_IO.Serialization;
using Grasshopper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhinoPythonNetEditor.Component
{
    public class ScriptSource : GH_ISerializable
    {
        internal List<string> References { get; } = new List<string>();

        public string PythonCode { get; set; }

        public bool Write(GH_IWriter writer)
        {
            if (!string.IsNullOrEmpty(PythonCode)) writer.SetString("PythonCode", PythonCode);
            writer.SetInt32("ReferenceCount", this.References.Count);
            var count = References.Count;
            for (int i = 0; i < count; i++)
            {
                writer.SetString("Reference", i, References[i]);
            }
            return true;
        }

        public bool Read(GH_IReader reader)
        {
            PythonCode = !reader.ItemExists("PythonCode") ? String.Empty : Utility.FixNewlines(reader.GetString("PythonCode"));
            References.Clear();
            if (reader.ItemExists("ReferenceCount"))
            {
                var count = reader.GetInt32("ReferenceCount");
                for(int i =0;i < count; i++)
                {
                    References.Add(reader.GetString("Reference", i));
                }
              
            }
            return true;
        }

        public void WriteHashData(BinaryWriter writer)
        {
            writer.Write("Script");
            if (!string.IsNullOrEmpty(PythonCode)) writer.Write(PythonCode);
            writer.Write("References");
            writer.Write(References.Count);
            foreach(var str in References)writer.Write(str);
        }

        public bool IsEmpty => string.IsNullOrEmpty(PythonCode);
    }
}
