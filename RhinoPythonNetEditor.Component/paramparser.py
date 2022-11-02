import clr
import sys
def parse_args(assemblyPath, paramPath):
    sys.path.append(assemblyPath)
    clr.AddReference("RhinoInside")
    import RhinoInside as ri
    ri.Resolver.Initialize()
    print(ri.Resolver.RhinoSystemDirectory)
    sys.path.append(ri.Resolver.RhinoSystemDirectory)
    sys.path.append(ri.Resolver.RhinoSystemDirectory + r'\Plug-ins\Grasshopper')
    clr.AddReference("RhinoCommon")
    clr.AddReference("mscorlib")
    import Rhino.Runtime.InProcess as rri
    import Rhino.Geometry as rg
    import Rhino.Runtime as rr
    import System.IO as si
    import Rhino as rh
    core = rri.RhinoCore()
    clr.AddReference("Grasshopper")
    clr.AddReference("GH_IO")
    clr.AddReference("RhinoPythonNetEditor.Managers")
    import RhinoPythonNetEditor.Managers as rm
    parser = rm.ParamParser(paramPath)
    return parser.ParamDict
