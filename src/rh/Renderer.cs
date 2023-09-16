using System;
using System.Collections.Generic;
using RH = Rhino;
using RD = Rhino.Display;
using SD = System.Drawing;


namespace RhGL.Rhino;


enum SamplerTextureUnit : int
{
    InitialColorBuffer = 1,
    InitialDepthBuffer = 2,
    BaseSampler = 3
    // Toute entrée de texture utilisateur commence à `OpenGL.GL_TEXTURE0 + (uint)SamplerTextureUnit.BaseSampler`
    // OpenGL.GL_TEXTURE1 et OpenGL.GL_TEXTURE2 sont réservés aux textures `:color` et `:depth`.
}


static class PerFrameCache
{
    class PerFrameLifetimeObject : IDisposable
    {
        public void Dispose()
        {
            PerFrameCache.EndFrame();
        }
    }

    static readonly Dictionary <string, IntPtr> _samplerMap = new ();


    public static IntPtr InitialColorBuffer { get; private set; }
    public static IntPtr InitialDepthBuffer { get; private set; }


    public static IDisposable BeginFrame (RD.DisplayPipeline display, IEnumerable <Pipeline> pipelines)
    {
        // check to see if any components use _colorBuffer or _depthBuffer
        bool usesInitialColorBuffer = false;
        bool usesInitialDepthBuffer = false;
        foreach (var pipeline in pipelines)
        {
            //if (!usesInitialColorBuffer && pipeline.Program.TryGetUniformType ("_colorBuffer", out string dataType, out int arrayLength))
            if (!usesInitialColorBuffer && pipeline.Program.HasUniformName ("_colorBuffer"))
                usesInitialColorBuffer = true;
            //if (!usesInitialDepthBuffer && pipeline.Program.TryGetUniformType ("_depthBuffer", out dataType, out arrayLength))
            if (!usesInitialDepthBuffer && pipeline.Program.HasUniformName ("_depthBuffer"))
                usesInitialDepthBuffer = true;
        }
        if (usesInitialColorBuffer)
        {
            IntPtr texture2dPtr = Rhino7NativeMethods.RhTexture2dCreate();
            Rhino7NativeMethods.RhTexture2dCapture (display, texture2dPtr, Rhino7NativeMethods.CaptureFormat.kRGBA);
            InitialColorBuffer = texture2dPtr;
        }
        if (usesInitialDepthBuffer)
        {
            IntPtr texture2dPtr = Rhino7NativeMethods.RhTexture2dCreate();
            Rhino7NativeMethods.RhTexture2dCapture (display, texture2dPtr, Rhino7NativeMethods.CaptureFormat.kDEPTH24);
            InitialDepthBuffer = texture2dPtr;
        }

        foreach (var texture in _samplerMap.Values)
            Scheduler.AddTextureToDeleteList (texture);
        _samplerMap.Clear ();

        // figure out list of per component depth and color textures that need to be created/retrieved
        foreach (var pipeline in pipelines)
        {
            string[] samplers = pipeline.GetIterationData(0).GetComponentSamplers ();
            foreach (var sampler in samplers)
                _samplerMap[sampler.ToLowerInvariant()] = IntPtr.Zero;
        }

        return new PerFrameLifetimeObject();
    }
    public static void EndFrame ()
    {
        Scheduler.AddTextureToDeleteList (InitialColorBuffer);
        InitialColorBuffer = IntPtr.Zero;
        Scheduler.AddTextureToDeleteList (InitialDepthBuffer);
        InitialDepthBuffer = IntPtr.Zero;
    }


    // Return true if the output color/depth buffer from a given component is used downstream
    public static bool IsColorTextureUsed (Guid guid)
    {
        string id = $"{guid}:color".ToLowerInvariant();
        return _samplerMap.ContainsKey(id);
    }
    public static bool IsDepthTextureUsed (Guid guid)
    {
        string id = $"{guid}:depth".ToLowerInvariant();
        return _samplerMap.ContainsKey(id);
    }

    public static void SaveColorTexture (Guid guid, IntPtr ptrTexture)
    {
        string id = $"{guid}:color".ToLowerInvariant();
        _samplerMap[id] = ptrTexture;
    }
    public static void SaveDepthTexture (Guid guid, IntPtr ptrTexture)
    {
        string id = $"{guid}:depth".ToLowerInvariant();
        _samplerMap[id] = ptrTexture;
    }

    public static uint GetTextureId (string name)
    {
        if(_samplerMap.TryGetValue(name.ToLowerInvariant(), out IntPtr ptrTexture))
            return Rhino7NativeMethods.RhTexture2dHandle(ptrTexture);
        return 0;
    }
    public static SD.Bitmap? GetTextureImage(Guid guid, bool colorBuffer)
    {
        string id = colorBuffer ?
            $"{guid}:color".ToLowerInvariant() :
            $"{guid}:depth".ToLowerInvariant();
        if (_samplerMap.TryGetValue(id.ToLowerInvariant(), out IntPtr ptrColorTexture))
        {
            Conduit.ActivateGlContext ();
            // GLShaderComponentBase.ActivateGlContext();
            return Rhino7NativeMethods.RhTexture2dToDib(ptrColorTexture);
        }
        return null;
    }


}


static partial class Scheduler
{
    /*/
        GH_Component.SolveInstance()
            if data.Iteration == 0 -> Pipeline.ClearIterationData()
                foreach iterationData -> PipelineData.ClearData()
                    foreach mesh -> foreach buffer -> (**Vbo|**Bufer) = 0
                        AddVboToDeleteList()
    /*/

    #region RecycleBin


    static readonly HashSet <uint> _shadersToDelete = new ();
    static readonly HashSet <uint> _programsToDelete = new ();
    static readonly HashSet <uint> _vbosToDelete = new ();
    static readonly HashSet <uint> _texturesToDelete = new ();
    static readonly HashSet <IntPtr> _texturePtrsToDelete = new ();

    public static void AddShaderToDeleteList (uint shader)
    {
        if (0 != shader)
            _shadersToDelete.Add(shader);
    }
    public static void AddProgramToDeleteList (uint program)
    {
        if (0 != program)
            _programsToDelete.Add(program);
    }
    public static void AddVboToDeleteList (uint vbo)
    {
        if (0 != vbo)
            _vbosToDelete.Add(vbo);
    }
    public static void AddTextureToDeleteList (uint textureId)
    {
        if (0 != textureId)
            _texturesToDelete.Add(textureId);
    }
    public static void AddTextureToDeleteList (IntPtr texture2dPtr)
    {
        if (texture2dPtr != System.IntPtr.Zero)
            _texturePtrsToDelete.Add(texture2dPtr);
    }

    public static void Recycle ()
    {
        foreach (var vbo in _vbosToDelete)
            OpenGL.glDeleteBuffers (1, new[] { vbo });
        foreach (var texture in _texturesToDelete)
            OpenGL.glDeleteTextures (1, new[] { texture });
        foreach (var texturePtr in _texturePtrsToDelete)
            Rhino7NativeMethods.RhTexture2dDelete(texturePtr);
        foreach (var shader in _shadersToDelete)
            OpenGL.glDeleteShader(shader);
        foreach (var program in _programsToDelete)
            OpenGL.glDeleteProgram (program);

        _shadersToDelete.Clear();
        _programsToDelete.Clear();
        _vbosToDelete.Clear();
        _texturesToDelete.Clear();
        _texturePtrsToDelete.Clear();
    }
    

    #endregion


    #region Render Functions


    static readonly List <Pipeline> _renderList = new ();
    public static IReadOnlyList <Pipeline> RenderList => _renderList;
    
    public static void AddToRenderList (Pipeline pipeline)
    {
        Conduit.AnimationTimerEnabled = false;
        foreach (var component in _renderList) {
            if (pipeline == component)
                return;
        }
        _renderList.Add (pipeline);
        SortComponents ();
    }

    public static void SortComponents ()
    {
        _renderList.Sort((x, y) => {
            var xUpstream = x.RequiredUpstreamComponents ();
            var yUpstream = y.RequiredUpstreamComponents ();

            foreach(var component in xUpstream)
            {
                // if x depends on y, then y must be drawn first
                if (component == y)
                    return 1;
            }
            foreach(var component in yUpstream)
            {
                // if y depends on x, then x must be drawn first
                if (component == x)
                    return -1;
            }

            if (x.Program.PreviewSortOrder < y.Program.PreviewSortOrder)
                return -1;
            if (x.Program.PreviewSortOrder > y.Program.PreviewSortOrder)
                return 1;
            return 0;
        });
    }

    public static void OnDrawObjects (object sender, RD.DrawEventArgs args)
    {
        if (_renderList.Count < 1)
            return;

        // Devrais être dans RH_Renderer
        if (!OpenGL.Initialized)
            OpenGL.Initialize();
        if (!OpenGL.IsAvailable)
            return;

        Conduit.UpdateContext (args);
        // 

        SortComponents ();
        using (IDisposable lifetimeObject = PerFrameCache.BeginFrame (args.Display, RenderList))
        {
            foreach (var pipeline in _renderList)
            {
                if (pipeline.Hidden)
                    continue;

                if ((UI.ProgramEditorForm.EditorsOpen == false) &&
                    (Conduit.AnimationTimerEnabled == false) &&
                    pipeline.Program.HasAnimatedUniform)
                    // pipeline.Program.HasAnimatedUniform ())
                    Conduit.AnimationTimerEnabled = true;
                
                // _DrawPipeline (args.Display, pipeline);
                pipeline.Draw (args.Display);
            }
        }
        Scheduler.Recycle();
        _renderList.Clear();
    }

    // static void _DrawPipeline (RD.DisplayPipeline display, Pipeline pipeline)
    // {
    //     var prog = pipeline.Program;

    //     uint programId = prog.ProgramId;
    //     if (programId == 0)
    //         return;

    //     bool currentDepthTestingEnabled = OpenGL.IsEnabled(OpenGL.GL_DEPTH_TEST);
    //     if (currentDepthTestingEnabled != prog.DepthTestingEnabled)
    //     {
    //         if (prog.DepthTestingEnabled)
    //             OpenGL.glEnable (OpenGL.GL_DEPTH_TEST);
    //         else
    //             OpenGL.glDisable (OpenGL.GL_DEPTH_TEST);
    //     }
    //     if (!prog.DepthWritingEnabled)
    //         OpenGL.glDepthMask ((byte)OpenGL.GL_FALSE);

    //     OpenGL.glGenVertexArrays (1, out var vao);
    //     OpenGL.glBindVertexArray (vao[0]);
    //     OpenGL.glUseProgram (programId);

    //     // TODO: Parse shader and figure out the proper number to place here
    //     if (OpenGL.GL_PATCHES == prog.DrawMode)
    //         OpenGL.glPatchParameteri (OpenGL.GL_PATCH_VERTICES, prog.PatchVertices);

    //     OpenGL.glLineWidth ((float)prog.glLineWidth);
    //     OpenGL.glPointSize ((float)prog.glPointSize);

    //     // Define standard uniforms
    //     foreach (var builtin in BuiltIn.GetUniformBuiltIns ())
    //         builtin.Setup (programId, display);

    //     if (OpenGL.GL_POINTS == prog.DrawMode)
    //         OpenGL.glEnable(OpenGL.GL_VERTEX_PROGRAM_POINT_SIZE);
    //     OpenGL.glEnable(OpenGL.GL_BLEND);
    //     OpenGL.glBlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);

    //     foreach (var iteration in pipeline.IterationData)
    //         iteration.Draw (display, programId, prog.DrawMode);

    //     OpenGL.glDisable (OpenGL.GL_BLEND);
    //     OpenGL.glBindVertexArray (0);
    //     OpenGL.glDeleteVertexArrays (1, vao);
    //     OpenGL.glUseProgram (0);

    //     if (currentDepthTestingEnabled != prog.DepthTestingEnabled)
    //     {
    //         if (currentDepthTestingEnabled)
    //             OpenGL.glEnable(OpenGL.GL_DEPTH_TEST);
    //         else
    //             OpenGL.glDisable(OpenGL.GL_DEPTH_TEST);
    //     }
    //     if (!prog.DepthWritingEnabled)
    //         OpenGL.glDepthMask ((byte)OpenGL.GL_TRUE);

    //     // capture output color and depth buffer if they are needed downstream
    //     bool saveColor = true;//            PerFrameCache.IsColorTextureUsed(component);
    //     if( saveColor )
    //     {
    //         IntPtr texture2dPtr = Rhino7NativeMethods.RhTexture2dCreate();
    //         if (Rhino7NativeMethods.RhTexture2dCapture(display, texture2dPtr, Rhino7NativeMethods.CaptureFormat.kRGBA))
    //             PerFrameCache.SaveColorTexture (pipeline.Guid, texture2dPtr);
    //     }
    //     bool saveDepth = true;//            PerFrameCache.IsDepthTextureUsed(component);
    //     if(saveDepth)
    //     {
    //         IntPtr texture2dPtr = Rhino7NativeMethods.RhTexture2dCreate();
    //         if (Rhino7NativeMethods.RhTexture2dCapture(display, texture2dPtr, Rhino7NativeMethods.CaptureFormat.kDEPTH24))
    //             PerFrameCache.SaveDepthTexture (pipeline.Guid, texture2dPtr);
    //     }
    // }


    #endregion

}


static class Conduit
{

    #region OpenGL Context initialization


    static bool _initialization_tried;
    static IntPtr _hglrc;
    static uint _viewSerialNumber;

    public static bool InitializationAttempted => _initialization_tried;

    public static void Initialize ()
    {
        if (_initialization_tried)
            return;
        _initialization_tried = true;

        RD.DisplayPipeline.DrawForeground += _InitGL_OnDrawForeground;
        if (RH.RhinoDoc.ActiveDoc is RH.RhinoDoc doc) doc.Views.Redraw ();
    }
    
    // One time setup function to get the OpenGL functions initialized for use
    static void _InitGL_OnDrawForeground (object sender, RD.DrawEventArgs e)
    {
        RD.DisplayPipeline.DrawForeground -= _InitGL_OnDrawForeground;
        if (OpenGL.Initialized == false)
            OpenGL.Initialize();

        _hglrc = OpenGL.wglGetCurrentContext();
        var view = e.Display.Viewport.ParentView;
        if (view == null)
            view = e.RhinoDoc.Views.ActiveView;
        _viewSerialNumber = view.RuntimeSerialNumber;
    }

    public static bool ActivateGlContext ()
    {
        if (OpenGL.IsAvailable == false)
            return false;

        // just assume GL context is active for now
        if (RH.Runtime.HostUtils.RunningOnOSX)
            return true;

        if (OpenGL.wglGetCurrentContext() != IntPtr.Zero)
            return true;

        if (IntPtr.Zero == _hglrc)
            return false;

        RD.RhinoView view = RD.RhinoView.FromRuntimeSerialNumber(_viewSerialNumber);
        if (null == view)
        {
            _hglrc = IntPtr.Zero;
            _viewSerialNumber = 0;
            return false;
        }
        var hwnd = view.Handle;
        var hdc = OpenGL.GetDC(hwnd);
        OpenGL.wglMakeCurrent (hdc, _hglrc);
        return true;
    }

    public static void UpdateContext (RD.DrawEventArgs args)
    {
        _hglrc = OpenGL.wglGetCurrentContext();
        _viewSerialNumber
            = args.Display.Viewport.ParentView is null ? 0 // case of GhCanvasViewport
            : args.Display.Viewport.ParentView.RuntimeSerialNumber;
    }


    #endregion


    #region Rhino Conduit activation


    static bool _conduitEnabled = false;

    public static bool IsEnabled { get => _conduitEnabled; }

    public static void EnableConduit ()
    {
        RD.DisplayPipeline.PostDrawObjects += Scheduler.OnDrawObjects;
        _conduitEnabled = true;
    }


    static IdleRedraw? _idleRedraw;

    /// <summary>
    /// Helper class used for OnIdle redrawing when animations are enabled
    /// </summary>
    class IdleRedraw
    {
        public void PerformRedraw (object sender, EventArgs e)
        {
            if (RH.RhinoDoc.ActiveDoc is RH.RhinoDoc doc)
                doc.Views.Redraw();
        }
    }

    public static bool AnimationTimerEnabled
    {
        get
        {
            return _idleRedraw != null;
        }
        set
        {
            if( value )
            {
                if (_idleRedraw == null)
                {
                    _idleRedraw = new IdleRedraw();
                    RH.RhinoApp.Idle += _idleRedraw.PerformRedraw;
                }
            }
            else
            {
                if(_idleRedraw != null)
                {
                    RH.RhinoApp.Idle -= _idleRedraw.PerformRedraw;
                    _idleRedraw = null;
                }
            }
        }
    }


    #endregion

}
