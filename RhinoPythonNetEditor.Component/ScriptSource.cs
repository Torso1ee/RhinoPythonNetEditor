using GH_IO;
using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
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
        public ScriptSource(PythonNetScriptComponent comp)
        {
            Component = comp;
        }

        internal PythonNetScriptComponent Component { get; set; }
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
                for (int i = 0; i < count; i++)
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
            foreach (var str in References) writer.Write(str);
        }

        public bool IsEmpty => string.IsNullOrEmpty(PythonCode);

        public string GenerateCode(Guid id)
        {
            var template = Resources.Template;
            return "";
        }


        private static SortedDictionary<string, string> _keywords;
        public static string MungeParameterName(string name)
        {
            if (_keywords == null)
            {
                _keywords = new SortedDictionary<string, string>();
                foreach (string str2 in Utility.FixNewlines(Resources.CS_LanguageKeywords).Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                {
                    _keywords.Add(str2.ToUpperInvariant(), str2);
                }
            }
            return (!_keywords.ContainsKey(name.ToUpperInvariant()) ? name : string.Format("@{0}", name));
        }

        private string CodeBlock_PythonCode(Guid id)
        {
            var pythonFunc = new StringBuilder();
            pythonFunc.AppendLine($"dynamic module = Py.Import(\"{id}\");");
            pythonFunc.AppendLine("dynamic func = module.func;");
            pythonFunc.AppendLine($"func({CodeBlock_ParameterSignature()});");
            return pythonFunc.ToString();
        }
        private string CodeBlock_ParameterSignature()
        {
            List<string> list = new List<string>();
            int num = Component.Params.Input.Count - 1;
            for (int i = 0; i <= num; i++)
            {
                string item = MungeParameterName(GH_Convert.ToVariableName(Component.Params.Input[i].NickName));
                list.Add(item);
            }
            int num3 = Component.Params.Output.Count - 1;
            for (int j = Component.FirstOutputIndex; j <= num3; j++)
            {
                list.Add( MungeParameterName(GH_Convert.ToVariableName(Component.Params.Output[j].NickName)));
            }
            return string.Join(", ", list.ToArray());
        }

        private string CodeBlock_InputParameters()
        {
            string str;
            List<string> list = new List<string>();
            string str2 = string.Empty;
            if (Component.Params.Input.Count == 0)
            {
                str = str2;
            }
            else
            {
                var num = Component.Params.Input.Count;
                for (int i = 0; i < num; i++)
                {
                    Param_ScriptVariable variable = (Param_ScriptVariable)Component.Params.Input[i];
                    string typeName = "object";
                    if (variable.TypeHint != null)
                    {
                        typeName = variable.TypeHint.TypeName;
                    }
                    if (string.IsNullOrEmpty(typeName))
                    {
                        typeName = "object";
                    }
                    string item = string.Empty;
                    string str5 = MungeParameterName(GH_Convert.ToVariableName(variable.NickName));
                    switch (variable.Access)
                    {
                        case GH_ParamAccess.item:
                            item = string.Format("{1} {0}", str5, typeName);
                            break;

                        case GH_ParamAccess.list:
                            item = string.Format("List<{1}> {0}", str5, typeName);
                            break;

                        case GH_ParamAccess.tree:
                            item = string.Format("DataTree<{1}> {0}", str5, typeName);
                            break;

                        default:
                            break;
                    }
                    list.Add(item);
                }
                str2 = (list.Count != 0) ? string.Join(", ", list.ToArray()) : list[0];
                str = str2;
            }
            return str;
        }

        private string CodeBlock_OutputParameters()
        {
            string str;
            List<string> list = new List<string>();
            string str2 = string.Empty;
            if (Component.Params.Output.Count == Component.FirstOutputIndex)
            {
                str = str2;
            }
            else
            {
                int num = Component.Params.Output.Count - 1;
                for (int i = Component.FirstOutputIndex; i < num; i++)
                {
                    string str3 = MungeParameterName(GH_Convert.ToVariableName(Component.Params.Output[i].NickName));
                    list.Add(string.Format("ref object {0}", str3));
                }
                str2 = (list.Count != 0) ? string.Join(", ", list.ToArray()) : string.Empty;
                str = str2;
            }
            return str;
        }

    }
}
