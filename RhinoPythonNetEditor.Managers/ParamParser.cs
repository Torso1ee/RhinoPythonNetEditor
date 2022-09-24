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
using GH_IO.Types;
using System.Management.Automation.Language;
using Grasshopper;

namespace RhinoPythonNetEditor.Managers
{
    public class ParamParser
    {
        public Dictionary<string, object> ParamDict = new Dictionary<string, object>();

        public int Count { get; set; }

        public ParamParser(string path)
        {
            Parse(path);
        }

        private void Parse(string path)
        {
            var buffer = File.ReadAllBytes(path);
            var chunk = new GH_LooseChunk("Grasshopper Data");
            chunk.Deserialize_Binary(buffer);
            Count = chunk.GetInt32("Count");
            PreReadChunk(chunk, Count);
            for (int i = 0; i < Count; i++)
            {
                ReadChunk(chunk, i);
            }
        }

        private void PreReadChunk(GH_LooseChunk chunk, int i)
        {
            var ck = chunk.FindChunk("param", i);
            var reader = ck.FindChunk("Data");
            var access = ck.GetString("access");
            var tree = new GH_Structure<IGH_Goo>();
            var name = ck.GetString("name");
            tree.Read(reader);
        }

        private void ReadChunk(GH_LooseChunk chunk, int i)
        {
            var ck = chunk.FindChunk("param", i);
            var reader = ck.FindChunk("Data");
            var access = ck.GetString("access");
            var tree = new GH_Structure<IGH_Goo>();
            var name = ck.GetString("name");
            tree.Read(reader);
            if (access == "Item")
            {
                var item = tree.get_FirstItem(false);
                if (item == null) ParamDict[name] = null;
                else
                {
                    dynamic d = item;
                    ParamDict[name] = d.Value;
                }
            }
            else if (access == "List")
            {
                var lst = tree.Branches[0].Select(v =>
                {
                    if (v == null) return null;
                    else
                    {
                        dynamic d = v;
                        return d.Value;
                    }
                }
                ).ToList();
                ParamDict[name] = lst;
            }
            else if (access == "Tree")
            {
                var dt = new DataTree<dynamic>();
                var count = tree.Branches.Count;
                for (int j = 0; j < count; j++)
                {
                    dt.AddRange(tree.Branches[j].Select(v =>
                    {
                        if (v == null) return null;
                        else
                        {
                            dynamic d = v;
                            return d.Value;
                        }
                    }
                ), tree.Paths[j]);
                }
                ParamDict[name] = dt;
            }
        }
    }
}