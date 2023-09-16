// using System;
// 
// using RH = Rhino;
// using RUI = Rhino.UI;
// using RD = Rhino.Display;
// using Rhino.Runtime.InteropWrappers;
// 
// using GH_IO.Serialization;
// using GH = Grasshopper;
// using Grasshopper.GUI;
// using Grasshopper.GUI.Canvas;
// using Grasshopper.Kernel;
// using Grasshopper.Kernel.Attributes;
// using System.Runtime.InteropServices;
// 
// 
// using ghgl.Rhino;
// 
// 
// namespace ghgl.Grasshopper;
// 
// 
// [Guid("2DAA8A06-B6E1-4A19-AD41-86E3C3F8617F")]
// public class GLBuiltInShader : GH_Component
// {
//     Program _prog; // = new Pipeline(new Program ());
//     Pipeline _pipeline; // = new Pipeline(new Program ());
//     string _resourceName = "";
//     string _defines = "";
// 
// 
//     public GLBuiltInShader()
//         : base("GL BuiltIn Shader", "GL BuiltIn", "Update internal Rhino Shader", "Display", "Preview")
//     {
//     }
// 
// 
//     public override Guid ComponentGuid => GetType().GUID;
//     
//     public override GH_Exposure Exposure => GH_Exposure.hidden;
//     
// 
//     class GlShaderComponentAttributes : GH_ComponentAttributes
//     {
//         readonly Action _doubleClickAction;
//         public GlShaderComponentAttributes(IGH_Component component, Action doubleClickAction)
//             : base(component)
//         {
//             _doubleClickAction = doubleClickAction;
//         }
// 
//         public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
//         {
//             _doubleClickAction();
//             return base.RespondToMouseDoubleClick(sender, e);
//         }
//     }
// 
//     public override void CreateAttributes()
//     {
//         Attributes = new GlShaderComponentAttributes(this, OpenEditor);
//     }
// 
// 
//     protected override void RegisterInputParams(GH_InputParamManager pManager)
//     {
//         pManager.AddTextParameter("Resource", "R", "Resource Name", GH_ParamAccess.item);
//         pManager.AddTextParameter("Defines", "D", "defines", GH_ParamAccess.item, "");
//     }
// 
//     protected override void RegisterOutputParams(GH_OutputParamManager pManager)
//     {
//     }
// 
// 
//     public override bool Write (GH_IWriter writer)
//     {
//         bool rc = base.Write (writer);
//         if (rc)
//         {
//             writer.SetVersion("GLShader", 0, 1, 0);
//             rc = _prog.Write(writer);
//             writer.SetString("ResourceName", _resourceName);
//             writer.SetString("Defines", _defines);
// 
//         }
//         return rc;
//     }
// 
//     public override bool Read (GH_IReader reader)
//     {
//         bool rc = base.Read (reader) &&
//             _prog.Read (reader);
//         reader.TryGetString("ResourceName", ref _resourceName);
//         reader.TryGetString("Defines", ref _defines);
//         return rc;
//     }
// 
// 
//     protected override void SolveInstance(IGH_DataAccess data)
//     {
//         string resourceName = "";
//         string defines = "";
//         data.GetData(0, ref resourceName);
//         data.GetData(1, ref defines);
//         defines = defines.Replace("\\n", "\n");
//         defines = defines.Replace("\r", "");
//         if (!resourceName.Equals(_resourceName) || !defines.Equals(_defines))
//         {
//             _resourceName = resourceName;
//             _defines      = defines;
//             _prog        = new Program (); // Pourquoir recréer un model-view
//             _pipeline     = new Pipeline(_prog);
//         }
//         if( _prog.ProgramId == 0 &&
//             !string.IsNullOrWhiteSpace(_prog.VertexShaderCode) &&
//             !string.IsNullOrWhiteSpace(_resourceName))
//         {
//             ActivateGL();
//             if (_prog.CompileProgram())
//             {
//                 if (_prog.ProgramId != 0)
//                 {
//                     if (RH.Runtime.HostUtils.RunningOnWindows)
//                         WindowsMethods.RHC_UpdateShader(_resourceName, _defines, _prog.ProgramId);
//                     else
//                         MacMethods.RHC_UpdateShader(_resourceName, _defines, _prog.ProgramId);
//                     //_model.RecycleCurrentProgram = false;
// 
//                     if (RH.RhinoDoc.ActiveDoc is RH.RhinoDoc doc)
//                         doc.Views.Redraw();
//                     GHGL_ShaderComponentBase.RedrawCanvasViewportControl ();
//                 }
//             }
//         }
//     }
// 
// 
//     public override void AppendAdditionalMenuItems(System.Windows.Forms.ToolStripDropDown menu)
//     {
//         base.AppendAdditionalMenuItems(menu);
// 
//         var tsi = new System.Windows.Forms.ToolStripMenuItem("&Edit code...", null, (sender, e) =>
//         {
//             OpenEditor();
//         });
//         tsi.Font = new System.Drawing.Font(tsi.Font, System.Drawing.FontStyle.Bold);
//         menu.Items.Add(tsi);
//         menu.Items.Add(new System.Windows.Forms.ToolStripMenuItem("Reset", null, (sender, e) =>
//         {
//             if( RUI.Dialogs.ShowMessage("Reset the code to what is built-in?", "reset", RUI.ShowMessageButton.OKCancel, RUI.ShowMessageIcon.Question) == RUI.ShowMessageResult.OK)
//             {
//                 _prog    = new Program ();        // Pourquoir recréer un model-view
//                 _pipeline = new Pipeline(_prog);
//             }
//         })
//         );
//     }
// 
// 
//     void OpenEditor()
//     {
//         if (string.IsNullOrWhiteSpace(_resourceName))
//             return;
//         if( string.IsNullOrWhiteSpace(_prog.VertexShaderCode))
//         {
//             using (var vertex = new StringWrapper())
//             using (var tessctrl = new StringWrapper())
//             using (var tesseval = new StringWrapper())
//             using (var geometry = new StringWrapper())
//             using (var fragment = new StringWrapper())
//             {
//                 IntPtr _vertex = vertex.NonConstPointer;
//                 IntPtr _tessctrl = tessctrl.NonConstPointer;
//                 IntPtr _tesseval = tesseval.NonConstPointer;
//                 IntPtr _geometry = geometry.NonConstPointer;
//                 IntPtr _fragment = fragment.NonConstPointer;
//                 if( RH.Runtime.HostUtils.RunningOnWindows )
//                     WindowsMethods.RHC_GetShaderSource(_resourceName, _defines, _vertex, _tessctrl, _tesseval, _geometry, _fragment);
//                 else
//                     MacMethods.RHC_GetShaderSource(_resourceName, _defines, _vertex, _tessctrl, _tesseval, _geometry, _fragment);
// 
//                 _prog.VertexShaderCode = vertex.ToString();
//                 _prog.TessellationControlCode = tessctrl.ToString();
//                 _prog.TessellationEvalualtionCode = tesseval.ToString();
//                 _prog.GeometryShaderCode = geometry.ToString();
//                 _prog.FragmentShaderCode = fragment.ToString();
//             }
//         }
// 
//         RD.DisplayPipeline.PreDrawObjects += DisplayPipeline_PreDrawObjects;
//         string savedVS = _prog.VertexShaderCode;
//         string savedGS = _prog.GeometryShaderCode;
//         string savedTC = _prog.TessellationControlCode;
//         string savedTE = _prog.TessellationEvalualtionCode;
//         string savedFS = _prog.FragmentShaderCode;
//         string savedXfrmFeedbackVertex = _prog.TransformFeedbackShaderCode;
// 
//         var dlg = new UI.ProgramEditorForm(_prog, "Built-In");
//         var parent = RUI.Runtime.PlatformServiceProvider.Service.GetEtoWindow(GH.Instances.DocumentEditor.Handle);
//         _prog.Modified = false;
// 
//         dlg.Closed += (s, e) =>
//         {
// 
//             if (!dlg.Canceled)
//             {
//                 if (_prog.Modified)
//                 {
//                     var doc = OnPingDocument();
//                     doc?.Modified();
//                 }
//             }
//             else
//             {
//                 _prog.VertexShaderCode = savedVS;
//                 _prog.GeometryShaderCode = savedGS;
//                 _prog.FragmentShaderCode = savedFS;
//                 _prog.TessellationControlCode = savedTC;
//                 _prog.TessellationEvalualtionCode = savedTE;
//                 _prog.TransformFeedbackShaderCode = savedXfrmFeedbackVertex;
//             }
//             _prog.Modified = false;
//             //recompile shader if necessary
//             if (_prog.ProgramId == 0)
//                 ExpireSolution(true);
//             RD.DisplayPipeline.PreDrawObjects -= DisplayPipeline_PreDrawObjects;
//         };
//         dlg.Title = this.NickName;
//         dlg.Owner = parent;
//         dlg.Show();
//     }
// 
//     private void DisplayPipeline_PreDrawObjects(object sender, RD.DrawEventArgs e)
//     {
//         RH_Renderer.UpdateContext (e);
//         if( _prog.ProgramId != 0 )
//         {
//             if( RH.Runtime.HostUtils.RunningOnWindows)
//                 WindowsMethods.RHC_UpdateShader(_resourceName, _defines, _prog.ProgramId); // !!!!!!!!!!! new doit plus fonctionner
//             else
//                 MacMethods.RHC_UpdateShader(_resourceName, _defines, _prog.ProgramId); // !!!!!!!!!!! new doit plus fonctionner
//             //_model.RecycleCurrentProgram = false;
//         }
//     }
// 
//     public static void ActivateGL()
//     {
//         if (RH.Runtime.HostUtils.RunningOnOSX)
//             MacMethods.RHC_ActivateGL();
//     }
// 
// 
//     class WindowsMethods
//     {
//         const string lib = "rhcommon_c";
//         //void RHC_GetShaderSource(const RHMONO_STRING* resource_name, const RHMONO_STRING* defines,
//         //  ON_wString* vertex, ON_wString* tessctrl, ON_wString* tesseval, ON_wString* geometry, ON_wString* fragment)
//         // C:\dev\github\mcneel\rhino\src4\DotNetSDK\rhinocommon\c\rh_displaypipeline.cpp line 2552
//         [DllImport(lib, CallingConvention = CallingConvention.Cdecl)]
//         internal static extern void RHC_GetShaderSource([MarshalAs(UnmanagedType.LPWStr)]string resourceName, [MarshalAs(UnmanagedType.LPWStr)]string defines, IntPtr vertex, IntPtr tessctrl, IntPtr tesseval, IntPtr geometry, IntPtr fragment);
// 
//         //void RHC_UpdateShader(const RHMONO_STRING* resourceName, const RHMONO_STRING* defines, unsigned int programId)
//         // C:\dev\github\mcneel\rhino\src4\DotNetSDK\rhinocommon\c\rh_displaypipeline.cpp line 2561
//         [DllImport(lib, CallingConvention = CallingConvention.Cdecl)]
//         internal static extern void RHC_UpdateShader([MarshalAs(UnmanagedType.LPWStr)]string resourceName, [MarshalAs(UnmanagedType.LPWStr)]string defines, uint programId);
//     }
//     
//     class MacMethods
//     {
//         const string lib = "__Internal";
//         //void RHC_GetShaderSource(const RHMONO_STRING* resource_name, const RHMONO_STRING* defines,
//         //  ON_wString* vertex, ON_wString* tessctrl, ON_wString* tesseval, ON_wString* geometry, ON_wString* fragment)
//         // C:\dev\github\mcneel\rhino\src4\DotNetSDK\rhinocommon\c\rh_displaypipeline.cpp line 2552
//         [DllImport(lib, CallingConvention = CallingConvention.Cdecl)]
//         internal static extern void RHC_GetShaderSource([MarshalAs(UnmanagedType.LPWStr)]string resourceName, [MarshalAs(UnmanagedType.LPWStr)]string defines, IntPtr vertex, IntPtr tessctrl, IntPtr tesseval, IntPtr geometry, IntPtr fragment);
// 
//         //void RHC_UpdateShader(const RHMONO_STRING* resourceName, const RHMONO_STRING* defines, unsigned int programId)
//         // C:\dev\github\mcneel\rhino\src4\DotNetSDK\rhinocommon\c\rh_displaypipeline.cpp line 2561
//         [DllImport(lib, CallingConvention = CallingConvention.Cdecl)]
//         internal static extern void RHC_UpdateShader([MarshalAs(UnmanagedType.LPWStr)]string resourceName, [MarshalAs(UnmanagedType.LPWStr)]string defines, uint programId);
// 
//         [DllImport(lib, CallingConvention = CallingConvention.Cdecl)]
//         internal static extern void RHC_ActivateGL();
//     }
// }
// 