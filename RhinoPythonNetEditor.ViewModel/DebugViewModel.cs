using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json;
using Rhino.Geometry;
using RhinoPythonNetEditor.Managers;
using RhinoPythonNetEditor.ViewModel.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Shapes;

namespace RhinoPythonNetEditor.ViewModel
{
    public class DebugViewModel : ObservableRecipient
    {
        private WeakReferenceMessenger Messenger => Locator?.Messenger;
        private ViewModelLocator Locator { get; set; }
        public DebugViewModel(ViewModelLocator locator)
        {
            Locator = locator;
            Locator.ConfigureFinished += (s, e) => Messenger.Register<AllBreakPointInformationsMessage>(this, Receive);
            CurrentDir = Directory.GetParent(typeof(DebugViewModel).Assembly.Location).ToString();
            IsActive = true;
        }

        private string PythonPath { get; set; } = @"D:\Anaconda\envs\PythonNet\python.exe";


        private bool isDebuging;

        public bool IsDebuging
        {
            get { return isDebuging; }
            set
            {
                SetProperty(ref isDebuging, value);
            }
        }
        private string CurrentDir { get; set; }

        public ICommand StartDebug => new RelayCommand(() => StartDebugCore(), () => !IsDebuging);

        private bool stopped;

        public bool Stopped
        {
            get { return stopped; }
            set { SetProperty(ref stopped, value); }
        }
        private bool configDone = false;

        public bool ConfigDone
        {
            get { return configDone; }
            set { SetProperty(ref configDone, value); }
        }

        private bool restart;

        public bool Restarting
        {
            get { return restart; }
            set { SetProperty(ref restart, value); }
        }

        public int LineOffset { get; set; }

        public ICommand Stop => new RelayCommand(() => debugManager?.Stop());
        public ICommand Continue => new RelayCommand(() => { debugManager?.Continue(); ConfigDone = false; });
        public ICommand Next => new RelayCommand(() => debugManager?.Next());

        public ICommand StepIn => new RelayCommand(() => debugManager?.StepIn());
        public ICommand StepOut => new RelayCommand(() => debugManager?.StepOut());

        public ICommand Restart => new RelayCommand(() => { Restarting = true; debugManager?.Terminate(); }, () => !Restarting);

        private List<int> Indicis { get; set; } = new List<int>();
        public ICommand Terminate => new RelayCommand(() => { debugManager?.Terminate(); restart = false; });

        private DebugManager debugManager;
        private int CurrentLine { get; set; } = -1;

        private Reason CurrentStopReason { get; set; } = Reason.Unset;
        private async void StartDebugCore()
        {
            debugManager = new DebugManager();
            var port = debugManager.NextPort();
            var infos = Indicis;
            var code = Messenger.Send<CodeRequestMessage>();
            var guid = Guid.NewGuid();
            var dir = CurrentDir + $@"\temp\{guid}\";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.Copy(CurrentDir + @"\paramparser.py", dir + @"\paramparser.py");
            SerializeInput(0, $@"{dir}params_data");
            using (var fs = new FileStream($@"{dir}temp.py", FileMode.Create))
            {
                var result = CodeFile(code.Response, $@"{dir}params_data");
                LineOffset = result.Item1;
                var bytes = Encoding.UTF8.GetBytes(result.Item2);
                await fs.WriteAsync(bytes, 0, bytes.Length);
            }
            var file = $@"{dir}temp.py";
            var script = $@"{PythonPath} -u -m debugpy --listen localhost:{port} --wait-for-client --log-to ~/logs ""{file}""";
            debugManager.DebugEnd += DebugManager_DebugEnd;
            debugManager.Stopped += DebugManager_Stopped;
            debugManager.ConfigDone += (s, e) => ConfigDone = true;
            debugManager.Continued += DebugManager_Continued;
            if (Messenger.Send(new DebugRequestMessage { Port = port, Script = script }))
            {
                IsDebuging = true;
                debugManager.Start(infos.ToList(), file);
            }
        }

        private async void DebugManager_DebugEnd(object sender, EventArgs e)
        {
            var start = restart;
            ConfigDone = false;
            Stopped = false;
            Messenger.Send(new StepMessage(false) { Line = -1 });
            if (start)
            {
                await Task.Delay(1000);
                Application.Current.Dispatcher.Invoke(() => StartDebugCore());
            }
            else
            {
                IsDebuging = false;
            }
            Restarting = false;
        }

        private void DebugManager_Continued(object sender, EventArgs e)
        {
            Messenger.Send(new StepMessage(false) { Line = CurrentLine });
            Stopped = false;
            ConfigDone = true;
        }

        private void DebugManager_Stopped(object sender, StoppedArgs e)
        {
            CurrentLine = e.Line;
            Stopped = true;
            CurrentStopReason = e.Reason;
            Messenger.Send(new StepMessage(true) { Line = e.Line-LineOffset });
        }

        public void Receive(object recipient, AllBreakPointInformationsMessage message)
        {
            Indicis.Clear();
            Indicis.AddRange(message.Value);
            if (IsDebuging) debugManager.SendBreakPointRequest(Indicis.Select(i=>i+LineOffset).ToList());
        }


        private int GetIterateCount()
        {
            Type iteratorType = null;
            int i = 0;
            Assembly grasshopperAssembly = Assembly.GetAssembly(typeof(Grasshopper.Instances));
            foreach (Type type in grasshopperAssembly.GetTypes())
                if (type.Name == "GH_StructureIterator")
                {
                    iteratorType = type;
                    break;
                }
            var TheType = iteratorType;
            if (iteratorType != null)
            {
                object iteratorInstance = null;
                foreach (ConstructorInfo constructor in iteratorType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    iteratorInstance = constructor.Invoke(new object[] { Locator.ComponentHost });
                    if (iteratorInstance != null)
                        break;
                }
                var access = iteratorInstance as IGH_DataAccess;
                var m1 = TheType.GetMethod("EnsureAssignments");
                var m2 = TheType.GetMethod("IncrementItemIndices");
                var m3 = TheType.GetMethod("IncrementBranchIndices");
                var m4 = TheType.GetMethod("ResetAssignedFlags");
                m4.Invoke(iteratorInstance, new object[] { });
                while (true)
                {
                    m1.Invoke(iteratorInstance, new object[] { });
                    if ((bool)m2.Invoke(iteratorInstance, new object[] { }))
                    {
                        access.IncrementIteration();
                    }
                    else
                    {
                        if (!(bool)m3.Invoke(iteratorInstance, new object[] { }))
                        {
                            break;
                        }
                        access.IncrementIteration();
                    }
                    i++;
                }
            }
            return i;
        }

        private (int, string) CodeFile(string code, string paramPath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("import paramparser");
            sb.AppendLine($"prmDict = paramparser.parse_args(r'{CurrentDir}',r'{paramPath}')");
            int i = 3;
            foreach (var p in Locator.ComponentHost.Params.Input)
            {
                sb.AppendLine($"{p.NickName}=prmDict['{p.NickName}']");
                i++;
            }
            sb.AppendLine(code);
            i++;
            return (i,sb.ToString());
        }

        private void SerializeInput(int time, string path)
        {
            GH_LooseChunk chunk = new GH_LooseChunk("params data");
            int i = 0;
            Type iteratorType = null;
            Assembly grasshopperAssembly = Assembly.GetAssembly(typeof(Grasshopper.Instances));
            foreach (Type type in grasshopperAssembly.GetTypes())
                if (type.Name == "GH_StructureIterator")
                {
                    iteratorType = type;
                    break;
                }
            var TheType = iteratorType;
            if (iteratorType != null)
            {
                object iteratorInstance = null;
                foreach (ConstructorInfo constructor in iteratorType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    iteratorInstance = constructor.Invoke(new object[] { Locator.ComponentHost });
                    if (iteratorInstance != null)
                        break;
                }
                var access = iteratorInstance as IGH_DataAccess;
                var m1 = TheType.GetMethod("EnsureAssignments");
                var m2 = TheType.GetMethod("IncrementItemIndices");
                var m3 = TheType.GetMethod("IncrementBranchIndices");
                var m4 = TheType.GetMethod("ResetAssignedFlags");
                m4.Invoke(iteratorInstance, new object[] { });
                while (true)
                {
                    if (i == time)
                    {
                        var count = Locator.ComponentHost.Params.Input.Count;
                        chunk.SetInt32("Count", count);
                        for (int j = 0; j < count; j++)
                        {
                            GH_IWriter writer = chunk.CreateChunk("param", j);
                            var param = Locator.ComponentHost.Params.Input[j];
                            switch (param.Access)
                            {
                                case GH_ParamAccess.item:
                                    IGH_Goo data = null;
                                    access.GetData(j, ref data);
                                    var tree2 = new GH_Structure<IGH_Goo>();
                                    tree2.Append(data);
                                    tree2.Write(writer.CreateChunk("Data"));
                                    writer.SetString("access", "Item");
                                    writer.SetString("name", param.NickName);
                                    break;
                                case GH_ParamAccess.list:
                                    List<IGH_Goo> list = new List<IGH_Goo>();
                                    access.GetDataList(j, list);
                                    var tree = new GH_Structure<IGH_Goo>();
                                    tree.AppendRange(list);
                                    tree.Write(writer.CreateChunk("Data"));
                                    writer.SetString("access", "List");
                                    writer.SetString("name", param.NickName);
                                    break;
                                case GH_ParamAccess.tree:
                                    access.GetDataTree(j, out GH_Structure<IGH_Goo> tree1);
                                    tree1.Write(writer.CreateChunk("Data"));
                                    writer.SetString("access", "Tree");
                                    writer.SetString("name", param.NickName);
                                    break;
                            }
                        }

                        GH_IWriter wr = chunk.CreateChunk("param", count);
                        var tree3 = new GH_Structure<IGH_Goo>();
                        tree3.Append(new GH_Curve(new Circle(Point3d.Origin, 10).ToNurbsCurve()));
                        tree3.Write(wr.CreateChunk("Data"));
                        wr.SetString("access", "Tree");
                        wr.SetString("name", "bin");

                        byte[] bytes = chunk.Serialize_Binary();
                        File.WriteAllBytes(path, bytes);
                        break;
                    }
                    m1.Invoke(iteratorInstance, new object[] { });
                    if ((bool)m2.Invoke(iteratorInstance, new object[] { }))
                    {
                        access.IncrementIteration();
                    }
                    else
                    {
                        if (!(bool)m3.Invoke(iteratorInstance, new object[] { }))
                        {
                            break;
                        }
                        access.IncrementIteration();
                    }
                    i++;
                }
            }
        }
    }
}
