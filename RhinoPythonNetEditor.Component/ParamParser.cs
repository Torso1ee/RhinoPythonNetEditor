using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using GH_IO.Serialization;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel;
using System.Reflection;

namespace RhinoPythonNetEditor.Component
{
    public class ParamParser
    {
        public Dictionary<string, GH_Structure<IGH_Goo>> ParamDict = new Dictionary<string, GH_Structure<IGH_Goo>>();
        public ParamParser(string path)
        {
            var buffer = File.ReadAllBytes(path);
            var chunk = new GH_LooseChunk("Grasshopper Data");
            chunk.Deserialize_Binary(buffer);
            foreach (var c in chunk.Chunks)
            {
                var reader = (c as GH_Chunk).FindChunk("Data");
                var tree = new GH_Structure<IGH_Goo>();
                tree.Read(reader);
                ParamDict[c.Name] = tree;
            }
        }

       
    }
}