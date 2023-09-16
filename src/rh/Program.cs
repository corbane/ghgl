
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

using RhGL.IO;


namespace RhGL.Rhino;


public class Program : INotifyPropertyChanged
{
    public Program ()
    {
        for (int i = 0; i < (int)ShaderType.Fragment+1; i++)
        {
            _shaders[i] = new Shader((ShaderType)i);
            _shaders[i].PropertyChanged += OnShaderChanged;
        }
    }

    ~Program ()
    {

    }


    #region Errors

    List<CompileError> _compileErrors = new List<CompileError>();
    public void ClearCompileError ()
    {
        _compileErrors.Clear();
    }
    public void AddCompileError (string message)
    {
        _compileErrors.Add(new CompileError(message));
    }
    public CompileError[] AllCompileErrors()
    {
        List<CompileError> errors = new List<CompileError>(_compileErrors);
        foreach (var shader in _shaders)
            errors.AddRange(shader.CompileErrors);
        return errors.ToArray();
    }


    bool _compileFailed;
    public bool CompileFailed
    {
        get => _compileFailed == false && _compileErrors.Count > 0;
    }

    #endregion


    #region IO


    string? _filepath;

    public string? FilePath
    {
        get => _filepath;
        set {
            if (AssetIO.IsValidJsonAsset (value) == false)
                return;
        
            AssetIO.ImportAsXML (this, value!, importShaders: true);
            _UpdateWatcher (value, _watchfile);
            _filepath = value;
        }
    }
    
    bool _watchfile;
    public bool WatchFile
    {
        get => _watchfile;
        set {
            if (_watchfile == value) return;
            _UpdateWatcher (_filepath, value);
            _watchfile = value;
        }
    }

    void _UpdateWatcher (string? newPath, bool newWatch)
    {
        if (_filepath != newPath)
        {
            if (_filepath != null)
                AssetIO.DetachWatcher (_filepath);
            if (newPath != null)
                AssetIO.AttachWatcher (newPath, _OnFileChanged);
            return;
        }
    
        if (_filepath == null)
            return;

        if (_watchfile != newWatch)
        {
            if (newWatch)
                AssetIO.AttachWatcher (_filepath, _OnFileChanged);
            else
                AssetIO.DetachWatcher (_filepath);
        }
    }

    void _OnFileChanged (string filepath)
    {
        if (_filepath == null
        || Path.GetFileNameWithoutExtension (filepath) != Path.GetFileNameWithoutExtension (_filepath)
        ) return; // ne devrais jamais passé ici

        if (filepath == _filepath)
        {
            AssetIO.ImportAsXML (this, filepath, importShaders: false);
        }
        else if (false == AssetIO.TryGetShaderTypeFromPath (filepath, out var type))
        {
            switch (type)
            {
            case ShaderType.Vertex              :  VertexShaderCode            = File.ReadAllText (filepath); break;
            case ShaderType.TessellationControl :  TessellationControlCode     = File.ReadAllText (filepath); break;
            case ShaderType.TessellationEval    :  TessellationEvalualtionCode = File.ReadAllText (filepath); break;
            case ShaderType.Geometry            :  GeometryShaderCode          = File.ReadAllText (filepath); break;
            case ShaderType.TransformFeedback   :  TransformFeedbackShaderCode = File.ReadAllText (filepath); break;
            case ShaderType.Fragment            :  FragmentShaderCode          = File.ReadAllText (filepath); break;
            }
        }
    }


    #endregion


    #region Shader


    readonly Shader[] _shaders = new Shader[(int)ShaderType.Fragment+1];
    public IReadOnlyList <Shader> Shaders => _shaders;
    public Shader GetShader (ShaderType which)
    {
        return _shaders[(int)which];
    }
    private void OnShaderChanged (object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Code")
        {
            Modified = true;
            _SetProgramId (0);
        }
    }


    public string GetCode(ShaderType type)
    {
        return type switch
        {
            ShaderType.Vertex                  => VertexShaderCode,
            ShaderType.TessellationControl     => TessellationControlCode,
            ShaderType.TessellationEval        => TessellationEvalualtionCode,
            ShaderType.Geometry                => GeometryShaderCode,
            ShaderType.Fragment                => FragmentShaderCode,
            ShaderType.TransformFeedback => TransformFeedbackShaderCode,
            _ => "",
        };
    }
    public void SetCode(ShaderType type, string code)
    {
        switch (type)
        {
            case ShaderType.Vertex               : VertexShaderCode            = code; break;
            case ShaderType.TessellationControl  : TessellationControlCode     = code; break;
            case ShaderType.TessellationEval     : TessellationEvalualtionCode = code; break;
            case ShaderType.Geometry             : GeometryShaderCode          = code; break;
            case ShaderType.Fragment             : FragmentShaderCode          = code; break;
            case ShaderType.TransformFeedback    : TransformFeedbackShaderCode = code; break;
        }
    }
    public string TransformFeedbackShaderCode
    {
        get => _shaders[(int)ShaderType.TransformFeedback].Code;
        set => _SetCode((int)ShaderType.TransformFeedback, value);
    }
    public string VertexShaderCode
    {
        get => _shaders[(int)ShaderType.Vertex].Code;
        set => _SetCode((int)ShaderType.Vertex, value);
    }
    public string TessellationControlCode
    {
        get => _shaders[(int)ShaderType.TessellationControl].Code;
        set => _SetCode((int)ShaderType.TessellationControl, value);
    }
    public string TessellationEvalualtionCode
    {
        get => _shaders[(int)ShaderType.TessellationEval].Code;
        set => _SetCode((int)ShaderType.TessellationEval, value);
    }
    public string FragmentShaderCode
    {
        get => _shaders[(int)ShaderType.Fragment].Code;
        set => _SetCode((int)ShaderType.Fragment, value);
    }
    public string GeometryShaderCode
    {
        get => _shaders[(int)ShaderType.Geometry].Code;
        set => _SetCode((int)ShaderType.Geometry, value);
    }
    void _SetCode (int which, string v, [System.Runtime.CompilerServices.CallerMemberName] string? memberName = null)
    {
        if (false == string.Equals (_shaders[which].Code, v, StringComparison.Ordinal))
        {
            _shaders[which].Code = v;
            OnPropertyChanged (memberName);
        }
    }

    // /// <summary>
    // /// Get the data type for a uniform in this program (all shaders)
    // /// </summary>
    // /// <param name="name">name of uniform to try and get a type for</param>
    // /// <param name="dataType"></param>
    // /// <returns></returns>
    // public bool TryGetUniformType (string name, out string dataType, out int arrayLength)
    // {
    //     dataType = "";
    //     arrayLength = 0;
    //     foreach (var shader in Shaders)
    //     {
    //         var uniforms = shader.GetUniforms ();
    //         foreach (UniformDescription uni in uniforms)
    //         {
    //             if (uni.Name == name)
    //             {
    //                 dataType = uni.DataType;
    //                 arrayLength = uni.ArrayLength;
    //                 return true;
    //             }
    //         }
    //     }
    //     return false;
    // }
    // public bool TryGetAttributeType (string name, out string dataType, out int location)
    // {
    //     dataType = "";
    //     location = -1;
    //     foreach (var shader in Shaders)
    //     {
    //         var attributes = shader.GetVertexAttributes();
    //         foreach (AttributeDescription attrib in attributes)
    //         {
    //             if (attrib.Name == name)
    //             {
    //                 dataType = attrib.DataType;
    //                 location = attrib.Location;
    //                 return true;
    //             }
    //         }
    //     }
    //     return false;
    // }

    // public bool HasAnimatedUniform ()
    // {
    //     string dataType;
    //     int arrayLength;
    //     return 
    //         TryGetUniformType ("_time", out dataType, out arrayLength) ||
    //         TryGetUniformType ("_date", out dataType, out arrayLength) ||
    //         TryGetUniformType ("_mousePosition", out dataType, out arrayLength) ||
    //         TryGetUniformType ("_mouseDownPosition", out dataType, out arrayLength) ||
    //         TryGetUniformType ("_mouseState", out dataType, out arrayLength);
    // }

    #endregion


    #region Vertex Buffer options

    uint _drawMode;
    public uint DrawMode
    {
        get { return _drawMode; }
        set
        {
            if (_drawMode != value && _drawMode <= OpenGL.GL_PATCHES)
            {
                _drawMode = value;
                OnPropertyChanged();
            }
        }
    }

    ushort _patchVertrices;
    public ushort PatchVertices
    {
        get => _patchVertrices;
        set {
            // https://www.khronos.org/opengl/wiki/Tessellation
            if (0 < value && value <= 32) {
                _patchVertrices = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion


    #region Drawing Options


    // TODO: ??? OnPropertyChanged();
    public int PreviewSortOrder { get; set; } = 5;


    const double DefaultLineWidth = 3.0;
    double _glLineWidth = DefaultLineWidth;
    public double glLineWidth
    {
        get { return _glLineWidth; }
        set
        {
            if (_glLineWidth != value && value > 0)
            {
                _glLineWidth = value;
                OnPropertyChanged();
            }
        }
    }


    const double DefaultPointSize = 8.0;
    double _glPointSize = DefaultPointSize;
    public double glPointSize
    {
        get { return _glPointSize; }
        set
        {
            if (_glPointSize != value && value > 0)
            {
                _glPointSize = value;
                OnPropertyChanged();
            }
        }
    }


    bool _depthTestingEnabled = true;
    public bool DepthTestingEnabled
    {
        get { return _depthTestingEnabled; }
        set
        {
            if (_depthTestingEnabled != value)
            {
                _depthTestingEnabled = value;
                OnPropertyChanged();
            }
        }
    }


    bool _depthWritingEnabled = true;
    public bool DepthWritingEnabled
    {
        get { return _depthWritingEnabled; }
        set
        {
            if (_depthWritingEnabled != value)
            {
                _depthWritingEnabled = value;
                OnPropertyChanged();
            }
        }
    }


    #endregion


    #region INotifyPropertyChanged

    readonly DateTime _startTime = DateTime.Now;
    public bool Modified { get; set; }


    void OnPropertyChanged ([System.Runtime.CompilerServices.CallerMemberName] string? memberName = null)
    {
        PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(memberName));

        switch (memberName)
        {
        case nameof (VertexShaderCode):
        case nameof (TessellationControlCode):
        case nameof (TessellationEvalualtionCode):
        case nameof (FragmentShaderCode):
        case nameof (GeometryShaderCode):
            _SetProgramId (0);
            _compileFailed = false;
            break;
        }
    }
    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

    #endregion


    #region Serialization

    public bool Write (GH_IO.Serialization.GH_IWriter writer)
    {
        writer.SetString("VertexShader", VertexShaderCode);
        writer.SetString("GeometryShader", GeometryShaderCode);
        writer.SetString("FragmentShader", FragmentShaderCode);
        writer.SetString("TessCtrlShader", TessellationControlCode);
        writer.SetString("TessEvalShader", TessellationEvalualtionCode);
        writer.SetString("TransformFeedbackVertexShader", TransformFeedbackShaderCode);
        writer.SetDouble("glLineWidth", glLineWidth);
        writer.SetDouble("glPointSize", glPointSize);
        writer.SetInt32("DrawMode", (int)DrawMode);
        writer.SetInt32("PatchVertices", (int)PatchVertices);

        writer.SetBoolean("DepthTestingEnabled", DepthTestingEnabled);
        writer.SetBoolean("DepthWritingEnabled", DepthWritingEnabled);
        writer.SetInt32 ("PreviewSortOrder", PreviewSortOrder);
        return true;
    }
    public bool Read (GH_IO.Serialization.GH_IReader reader)
    {
        string s = "";
        VertexShaderCode            = reader.TryGetString("VertexShader", ref s) ? s : "";
        GeometryShaderCode          = reader.TryGetString("GeometryShader", ref s) ? s : "";
        FragmentShaderCode          = reader.TryGetString("FragmentShader", ref s) ? s : "";
        TessellationControlCode     = reader.TryGetString("TessCtrlShader", ref s) ? s : "";
        TessellationEvalualtionCode = reader.TryGetString("TessEvalShader", ref s) ? s : "";
        TransformFeedbackShaderCode = reader.TryGetString("TransformFeedbackVertexShader", ref s) ? s : "";
        double d                           = 0;
        if (reader.TryGetDouble("glLineWidth", ref d))
            glLineWidth = d;
        if (reader.TryGetDouble("glPointSize", ref d))
            glPointSize = d;
        int i = 0;
        if (reader.TryGetInt32("DrawMode", ref i))
            DrawMode = (uint)i;
        if (reader.TryGetInt32("PatchVertices", ref i))
            PatchVertices = (ushort)i;

        bool b = true;
        if (reader.TryGetBoolean("DepthTestingEnabled", ref b))
            DepthTestingEnabled = b;
        if (reader.TryGetBoolean("DepthWritingEnabled", ref b))
            DepthWritingEnabled = b;

        int order = PreviewSortOrder;
        if (reader.TryGetInt32 ("PreviewSortOrder", ref order))
            PreviewSortOrder = order;

        return true;
    }

    #endregion


    /*/
        GH_Component.SolveInstance / foreach params -> Progrm.TryGet[Uniform|Attribute]Type(param.name)
            Shader.[GetUniforms|GetVertexAttributes]() / if [_vertexAttributes|_uniforms]==null -> _ParseUniformsAndAttributes()
        Shader.[_vertexAttributes|_uniforms] sont mis a null lorsque Shader.Code est assigné.
    /*/

    List <AttributeDescription> _attribs  = new ();
    public IReadOnlyCollection <AttributeDescription> Attributes => _attribs;
    public bool TryGetAttributeType (string name, out GlslAttribType dataType, out int location)
    {
        var a = _attribs.Find ((a) => a.Name == name);
        if (a == null) {
            dataType = 0;
            location = 0;
            return false;
        } else {
            dataType = a.DataType;
            location = a.Location;
            return true;
        }
    }

    List <UniformDescription> _uniforms = new ();
    public IReadOnlyCollection <UniformDescription> Uniforms => _uniforms;
    public bool HasUniformName (string name)
    {
        return _uniforms.Find ((u) => u.Name == name) != null;
    }
    public bool TryGetUniformType (string name, out GlslUniformType dataType, out int arrayLength)
    {
        var u = _uniforms.Find ((u) => u.Name == name);
        if (u == null) {
            dataType    = 0;
            arrayLength = 0;
            return false;
        } else {
            dataType    = u.DataType;
            arrayLength = u.ArrayLength;
            return true;
        }
    }

    bool _hasAnimatedUniform;
    public bool HasAnimatedUniform => _hasAnimatedUniform;

    void _initializeAttributesAndUniforms ()
    {
        _attribs.Clear ();
        _uniforms.Clear ();
        _hasAnimatedUniform = false;

        int count;
        int maxNameLength;
        var nameBuilder = new System.Text.StringBuilder ();
        string name;
        int location;

        // NOTE: pour `uniform vec3 direction[5];` glGetActiveAttrib retourne le nom `direction[0]`

        OpenGL.glGetProgramiv (_programId, OpenGL.GL_ACTIVE_ATTRIBUTES, out count);
        OpenGL.glGetProgramiv (_programId, OpenGL.GL_ACTIVE_ATTRIBUTE_MAX_LENGTH, out maxNameLength);
        nameBuilder.Capacity = maxNameLength;
        for (uint i = 0; i < count; i++)
        {
            nameBuilder.Clear ();
            OpenGL.glGetActiveAttrib (_programId, i, maxNameLength, out var nameLength, out var size, out var type, nameBuilder);
            name = StringLib.RemoveArrayDescriptor (nameBuilder.ToString ());
            location = OpenGL.glGetAttribLocation (_programId, name);
            _attribs.Add (new AttributeDescription ((GlslAttribType)type, name, location));
        }
        
        OpenGL.glGetProgramiv (_programId, OpenGL.GL_ACTIVE_UNIFORMS, out count);
        OpenGL.glGetProgramiv (_programId, OpenGL.GL_ACTIVE_UNIFORM_MAX_LENGTH, out maxNameLength);
        nameBuilder.Capacity = maxNameLength;
        for (uint i = 0 ; i < count ; i++)
        {
            nameBuilder.Clear ();
            OpenGL.glGetActiveUniform (_programId, i, maxNameLength, out var nameLength, out var size, out var type, nameBuilder);
            name = StringLib.RemoveArrayDescriptor (nameBuilder.ToString ());
            location = OpenGL.glGetUniformLocation (_programId, name);
            _uniforms.Add (new UniformDescription (name, (GlslUniformType)type, size));

            switch (name)
            {
            case "_time":
            case "_date":
            case "_mousePosition":
            case "_mouseDownPosition":
            case "_mouseState":
                _hasAnimatedUniform = true;
                break;
            }
        }
    }


    #region Execution

    uint _programId;
    public uint ProgramId => _programId;
    void _SetProgramId (uint glid)
    {
        if (_programId == glid)
            return;
        if (_programId != 0)
            Scheduler.AddProgramToDeleteList (_programId);
        _programId = glid;
        OnPropertyChanged();
    }

    /*/
        - GH_Component.SolveInstance if iteration == 0 -> CompileProgram()
        - CodeEditor.TextChanged -> Debouncer -> CompileProgram()
    /*/
    public bool CompileProgram ()
    {
        if (_programId != 0)
            return true;
        if (CompileFailed)
            return false;

        Conduit.ActivateGlContext ();

        Scheduler.Recycle();

        ClearCompileError ();

        // Compile tous les shaders même s'il y a eu une erreur dès le premier.
        // Nécessaire pour extraire les messages d'erreur à afficher dans l'éditeur.
        bool compileSuccess = true;

        foreach (var shader in Shaders)
            compileSuccess = shader.Compile () && compileSuccess;

        // we want to make sure that at least a vertex and fragment shader
        // exist before making a program
        if (string.IsNullOrWhiteSpace (Shaders[(int)ShaderType.Vertex].Code))
        {
            AddCompileError ("A vertex shader is required to create a GL program");
            compileSuccess = false;
        }
        if (string.IsNullOrWhiteSpace (Shaders[(int)ShaderType.Fragment].Code))
        {
            AddCompileError ("A fragment shader is required to create a GL program");
            compileSuccess = false;
        }

        if (compileSuccess == false) return false;
        
        _SetProgramId (OpenGL.glCreateProgram ());
        foreach (var shader in Shaders)
        {
            if (shader.ShaderId != 0)
                OpenGL.glAttachShader (_programId, shader.ShaderId);
        }

        OpenGL.glLinkProgram (_programId);

        if (OpenGL.ErrorOccurred (out var errorMsg))
        {
            OpenGL.glDeleteProgram (_programId);
            _SetProgramId (0);
            AddCompileError (errorMsg);
        }

        if (_programId != 0)
            _initializeAttributesAndUniforms ();

        // Reset the start time every time a shader is compiled to ensure that any time-based shader "starts over"
        // Time should only ellapse when the shader is actually running. Until we have a way to start and stop a
        // shader at a given point, allow for edits, and then continue, picking up where we left off...this is the
        // only "easy" way of doing this for now.
        BuiltIn._startTime = DateTime.Now;
        
        return _programId != 0;
    }


    #endregion


    #region Export


    public void Save (string filename)
    {
        var text = new System.Text.StringBuilder();

        static string ShaderTypeToHeader (ShaderType type)
        {
            var s = from e in type.ToString ().Split('C', 'E', 'F', 'V')
                    select e.ToLower ();
            return "[" + string.Join (" ", s) + "]";
        }

        foreach (var shader in Shaders) {
            if(shader == null || string.IsNullOrWhiteSpace(shader.Code))
                continue;
            text.AppendLine (ShaderTypeToHeader(shader.ShaderType));
            text.AppendLine (shader.Code);
        }

        System.IO.File.WriteAllText(filename, text.ToString());
    }

    
    #endregion

}

