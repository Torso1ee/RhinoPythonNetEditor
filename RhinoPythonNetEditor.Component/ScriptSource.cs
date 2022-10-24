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

        internal Guid Id { get; set; }
        internal PythonNetScriptComponent Component { get; set; }
        internal List<string> References { get; } = new List<string>();

        public string PythonCode { get; set; } = "";

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
            GeneratePyFile(id);
            string template = Utility.FixNewlines(Resources.Template);
            template = template.Replace("{tadokorokouji}", CodeBlock_InputParameterAssignment());
            template = template.Replace("{miuradaisenbai}", CodeBlock_OutputParameterDeclarations());
            template = template.Replace("{bokusyu}", CodeBlock_PythonCode(id));
            template = template.Replace("{1145141919810}", CodeBlock_ParameterAssignment());
            //File.WriteAllText(PythonNetScriptComponent.CompiledPath + @"\test.cs", template);
            return template;
        }


        private void GeneratePyFile(Guid id)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"from System import *");
            sb.AppendLine($"def func({string.Join(",", new[] { CodeBlock_PyParameterSignature(), CodeBlock_PyReturnSignature() }.Where(s => !string.IsNullOrEmpty(s)))}):");
            var lines = PythonCode.Split('\n');
            var code = "";
            foreach (var l in lines) code += ("\t" + l);
            sb.AppendLine(code);
            sb.AppendLine("\t" + $"return [{CodeBlock_PyReturnSignature()}]");
            File.WriteAllText(PythonNetScriptComponent.CompiledPath + $@"\{id}.py", sb.ToString());
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
            var idName = id.ToString().Replace("-", "");
            pythonFunc.AppendLine($"dynamic module = Py.Import(\"{id}\");");
            pythonFunc.AppendLine("dynamic func = module.func;");
            pythonFunc.AppendLine($"var result{idName} = func({string.Join(",", new[] { CodeBlock_PyParameterSignature(), CodeBlock_PyReturnSignature() }.Where(s => !string.IsNullOrEmpty(s)))});");
            List<string> list = new List<string>();
            int num = Component.Params.Output.Count - 1;
            int index = 0;
            for (int i = Component.FirstOutputIndex; i <= num; i++)
            {
                string item = MungeParameterName(GH_Convert.ToVariableName(Component.Params.Output[i].NickName));
                pythonFunc.AppendLine($"{item} = result{idName}[{index++}].As<object>();");
            }
            return pythonFunc.ToString();
        }

        private string CodeBlock_PyParameterSignature()
        {
            List<string> list = new List<string>();
            int num = Component.Params.Input.Count - 1;
            for (int i = 0; i <= num; i++)
            {
                string item = MungeParameterName(GH_Convert.ToVariableName(Component.Params.Input[i].NickName));
                list.Add(item);
            }
            return string.Join(", ", list.ToArray());
        }

        private string CodeBlock_PyReturnSignature()
        {
            List<string> list = new List<string>();
            int num = Component.Params.Output.Count - 1;
            for (int i = Component.FirstOutputIndex; i <= num; i++)
            {
                string item = MungeParameterName(GH_Convert.ToVariableName(Component.Params.Output[i].NickName));
                list.Add(item);
            }
            return string.Join(", ", list.ToArray());
        }


        private string CodeBlock_ParameterAssignment()
        {
            StringBuilder builder = new StringBuilder();
            int num = Component.Params.Output.Count - 1;
            for (int i = Component.FirstOutputIndex; i <= num; i++)
            {
                string newValue = MungeParameterName(GH_Convert.ToVariableName(Component.Params.Output[i].NickName));
                string str2 = Utility.FixNewlines(Resources.CS_OutputParam_Template).Replace("{7F0A2DDA-D43D}", newValue).Replace("{C9474A41-931A}", i.ToString());
                builder.Append(str2);
            }
            return builder.ToString();
        }

        private string CodeBlock_InputParameterAssignment()
        {
            string str;
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < Component.Params.Input.Count; i++)
            {
                Param_ScriptVariable variable1 = (Param_ScriptVariable)Component.Params.Input[i];
                string newValue = MungeParameterName(GH_Convert.ToVariableName(variable1.NickName));
                IGH_TypeHint typeHint = variable1.TypeHint;
                string typeName = "object";
                if (typeHint != null)
                {
                    typeName = typeHint.TypeName;
                }
                string text = string.Empty;
                switch (variable1.Access)
                {
                    case GH_ParamAccess.item:
                        text = Resources.CS_InputParamItem_Template;
                        break;

                    case GH_ParamAccess.list:
                        text = Resources.CS_InputParamList_Template;
                        break;

                    case GH_ParamAccess.tree:
                        text = Resources.CS_InputParamTree_Template;
                        break;

                    default:
                        Tracing.Assert(new Guid("{8010B35A-3F61-40ca-BB0A-8153692FC78D}"), "Invalid Parameter Access flag. Source code generation failed.");
                        return string.Empty;
                }
                builder.Append(Utility.FixNewlines(text).Replace("{7F0A2DDA-D43D}", newValue).Replace("{C62D29C7-F213}", typeName).Replace("{C9474A41-931A}", i.ToString()) + Environment.NewLine);
            }
            str = builder.ToString();
            return str;
        }

        private string CodeBlock_OutputParameterDeclarations()
        {
            StringBuilder builder = new StringBuilder();
            int num = Component.Params.Output.Count - 1;
            for (int i = Component.FirstOutputIndex; i <= num; i++)
            {
                string str = MungeParameterName(GH_Convert.ToVariableName(Component.Params.Output[i].NickName));
                builder.AppendLine(string.Format("  object {0} = null;", str));
            }
            return builder.ToString();
        }



    }
}
