using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Python.Runtime;

namespace RhinoPythonNetEditor.View.Tools
{
    public class SyntaxHelper
    {
        private static string path;
        public static string AssemblyPath
        {
            get
            {
                if (string.IsNullOrEmpty(path))
                {
                    path = Path.GetDirectoryName(typeof(SyntaxHelper).Assembly.Location);
                }
                return path;
            }
        }
        public static string SyntaxCheck(string text)
        {
            var result = "";
            using (Py.GIL())
            {
                dynamic sys = Py.Import("sys");
                sys.path.append(AssemblyPath);
                dynamic sc = Py.Import("syntaxcheck");
                result = sc.syntax_check(text);
            }
            return result;
        }
    }
}
