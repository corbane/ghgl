using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using SF = System.Windows.Forms;
using SD = System.Drawing;

using RH  = Rhino;
using RUI = Rhino.UI;
using ON  = Rhino.Geometry;
using RD  = Rhino.Display;

using GIO = GH_IO;
using GH  = Grasshopper;
using GK  = Grasshopper.Kernel;
using GUI = Grasshopper.GUI;
using GT  = Grasshopper.Kernel.Types;

using RhGL.Rhino;
using RhGL.UI;
using RhGL.IO;


namespace RhGL.Grasshopper;



public class TrackerParam : GK.Parameters.Param_ScriptVariable
{
    public override Guid ComponentGuid => new ("{A356DFA7-4B68-4565-A8C8-75DAF2E2EE20}");

    protected override void OnVolatileDataCollected()
    {
        RH.RhinoApp.WriteLine ($"--- Param {this.Name} collected ---");
        base.OnVolatileDataCollected();
    }
}

public class TrackerParam_Mesh : GK.Parameters.Param_Mesh
{
    public override Guid ComponentGuid => new ("{3B9EDBB4-FC99-4B49-85EB-32610AFC9B20}");
    public override GK.GH_Exposure Exposure => GK.GH_Exposure.hidden;

    protected override void OnVolatileDataCollected()
    {
        RH.RhinoApp.WriteLine ($"--- Param {this.Name} collected ---");
        base.OnVolatileDataCollected();
    }
}



/// <summary>
/// Base class for GL Shader components. Most of the heavy lifting is done in this class
/// and the subclasses just specialize a little bit
/// </summary>
public abstract class GHGL_ShaderComponentBase : GK.GH_Component, GK.IGH_VariableParameterComponent, GUI.IGH_FileDropTarget
{
    static ON.Point2f[]? GooListToPoint2fArray(List <GT.IGH_Goo> list)
    {
        int count = list.Count;
        if (count < 1 || list[0] == null)
            return null;

        ON.Point2d point;
        if (list[0].CastTo(out point))
        {
            var vec2_array = new ON.Point2f[count];
            for (int i = 0; i < count; i++)
            {
                if (list[i].CastTo(out point))
                {
                    float x = (float)point.X;
                    float y = (float)point.Y;
                    vec2_array[i] = new ON.Point2f(x, y);
                }
            }
            return vec2_array;
        }

        ON.Vector2d vector;
        if (list[0].CastTo(out vector))
        {
            var vec2_array = new ON.Point2f[count];
            for (int i = 0; i < count; i++)
            {
                if (list[i].CastTo(out vector))
                {
                    float x = (float)vector.X;
                    float y = (float)vector.Y;
                    vec2_array[i] = new ON.Point2f(x, y);
                }
            }
            return vec2_array;
        }
        return null;
    }

    static ON.Point3f[]? GooListToPoint3fArray(List <GT.IGH_Goo> list)
    {
        int count = list.Count;
        if (count < 1 || list[0]==null)
            return null;

        if (list[0].CastTo (out ON.Point3d point))
        {
            var vec3_array = new ON.Point3f[count];
            for( int i=0; i<count; i++ )
            {
                if (list[i].CastTo (out point))
                {
                    float x = (float)point.X;
                    float y = (float)point.Y;
                    float z = (float)point.Z;
                    vec3_array[i] = new ON.Point3f(x, y, z);
                }
            }
            return vec3_array;
        }

        if (list[0].CastTo (out ON.Vector3d vector))
        {
            var vec3_array = new ON.Point3f[count];
            for (int i = 0; i < count; i++)
            {
                if (list[i].CastTo (out vector))
                {
                    float x = (float)vector.X;
                    float y = (float)vector.Y;
                    float z = (float)vector.Z;
                    vec3_array[i] = new ON.Point3f(x, y, z);
                }
            }
            return vec3_array;
        }
        return null;
    }

    // Cette classe semble redessiner les GhCanvaViewport (mais pas encore certain de cela).
    // Ce n'est peut être pas une bonne place ici.
    internal static void RedrawCanvasViewportControl () // Une ref dans BuitinShader
    {
        if (GH.Instances.ActiveCanvas != null)
        {
            var ctrls = GH.Instances.ActiveCanvas.Controls;
            if (ctrls != null)
            {
                for (int i = 0; i < ctrls.Count; i++)
                    ctrls[i].Refresh();
            }

        }
    }
 

    protected GHGL_ShaderComponentBase (string name, string nickname, string description)
        : base(name, nickname, description, "Display", "Preview")
    {
        if (GlslifyClient.IsInitialized == false)
            GlslifyClient.Initialize ();

        if (Conduit.InitializationAttempted == false)
            Conduit.Initialize ();
        
        _model    = new Program ();
        _pipeline = new Pipeline (_model);
        _model.PropertyChanged += _ModelPropertyChanged;
    }


    #region Properties


    int _majorVersion = 0;
    int _minorVersion = 2;


    public override Guid ComponentGuid => GetType().GUID;

    public override GK.GH_Exposure Exposure => GK.GH_Exposure.tertiary;


    public new bool Hidden 
    {
        get => base.Hidden;
        set {
            base.Hidden = value;
            Pipeline.Hidden = value;
        }
    }


    #endregion


    #region Component Attributes/DragDrop


    class GlShaderComponentAttributes : GK.Attributes.GH_ComponentAttributes
    {
        readonly GHGL_ShaderComponentBase _component;
        readonly Action _doubleClickAction;
        public GlShaderComponentAttributes(GHGL_ShaderComponentBase component, Action doubleClickAction)
            : base(component)
        {
            _component = component;
            _doubleClickAction = doubleClickAction;
        }

        public override GUI.Canvas.GH_ObjectResponse RespondToMouseDoubleClick(GUI.Canvas.GH_Canvas sender, GUI.GH_CanvasMouseEvent e)
        {
            _doubleClickAction();
            return base.RespondToMouseDoubleClick(sender, e);
        }

        public override void SetupTooltip(SD.PointF point, GUI.GH_TooltipDisplayEventArgs e)
        {
            // Allow the base class to set up the tooltip.
            // It will handle those cases where the mouse is over a state icon.
            base.SetupTooltip(point, e);

            try
            {
                using (var colorBuffer = PerFrameCache.GetTextureImage (_component.Pipeline.Guid, true))
                using (var depthBuffer = PerFrameCache.GetTextureImage (_component.Pipeline.Guid, false))
                {
                    if (colorBuffer != null && depthBuffer != null)
                    {
                        var size = colorBuffer.Size;
                        size.Width /= 2;
                        var bmp = new SD.Bitmap(size.Width, size.Height);
                        using (var g = SD.Graphics.FromImage(bmp))
                        {
                            g.DrawImage(colorBuffer, SD.Rectangle.FromLTRB(0, 0, size.Width, size.Height / 2));
                            g.DrawImage(depthBuffer, SD.Rectangle.FromLTRB(0, size.Height / 2, size.Width, size.Height));
                        }
                        e.Description = "Output Color/Depth Buffers";
                        e.Diagram = bmp;
                        //e.Diagram = GH_IconTable.ResizeImage(colorBuffer, new Size(300, 200),
                        //SD.Drawing2D.InterpolationMode.HighQualityBicubic,
                        //SD.Imaging.PixelFormat.Format24bppRgb);
                    }
                }
            }
            catch { /* no action required. */ }
        }
    }

    public override void CreateAttributes()
    {
        Attributes = new GlShaderComponentAttributes(this, OpenEditor);
    }

    public bool AcceptableFile (string path)
    {
        return AssetIO.IsValidJsonAsset (path);
    }

    public bool HandleDrop (string path, SD.PointF mouse_pt)
    {
        _pipeline.Program.FilePath = path;
        return _pipeline.Program.FilePath == path;
    }


    #endregion


    #region Serialization


    public override bool Write (GIO.Serialization.GH_IWriter writer)
    {
        bool rc = base.Write (writer);
        if (rc)
        {
            writer.SetVersion ("GLShader", _majorVersion, _minorVersion, 0);
            rc = _model.Write(writer);
        }
        return rc;
    }

    public override bool Read (GIO.Serialization.GH_IReader reader)
    {
        bool rc = base.Read (reader) &&
            _model.Read (reader);
        var version = reader.GetVersion ("GLShader");
        _majorVersion = version.major;
        _minorVersion = version.minor;
        return rc;
    }


    #endregion


    #region Params


    protected override void RegisterInputParams (GH_InputParamManager pManager)
    {
    }

    protected override void RegisterOutputParams (GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("Color", "C", "Color Buffer", GK.GH_ParamAccess.item);
        pManager.AddTextParameter("Depth", "D", "Depth Buffer", GK.GH_ParamAccess.item);
    }

    public virtual bool CanInsertParameter (GK.GH_ParameterSide side, int index)
    {
        return side == GK.GH_ParameterSide.Input;
    }

    bool GK.IGH_VariableParameterComponent.CanRemoveParameter (GK.GH_ParameterSide side, int index)
    {
        return (this as GK.IGH_VariableParameterComponent).CanInsertParameter (side, index);
    }

    public GK.IGH_Param? CreateParameter (GK.GH_ParameterSide side, int index)
    {
        if (side != GK.GH_ParameterSide.Input)
            return null;

        return new TrackerParam //Param_ScriptVariable
        {
            NickName    = GK.GH_ComponentParamServer.InventUniqueNickname("xyzuvwst", Params.Input),
            Name        = NickName,
            Description = "Script variable " + NickName,
            Access      = GK.GH_ParamAccess.list
        };
    }

    bool GK.IGH_VariableParameterComponent.DestroyParameter (GK.GH_ParameterSide side, int index)
    {
        return true;
    }

    void GK.IGH_VariableParameterComponent.VariableParameterMaintenance ()
    {
        foreach (GK.IGH_Param param in Params.Input)
            param.Name = param.NickName;
    }
    

    #endregion


    #region Menu


    public override void AppendAdditionalMenuItems (SF.ToolStripDropDown menu)
    {
        base.AppendAdditionalMenuItems (menu);

        var tsi = new SF.ToolStripMenuItem ("&Edit code...", null, (sender, e) => { OpenEditor (); });
        tsi.Font = new SD.Font (tsi.Font, SD.FontStyle.Bold);
        menu.Items.Add (tsi);

        tsi = new SF.ToolStripMenuItem ("Sort Order");
        for(int i = 1; i <= 10; i++)
        {
            int order = i;
            var tsi_sub = new SF.ToolStripMenuItem (i.ToString(), null, (s, e) =>
            {
                _model.PreviewSortOrder = order;
                Scheduler.SortComponents (); // Pourquoi, Dans OnDrawObjects, SortComponents est executé
                if (RH.RhinoDoc.ActiveDoc is RH.RhinoDoc doc)
                    doc.Views.Redraw();
            }){
                Checked = _model.PreviewSortOrder == order
            };
            tsi.DropDown.Items.Add  (tsi_sub);
        }
        menu.Items.Add (tsi);

        void AppendModeHelper(SF.ToolStripMenuItem parent, string name, uint mode)
        {
            var tsi_sub = new SF.ToolStripMenuItem (name, null, (s, e) => {
                _model.DrawMode = mode;
                RedrawCanvasViewportControl();
            });
            tsi_sub.Checked = _model.DrawMode == mode;
            parent.DropDown.Items.Add (tsi_sub);
        }

        void PatchHelper (SF.ToolStripMenuItem parent, string name, ushort count)
        {
            var tsi_sub = new SF.ToolStripMenuItem (name, null) {
                Checked = _model.DrawMode == OpenGL.GL_PATCHES && _model.PatchVertices == count
            };
            tsi_sub.Click += (s, e) => {
                _model.DrawMode = OpenGL.GL_PATCHES;
                _model.PatchVertices = count;
                RedrawCanvasViewportControl();
            };
            parent.DropDown.Items.Add (tsi_sub);
        }

        tsi = new SF.ToolStripMenuItem ("Draw Mode");
        bool isGenericGLComponent = this is GHGL_ShaderComponent;
        AppendModeHelper (tsi, "GL_POINTS", OpenGL.GL_POINTS);
        AppendModeHelper (tsi, "GL_LINES", OpenGL.GL_LINES);
        if (isGenericGLComponent)
        {
            AppendModeHelper (tsi, "GL_LINE_LOOP", OpenGL.GL_LINE_LOOP);
            AppendModeHelper (tsi, "GL_LINE_STRIP", OpenGL.GL_LINE_STRIP);
        }
        AppendModeHelper (tsi, "GL_TRIANGLES", OpenGL.GL_TRIANGLES);
        if (isGenericGLComponent)
        {
            AppendModeHelper (tsi, "GL_TRIANGLE_STRIP", OpenGL.GL_TRIANGLE_STRIP);
            AppendModeHelper (tsi, "GL_TRIANGLE_FAN", OpenGL.GL_TRIANGLE_FAN);
            // The following are deprecated in core profile. I don't think we should add support for them
            //AppendModeHelper (tsi, "GL_QUADS", OpenGL.GL_QUADS);
            //AppendModeHelper (tsi, "GL_QUAD_STRIP", OpenGL.GL_QUAD_STRIP);
            //AppendModeHelper (tsi, "GL_POLYGON", OpenGL.GL_POLYGON);
            AppendModeHelper (tsi, "GL_LINES_ADJACENCY", OpenGL.GL_LINES_ADJACENCY);
            AppendModeHelper (tsi, "GL_LINE_STRIP_ADJACENCY", OpenGL.GL_LINE_STRIP_ADJACENCY);
            AppendModeHelper (tsi, "GL_TRIANGLES_ADJACENCY", OpenGL.GL_TRIANGLES_ADJACENCY);
            AppendModeHelper (tsi, "GL_TRIANGLE_STRIP_ADJACENCY", OpenGL.GL_TRIANGLE_STRIP_ADJACENCY);
            // Not yet, this may require a completely different component
            // AppendModeHelper (tsi, "GL_PATCHES", OpenGL.GL_PATCHES);
        }
        var tsi_patches  = new SF.ToolStripMenuItem("GL_PATCHES");
        PatchHelper (tsi_patches, "1", 1);
        PatchHelper (tsi_patches, "2", 2);
        PatchHelper (tsi_patches, "3", 3);
        PatchHelper (tsi_patches, "4", 4);
        tsi.DropDown.Items.Add (tsi_patches);
        menu.Items.Add (tsi);

        tsi = new SF.ToolStripMenuItem("glLineWidth");
        Menu_AppendTextItem(tsi.DropDown, $"{_model.glLineWidth:F2}", (s, e) => MenuKeyDown(s, e, true), Menu_SingleDoubleValueTextChanged, true, 200, true);
        menu.Items.Add(tsi);
        tsi = new SF.ToolStripMenuItem("glPointSize");
        Menu_AppendTextItem(tsi.DropDown, $"{_model.glPointSize:F2}", (s, e) => MenuKeyDown(s, e, false), Menu_SingleDoubleValueTextChanged, true, 200, true);
        menu.Items.Add(tsi);

        tsi = new SF.ToolStripMenuItem ("Depth Testing", null, (sender, e) =>
        {
            _model.DepthTestingEnabled = !_model.DepthTestingEnabled;
        });
        tsi.Checked = _model.DepthTestingEnabled;
        menu.Items.Add(tsi);

        tsi = new SF.ToolStripMenuItem ("Depth Writing", null, (sender, e) =>
        {
            _model.DepthWritingEnabled = !_model.DepthWritingEnabled;
        });
        tsi.Checked = _model.DepthWritingEnabled;
        menu.Items.Add(tsi);

        tsi = new SF.ToolStripMenuItem ("Export...", null, (sender, e) => { ExportToHTML(); });
        menu.Items.Add(tsi);
    }
    
    void Menu_SingleDoubleValueTextChanged (GUI.GH_MenuTextBox sender, string text)
    {
        if ((text.Length == 0))
        {
            sender.TextBoxItem.ForeColor = SD.SystemColors.WindowText;
        }
        else
        {
            double d;
            if ((GK.GH_Convert.ToDouble(text, out d, GK.GH_Conversion.Secondary) && d > 0))
                sender.TextBoxItem.ForeColor = SD.SystemColors.WindowText;
            else
                sender.TextBoxItem.ForeColor = SD.Color.Red;
        }
    }
    
    void MenuKeyDown (GUI.GH_MenuTextBox sender, SF.KeyEventArgs e, bool lineWidth)
    {
        switch (e.KeyCode)
        {
            case SF.Keys.Enter:
                string text = sender.Text;
                e.Handled = true;
                double val;
                if (GK.GH_Convert.ToDouble (text, out val, GK.GH_Conversion.Secondary) && val > 0)
                {
                    if (lineWidth)
                        _model.glLineWidth = val;
                    else
                        _model.glPointSize = val;
                    ExpirePreview(true);
                }
                break;
            case SF.Keys.Escape:
                sender.CloseEntireMenuStructure();
                break;
        }
        RedrawCanvasViewportControl();
    }


    #endregion


    #region Pipeline 


    readonly Pipeline _pipeline;
    readonly Program _model;

    internal Pipeline Pipeline => _pipeline;
    protected Program Program => _model;

    // la propriété Program.Hidden est controllé par this.Hidden

    void _ModelPropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof (Program.DrawMode):
            case nameof (Program.PatchVertices):
            case nameof (Program.glLineWidth):
            case nameof (Program.glPointSize):
            case nameof (Program.DepthTestingEnabled):
            case nameof (Program.DepthWritingEnabled):
                ExpirePreview (true);
                break;
        }
    }


    #endregion


    #region Execution 


    const string VersionErrorMessage
        = "This version of GhGL is for Rhino 7 or above and will not work in Rhino 6.\n"
        + "Either run TestPackageManager and choose to use GhGL version 0.6.4 or use Rhino 7";

    protected override void SolveInstance (GK.IGH_DataAccess DA)
    {
        SolveInstanceHelper(DA, 0);
    }

    protected void SolveInstanceHelper (GK.IGH_DataAccess data, int startIndex)
    {
        if (RH.RhinoApp.ExeVersion == 6)
            AddRuntimeMessage (GK.GH_RuntimeMessageLevel.Error, VersionErrorMessage);

        if (Conduit.IsEnabled == false)
            Conduit.EnableConduit ();
        if (Conduit.ActivateGlContext () == false)
            return;
        if (OpenGL.IsAvailable == false)
            AddRuntimeMessage (GK.GH_RuntimeMessageLevel.Error, "Unable to access required OpenGL features");

        if ( _majorVersion>0 || _minorVersion>1 )
        {
            data.SetData(0, $"{InstanceGuid}:color");
            data.SetData(1, $"{InstanceGuid}:depth");
        }

        if (data.Iteration == 0)
        {
            if (!_model.CompileProgram ())
            {
                foreach (var err in _model.AllCompileErrors())
                    AddRuntimeMessage (GK.GH_RuntimeMessageLevel.Error, err.ToString());
            }

            _pipeline.ClearIterationData ();
        }

        // --- Gets component input data then sets pipeline data ---

        List <GT.IGH_Goo> paramData;
        var pipelineData = _pipeline.GetIterationData (data.Iteration);

        bool bval;
        int ival;
        double dval;
        string sval;

        for (int i = startIndex; i < Params.Input.Count; i++)
        {
            if (Params.Input[i].Access == GK.GH_ParamAccess.item)
            {
                GT.IGH_Goo destination = null!;
                data.GetData(i, ref destination);
                paramData = new (new [] { destination });
            }
            else
            {
                paramData = new ();
                data.GetDataList(i, paramData);
            }

            string varname = StringLib.RemoveArrayDescriptor (Params.Input[i].NickName);
            // string varname = Params.Input[i].NickName;
            // if (varname.Contains("["))
            //     varname = varname.Substring(0, varname.IndexOf('['));

            if (_model.TryGetUniformType (varname, out var uType, out int arrayLength))
            {
                switch (uType)
                {
                case GlslUniformType.Int:
                //case "int":
                    {
                        int[] values = new int[paramData.Count];
                        for (int j = 0; j < values.Length; j++)
                        {
                            GT.IGH_Goo destination = paramData[j];
                            if (false == destination.CastTo(out ival))
                            {
                                if (destination.CastTo (out dval))
                                    ival = (int)dval;
                                else if (destination.CastTo (out bval))
                                    ival = bval ? 1 : 0;
                            }
                            values[j] = ival;
                        }

                        // hack for iterations
                        if (arrayLength == 0 && data.Iteration < paramData.Count)
                            values[0] = values[data.Iteration];

                        pipelineData.AddUniform (varname, values, arrayLength);
                        break;
                    }
                case GlslUniformType.Float:
                //case "float":
                    {
                        float[] values = new float[paramData.Count];
                        for (int j = 0; j < values.Length; j++)
                        {
                            GT.IGH_Goo destination = paramData[j];
                            //destination.CastTo(out dval);
                            if (false == destination.CastTo (out dval))
                            {
                                if (destination.CastTo (out ival))
                                    dval = ival;
                            }
                            values[j] = (float)dval;
                        }

                        // hack for iterations
                        if (arrayLength == 0 && data.Iteration < paramData.Count)
                            values[0] = values[data.Iteration];

                        pipelineData.AddUniform(varname, values, arrayLength);
                        break;
                    }
                case GlslUniformType.Vec3:
                //case "vec3":
                    {
                        var values = GooListToPoint3fArray (paramData);

                        // hack for iterations
                        if (arrayLength == 0 && data.Iteration < paramData.Count)
                            values[0] = values[data.Iteration];

                        if ( values != null )
                            pipelineData.AddUniform (varname, values, arrayLength);
                        break;
                    }
                case GlslUniformType.Vec4:
                //case "vec4":
                    {
                        GL_Vec4[] values = new GL_Vec4[paramData.Count];
                        for (int j = 0; j < values.Length; j++)
                        {
                            GT.IGH_Goo destination = paramData[j];
                            if (destination.TypeName == "Colour")
                            {
                                SD.Color color;
                                if (destination.CastTo(out color))
                                {
                                    values[j] = new GL_Vec4 (color.R/255.0f, color.G/255.0f, color.B/255.0f, color.A/255.0f);
                                }
                            }
                        }

                        // hack for iterations
                        if (arrayLength == 0 && data.Iteration < paramData.Count)
                            values[0] = values[data.Iteration];

                        pipelineData.AddUniform(varname, values, arrayLength);
                        break;
                    }
                case GlslUniformType.Mat4:
                //case "mat4":
                    {
                        GL_Mat4[] values = new GL_Mat4[paramData.Count];
                        for (int j = 0; j < values.Length; j++)
                        {
                            GT.IGH_Goo destination = paramData[j];
                            ON.Transform xform;
                            if (destination.CastTo(out xform))
                            {
                                values[j] = new GL_Mat4(xform);
                            }
                        }

                        // hack for iterations
                        if (arrayLength == 0 && data.Iteration < paramData.Count)
                            values[0] = values[data.Iteration];

                        pipelineData.AddUniform(varname, values, arrayLength);
                        break;
                    }
                case GlslUniformType.Bool:
                //case "bool":
                    {
                        int[] values = new int[paramData.Count];
                        for (int j = 0; j < values.Length; j++)
                        {
                            GT.IGH_Goo destination = paramData[j];
                            if (!destination.CastTo(out bval))
                            {
                                //int ivalue;
                                if (destination.CastTo(out ival))
                                    bval = ival != 0;
                            }
                            values[j] = bval ? 1 : 0;
                        }

                        // hack for iterations
                        if (arrayLength == 0 && data.Iteration < paramData.Count)
                            values[0] = values[data.Iteration];

                        pipelineData.AddUniform(varname, values, arrayLength);
                        break;
                    }
                case GlslUniformType.Sampler2D:
                //case "sampler2D":
                    {
                        GT.IGH_Goo destination = paramData[0];
                        //Try casting to a string first. This will be interpreted as a path to an image file
                        if (destination.CastTo (out string path))
                        {
                            bool isComponentInput = false;
                            // see if path refers to a component's output parameter
                            if(path.EndsWith(":color") || path.EndsWith(":depth"))
                            {
                                string id = path.Substring(0, path.IndexOf(":"));
                                isComponentInput = Guid.TryParse(id, out Guid compId);
                            }

                            if (!isComponentInput)
                            {
                                bool isUrl = path.StartsWith("http:/", StringComparison.InvariantCultureIgnoreCase) ||
                                    path.StartsWith("https:/", StringComparison.InvariantCultureIgnoreCase);
                                if (!isUrl && !System.IO.File.Exists(path))
                                {
                                    var ghdoc = OnPingDocument();
                                    if (ghdoc != null)
                                    {
                                        string workingDirectory = System.IO.Path.GetDirectoryName(ghdoc.FilePath);
                                        path = System.IO.Path.GetFileName(path);
                                        path = System.IO.Path.Combine(workingDirectory, path);
                                    }
                                }
                            }

                            pipelineData.AddSampler2DUniform (varname, path);
                        }
                        else
                        {
                            if (destination.CastTo (out SD.Bitmap bmp))
                            {
                                pipelineData.AddSampler2DUniform (varname, bmp);
                            }
                        }

                        break;
                    }
                }
                continue;
            }

            if (_model.TryGetAttributeType (varname, out var aType, out int location))
            {
                switch (aType)
                {
                case GlslAttribType.Int:
                case GlslAttribType.Float:
                    {
                        var goo = new List<GT.IGH_Goo>();
                        if(data.GetDataList(i, goo))
                        {
                            // var ints   = datatype == "int" ? new int[goo.Count] : null;
                            var ints   = aType == GlslAttribType.Int ? new int[goo.Count] : null;
                            var floats = ints == null ? new float[goo.Count] : null;
                            for (int index = 0; index < goo.Count; index++)
                            {
                                if (goo[index].CastTo(out dval))
                                {
                                    if (ints != null)
                                        ints[index] = (int)dval;
                                    if (floats != null)
                                        floats[index] = (float)dval;
                                }
                                else if (goo[index].CastTo(out ival))
                                {
                                    if (ints != null)
                                        ints[index] = ival;
                                    if (floats != null)
                                        floats[index] = (float)ival;
                                }
                                else if (goo[index].CastTo(out sval))
                                {
                                    if (double.TryParse(sval, out dval))
                                    {
                                        if (ints != null)
                                            ints[index] = (int)dval;
                                        if (floats != null)
                                            floats[index] = (float)dval;
                                    }
                                }
                            }
                            if (ints != null && ints.Length > 0)
                                pipelineData.AddAttribute(varname, location, ints);
                            if (floats != null && floats.Length > 0)
                                pipelineData.AddAttribute(varname, location, floats);
                        }
                        if (goo.Count < 1 && aType == GlslAttribType.Int)
                        {
                            var int_destination = new List<int>();
                            if (data.GetDataList(i, int_destination) && int_destination.Count > 0)
                            {
                                int[] ints = int_destination.ToArray();
                                if (ints != null && ints.Length > 0)
                                    pipelineData.AddAttribute(varname, location, ints);
                            }

                        }
                    }
                    break;
                case GlslAttribType.Vec2:
                    {
                        //vec2 -> point2d
                        var vec2_array = GooListToPoint2fArray(paramData);
                        if (vec2_array != null)
                            pipelineData.AddAttribute(varname, location, vec2_array);
                    }
                    break;
                case GlslAttribType.Vec3:
                    {
                        //vec3 -> point3d
                        var vec3_array = GooListToPoint3fArray(paramData);
                        if( vec3_array!=null )
                            pipelineData.AddAttribute(varname, location, vec3_array);
                    }
                    break;
                case GlslAttribType.Vec4:
                    {
                        var destination = new List<SD.Color>();
                        if (data.GetDataList(i, destination))
                        {
                            GL_Vec4[] vec4_array = new GL_Vec4[destination.Count];
                            for (int index = 0; index < destination.Count; index++)
                            {
                                var color = new RD.Color4f(destination[index]);
                                vec4_array[index] = new GL_Vec4(color.R, color.G, color.B, color.A);
                            }
                            pipelineData.AddAttribute(varname, location, vec4_array);
                        }
                    }
                    break;
                case GlslAttribType.Mat4:
                    {
                        var destination = new List<ON.Transform>();
                        if (data.GetDataList(i, destination))
                        {
                            GL_Mat4[] mat4_array = new GL_Mat4[destination.Count];
                            for (int index = 0; index < destination.Count; index++)
                            {
                                mat4_array[index] = new GL_Mat4(destination[index]);
                            }
                            pipelineData.AddAttribute(varname, location, mat4_array);
                        }
                    }
                    break;
                }
                continue;

                // if (datatype == "int" || datatype == "float")
                // {
                //     var goo = new List<GT.IGH_Goo>();
                //     if(data.GetDataList(i, goo))
                //     {
                //         var ints   = datatype == "int" ? new int[goo.Count] : null;
                //         var floats = ints == null ? new float[goo.Count] : null;
                //         for (int index = 0; index < goo.Count; index++)
                //         {
                //             if (goo[index].CastTo(out dval))
                //             {
                //                 if (ints != null)
                //                     ints[index] = (int)dval;
                //                 if (floats != null)
                //                     floats[index] = (float)dval;
                //             }
                //             else if (goo[index].CastTo(out ival))
                //             {
                //                 if (ints != null)
                //                     ints[index] = ival;
                //                 if (floats != null)
                //                     floats[index] = (float)ival;
                //             }
                //             else if (goo[index].CastTo(out sval))
                //             {
                //                 if (double.TryParse(sval, out dval))
                //                 {
                //                     if (ints != null)
                //                         ints[index] = (int)dval;
                //                     if (floats != null)
                //                         floats[index] = (float)dval;
                //                 }
                //             }
                //         }
                //         if (ints != null && ints.Length > 0)
                //             pipelineData.AddAttribute(varname, location, ints);
                //         if (floats != null && floats.Length > 0)
                //             pipelineData.AddAttribute(varname, location, floats);
                //     }
                //     if (goo.Count < 1 && datatype == "int")
                //     {
                //         var int_destination = new List<int>();
                //         if (data.GetDataList(i, int_destination) && int_destination.Count > 0)
                //         {
                //             int[] ints = int_destination.ToArray();
                //             if (ints != null && ints.Length > 0)
                //                 pipelineData.AddAttribute(varname, location, ints);
                //         }

                //     }
                // }
                // if (datatype == "vec2" )
                // {
                //     //vec2 -> point2d
                //     var vec2_array = GooListToPoint2fArray(paramData);
                //     if (vec2_array != null)
                //         pipelineData.AddAttribute(varname, location, vec2_array);
                // }
                // if (datatype == "vec3")
                // {
                //     //vec3 -> point3d
                //     var vec3_array = GooListToPoint3fArray(paramData);
                //     if( vec3_array!=null )
                //         pipelineData.AddAttribute(varname, location, vec3_array);
                // }
                // if (datatype == "vec4")
                // {
                //     var destination = new List<SD.Color>();
                //     if (data.GetDataList(i, destination))
                //     {
                //         GL_Vec4[] vec4_array = new GL_Vec4[destination.Count];
                //         for (int index = 0; index < destination.Count; index++)
                //         {
                //             var color = new RD.Color4f(destination[index]);
                //             vec4_array[index] = new GL_Vec4(color.R, color.G, color.B, color.A);
                //         }
                //         pipelineData.AddAttribute(varname, location, vec4_array);
                //     }
                // }
                // if (datatype == "mat4")
                // {
                //     var destination = new List<ON.Transform>();
                //     if (data.GetDataList(i, destination))
                //     {
                //         GL_Mat4[] mat4_array = new GL_Mat4[destination.Count];
                //         for (int index = 0; index < destination.Count; index++)
                //         {
                //             mat4_array[index] = new GL_Mat4(destination[index]);
                //         }
                //         pipelineData.AddAttribute(varname, location, mat4_array);
                //     }
                // }

                // continue;
            }

            // If we get here, we don't have a reference to this input in our code yet.
            // See if the input is an upstream texture since we know how to handle those
            if (paramData.Count>0 && paramData[0].CastTo(out string upstreamSampler))
            {
                // see if path refers to a component's output parameter
                if (upstreamSampler.EndsWith(":color") || upstreamSampler.EndsWith(":depth"))
                {
                    string id = upstreamSampler.Substring(0, upstreamSampler.IndexOf(":"));
                    bool isComponentInput = Guid.TryParse(id, out Guid compId);
                    if (isComponentInput)
                        pipelineData.AddSampler2DUniform(varname, upstreamSampler);
                }
            }

        }
    }

    public override void DrawViewportWires (GK.IGH_PreviewArgs args)
    {
        Scheduler.AddToRenderList (Pipeline);
    }

    protected void ReportErrors (string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            return;
        foreach (var line in errorMessage.Split('\n'))
            AddRuntimeMessage(GK.GH_RuntimeMessageLevel.Error, line.Trim());
    }


    // RequiredUpstreamComponents ()
    // // ------------------------------------------------------------------
    // // If this component uses texture inputs that are the result of other
    // // components, then those other components must be drawn before this
    // // component (they are "upstream")

    // // ???: This function is called inside the render loop, it seems unnecessary
    // //      Renderer.EnableConduit() > Rhino PostDrawObjects event
    // //          > Scheduler.OnDrawObjects() > Scheduler.SortComponents()
    // internal HashSet <GHGL_ShaderComponentBase> RequiredUpstreamComponents ()
    // {
    //     var upstream = new HashSet <GHGL_ShaderComponentBase> ();

    //     // Just assume that all sampler inputs are the same across iterations.
    //     var uniforms = _pipeline.GetIterationData (0);
    //     var samplers = uniforms.GetComponentSamplers ();
    //     var ids = new HashSet <Guid> ();
    //     foreach(var sampler in samplers)
    //     {
    //         string id = sampler.Substring (0, sampler.IndexOf (":"));
    //         if (Guid.TryParse (id, out Guid componentId))
    //             ids.Add (componentId);
    //     }
    //     foreach(var id in ids)
    //     {
    //         foreach (var component in GH_Scheduler.RenderList)
    //         {
    //             if (component == this)
    //                 continue;
    //             if (component.InstanceGuid == id)
    //                 upstream.Add (component);
    //         }
    //     }
    //     
    //     return upstream;
    // }


    #endregion


    #region Editor


    UI.ProgramEditorForm? _activeEditor;

    void OpenEditor ()
    {
        if (_activeEditor!=null)
        {
            _activeEditor.Focus();
            return;
        }
        string savedVS = _model.VertexShaderCode;
        string savedGS = _model.GeometryShaderCode;
        string savedTC = _model.TessellationControlCode;
        string savedTE = _model.TessellationEvalualtionCode;
        string savedFS = _model.FragmentShaderCode;
        string savedXfrmFeedbackVertex = _model.TransformFeedbackShaderCode;

        _activeEditor = new UI.ProgramEditorForm(_model, NickName);

        var parent = RUI.Runtime.PlatformServiceProvider.Service.GetEtoWindow(GH.Instances.DocumentEditor.Handle);
        _model.Modified = false;
        _activeEditor.Closed += (s, e) =>
        {
            var dlg = _activeEditor;
            _activeEditor = null;
            if (!dlg.Canceled)
            {
                if (_model.Modified)
                {
                    var doc = OnPingDocument();
                    doc?.Modified();
                }
            }
            else
            {
                _model.VertexShaderCode = savedVS;
                _model.GeometryShaderCode = savedGS;
                _model.FragmentShaderCode = savedFS;
                _model.TessellationControlCode = savedTC;
                _model.TessellationEvalualtionCode = savedTE;
                _model.TransformFeedbackShaderCode = savedXfrmFeedbackVertex;
            }
            _model.Modified = false;
            //recompile shader if necessary
            if (_model.ProgramId == 0)
                ExpireSolution(true);
        };
        _activeEditor.Owner = parent;
        _activeEditor.Show();
    }


    #endregion


    #region Export


    void ExportToHTML ()
    {
        var saveDlg = new Eto.Forms.SaveFileDialog();
        saveDlg.Filters.Add (new Eto.Forms.FileFilter("HTML file", new string[] { "html" }));
        var parent = RUI.Runtime.PlatformServiceProvider.Service.GetEtoWindow (GH.Instances.DocumentEditor.Handle);
        var ghDocPath = OnPingDocument ()?.FilePath;
        if( !string.IsNullOrWhiteSpace (ghDocPath))
        {
            string path = System.IO.Path.GetDirectoryName (ghDocPath);
            string filename = System.IO.Path.GetFileNameWithoutExtension (ghDocPath);
            path = System.IO.Path.Combine (path, filename + ".html");
            saveDlg.FileName = path;
        }
        if (saveDlg.ShowDialog (parent) == Eto.Forms.DialogResult.Ok)
        {
            IO.HtmlExporter.Export (saveDlg.FileName, this);
        }
    }


    #endregion

}



[Guid("E61CC873-5643-4154-B97F-3A743BE90AE8")]
public class GHGL_ShaderComponent : GHGL_ShaderComponentBase
{
    protected override SD.Bitmap Icon
    {
        get
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream("ghgl.resources.GLSL_Component_24x24.png");
            return new System.Drawing.Bitmap(stream);
        }
    }


    public GHGL_ShaderComponent() : base("GL Shader", "GL Shader", "OpenGL Drawing with a shader")
    {
        Program.VertexShaderCode = """
        #version 330

        layout(location = 0) in vec3 vertex;
        layout(location = 1) in vec4 vcolor;

        uniform mat4 _worldToClip;
        out vec4 vertex_color;

        void main() {
            vertex_color = vcolor;
            gl_Position = _worldToClip * vec4(vertex, 1.0);
        }
        """;

        Program.FragmentShaderCode = """
        #version 330

        in vec4 vertex_color;
        out vec4 fragment_color;

        void main() {
            fragment_color = vertex_color;
        }
        """;
    }


    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddScriptVariableParameter("vertex", "vertex", "", GK.GH_ParamAccess.list);
        pManager.AddScriptVariableParameter("vcolor", "vcolor", "", GK.GH_ParamAccess.list);
    }

}



[Guid("FF4EB1F7-7AD6-47CD-BD2D-C50C87E13703")]
public class GHGL_MeshShaderComponent : GHGL_ShaderComponentBase
{
    protected override SD.Bitmap Icon
    {
        get {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream("ghgl.resources.GLSL_MeshComponent_24x24.png");
            return new SD.Bitmap(stream);
        }
    }


    public GHGL_MeshShaderComponent() : base("GL Mesh Shader", "GL Mesh", "OpenGL Drawing mesh with a shader")
    {
        Program.DrawMode = OpenGL.GL_TRIANGLES;
        Program.VertexShaderCode = """
        #version 330

        layout(location = 0) in vec3 _meshVertex;
        layout(location = 1) in vec3 _meshNormal;

        uniform mat4 _worldToClip;
        out vec3 normal;

        void main() {
            normal = _meshNormal;
            gl_Position = _worldToClip * vec4(_meshVertex , 1.0);
        }
        """;

        Program.FragmentShaderCode = """
        #version 330

        uniform vec3 _lightDirection[4];
        uniform mat3 _worldToCameraNormal;

        in  vec3 normal;
        out vec4 fragment_color;

        void main() {
            vec3 l = normalize(_lightDirection[0]);
            vec3 camNormal = _worldToCameraNormal * normal;
            float intensity = dot(l, normalize(camNormal.xyz));
            vec4 diffuse = vec4(1.0, 0.0, 1.0, 1.0);

            vec3 ambient = vec3(0.1, 0.1, 0.1) * diffuse.rgb;
            vec3 c = ambient + diffuse.rgb * abs(intensity);
            fragment_color = vec4(c, diffuse.a);
        }
        """;
    }


    protected override void RegisterInputParams (GH_InputParamManager pManager)
    {
        // pManager.AddMeshParameter("Mesh", "M", "Input Mesh", GK.GH_ParamAccess.list);
        pManager.AddParameter (new TrackerParam_Mesh (), "Mesh", "M", "Input Mesh", GK.GH_ParamAccess.list);
    }

    public override bool CanInsertParameter (GK.GH_ParameterSide side, int index)
    {
        return base.CanInsertParameter (side, index) && index > 0;
    }


    protected override void SolveInstance (GK.IGH_DataAccess data)
    {
        SolveInstanceHelper (data, 1);
        var list = new List<ON.Mesh>();
        if (data.GetDataList (0, list))
        {
            var iterationData = Pipeline.GetIterationData (data.Iteration);
            foreach(var mesh in list)
                iterationData.AddMesh (mesh);
        }
    }
}

