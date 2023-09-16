using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;


namespace RhGL.Rhino;


public enum ShaderType
{
    Vertex              = 0,
    TessellationControl = 1,
    TessellationEval    = 2,
    Geometry            = 3,
    TransformFeedback   = 4,
    Fragment            = 5,
}


public class ShaderData : System.ComponentModel.INotifyPropertyChanged
{

    public ShaderData (ShaderType type)
    {
        ShaderType = type;
    }


    public ShaderType ShaderType { get; }


    List<CompileError> _compileErrors = new ();
    
    public IReadOnlyList<CompileError> CompileErrors {  get => _compileErrors; }

    public void ClearCompileErrors ()
    {
        _compileErrors.Clear ();
    }

    public void AddCompileError (CompileError error)
    {
        _compileErrors.Add (error);
    }


    uint _shaderId; // OpenGL ID for this shader
    public uint ShaderId
    {
        get { return _shaderId; }
        set
        {
            if (_shaderId != value)
            {
                Scheduler.AddShaderToDeleteList(_shaderId);
                _shaderId = value;
                // _uniforms = null;
                // _vertexAttributes = null;
                OnPropertyChanged();
            }
        }
    }

    string _code = "";
    public string Code
    {
        get { return _code; }
        set
        {
            if (!string.Equals(_code, value, StringComparison.Ordinal))
            {
                _code = value;
                // _uniforms = null;
                // _vertexAttributes = null;
                ShaderId = 0;
                OnPropertyChanged();
            }
        }
    }

    public bool IsModified => _shaderId == 0;


    // List <UniformDescription>? _uniforms;

    // public IReadOnlyList <UniformDescription> GetUniforms ()
    // {
    //     if (_uniforms == null)
    //         _ParseUniformsAndAttributes();
    //     return _uniforms!;
    // }


    // List <AttributeDescription>? _vertexAttributes;

    // public IReadOnlyList <AttributeDescription> GetVertexAttributes ()
    // {
    //     if (_vertexAttributes == null)
    //         _ParseUniformsAndAttributes();
    //     return _vertexAttributes!;
    // }


    // void _ParseUniformsAndAttributes ()
    // {
    //     _uniforms = new List<UniformDescription>();
    //     _vertexAttributes = new List<AttributeDescription>();

    //     if (string.IsNullOrWhiteSpace(Code))
    //         return; //nothing to parse

    //     var lines = Code.Split('\n');

    //     foreach (var line in lines)
    //     {
    //         if (line.StartsWith ("uniform"))
    //         {
    //             var sub_lines = line.Split(' ', ';', '=', '[');
    //             string? type = null;
    //             string? name = null;
    //             int arrayLength = 0;
    //             for (int j = 1; j < sub_lines.Length; j++)
    //             {
    //                 if (string.IsNullOrWhiteSpace(sub_lines[j]))
    //                     continue;
    //                 if (type == null)
    //                 {
    //                     type = sub_lines[j].Trim();
    //                     continue;
    //                 }
    //                 if (name == null)
    //                 {
    //                     name = sub_lines[j].Trim();
    //                     continue;
    //                 }
    //                 if( sub_lines[j].EndsWith("]"))
    //                 {
    //                     arrayLength = int.Parse(sub_lines[j].Substring(0, sub_lines[j].Length - 1));
    //                 }
    //                 break;
    //             }
    //             if (type != null && name != null)
    //             {
    //                 _uniforms.Add (new UniformDescription (name, type, arrayLength));
    //             }
    //         }
    //         if (line.StartsWith ("layout"))
    //         {
    //             //layout(location = 0) in vec4 world_vertex;
    //             int start = line.IndexOf('=');
    //             int end = line.IndexOf(')');
    //             if (start > "layout".Length && end > start)
    //             {
    //                 var s = line.Substring(start + 1, end - (start + 1)).Trim();
    //                 int location;
    //                 if (int.TryParse(s, out location) && location >= 0)
    //                 {
    //                     start = line.IndexOf("in ", end, StringComparison.InvariantCulture);
    //                     if (start > end)
    //                     {
    //                         var items = line.Substring(start + "in ".Length).Trim().Split(' ', ';');
    //                         string datatype = items[0];
    //                         string name = items[1];
    //                         _vertexAttributes.Add (new AttributeDescription (datatype, name, location));
    //                     }
    //                 }
    //             }
    //         }
    //         if (line.StartsWith("attribute"))
    //         {
    //             var items = line.Substring("attribute ".Length).Split(new[] { ' ', ';' }, StringSplitOptions.RemoveEmptyEntries);
    //             string datatype = items[0];
    //             string name = items[1];
    //             _vertexAttributes.Add (new AttributeDescription (datatype, name));
    //         }
    //     }

    // }
    

    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

    void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? memberName = null)
    {
        PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(memberName));
    }
    
}


/// <summary>
/// Represents a single GLSL shader that gets compiled and linked with other
/// shaders to produce a program. 
/// </summary>
public class Shader : ShaderData //: System.ComponentModel.INotifyPropertyChanged
{
    public Shader(ShaderType type) : base (type)
    {}


    public bool Compile ()
    {
        if (ShaderId != 0)
            return CompileErrors.Count == 0;

        ClearCompileErrors ();

        // an empty string is considered fine
        if (string.IsNullOrWhiteSpace(Code))
            return true;

        uint rc = 0;
        string processedCode = Code;
        if( Code.Contains("#pragma glslify:") )
        {
            processedCode = GlslifyClient.GlslifyCode(Code);
        }

        if (!string.IsNullOrWhiteSpace(processedCode))
        {
            uint glShader = 0;
            switch (ShaderType)
            {
                case ShaderType.Vertex:
                    glShader = OpenGL.GL_VERTEX_SHADER;
                    break;
                case ShaderType.Geometry:
                    glShader = OpenGL.GL_GEOMETRY_SHADER;
                    break;
                case ShaderType.TessellationControl:
                    glShader = OpenGL.GL_TESS_CONTROL_SHADER;
                    break;
                case ShaderType.TessellationEval:
                    glShader = OpenGL.GL_TESS_EVALUATION_SHADER;
                    break;
                case ShaderType.Fragment:
                    glShader = OpenGL.GL_FRAGMENT_SHADER;
                    break;
            }
            uint hShader = OpenGL.glCreateShader(glShader);
            OpenGL.glShaderSource(hShader, 1, new[] { processedCode }, null!);
            OpenGL.glCompileShader(hShader);
            int success;
            OpenGL.glGetShaderiv(hShader, OpenGL.GL_COMPILE_STATUS, out success);
            if (1 != success)
            {
                int maxLength;
                OpenGL.glGetShaderiv(hShader, OpenGL.GL_INFO_LOG_LENGTH, out maxLength);
                if (maxLength > 1)
                {
                    int length;
                    var infolog = new StringBuilder(maxLength + 16);
                    OpenGL.glGetShaderInfoLog(hShader, maxLength, out length, infolog);

                    foreach (var line in infolog.ToString().Split('\n'))
                    {
                        if (string.IsNullOrWhiteSpace(line))
                            continue;
                        AddCompileError (new CompileError(line, this));
                    }
                }
                OpenGL.glDeleteShader(hShader);
            }
            else
            {
                rc = hShader;
            }
        }
        ShaderId = rc;
        return 0 != rc;
    }


    // ??? Je ne comprend pas pourquoi ne pas utiliser simplement Shader.Code ???
    public string ToWebGL1Code()
    {
        if (ShaderType != ShaderType.Vertex && ShaderType != ShaderType.Fragment)
            throw new InvalidOperationException("Only vertex and fragment shaders can be convertes to WebGL 1.0");

        var sb = new StringBuilder();
        // foreach (var attribute in GetVertexAttributes())
        // {
        //     string dataType = attribute.DataType;
        //     if (dataType == "int")
        //         dataType = "float";
        //     sb.AppendLine($"attribute {dataType} {attribute.Name};");
        // }
        if (Code.Contains("gl_VertexID"))
            sb.AppendLine("attribute float _vertex_id;");

        string processedCode = Code.Trim();
        processedCode = processedCode.Replace("gl_VertexID", "_vertex_id");

        // foreach (var uniform in GetUniforms())
        // {
        //     string dataType = uniform.DataType;
        //     if (dataType == "int")
        //         dataType = "float";

        //     sb.Append($"uniform {dataType} {uniform.Name}");
        //     if (uniform.ArrayLength > 0)
        //         sb.Append($"[{uniform.ArrayLength}]");
        //     sb.AppendLine(";");
        // }

        string[] shaderLines = processedCode.Split('\n');
        for (int i = 0; i < shaderLines.Length; i++)
        {
            string line = shaderLines[i].TrimEnd();
            string trimmed = line.Trim();
            if (trimmed.StartsWith("#version"))
                continue;
            if (trimmed.StartsWith("//"))
            {
                sb.AppendLine(line);
                continue;
            }
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                sb.AppendLine();
                continue;
            }
            if (trimmed.StartsWith("layout") || trimmed.StartsWith("attribute") || trimmed.StartsWith("uniform"))
            {
                continue;
            }
            if (trimmed.StartsWith("out "))
            {
                if( ShaderType == ShaderType.Vertex )
                    line = line.Replace("out ", "varying ");
                else
                {
                    int index = line.IndexOf("vec4") + "vec4".Length;
                    string token = line.Substring(index).Trim(new char[] { ' ', ';' });
                    for (int j = i + 1; j < shaderLines.Length; j++)
                        shaderLines[j] = shaderLines[j].Replace(token, "gl_FragColor");

                    continue;
                }
            }
            if (trimmed.StartsWith("in "))
                line = line.Replace("in ", "varying ");

            sb.AppendLine(line);
        }
        string rc = sb.ToString().Trim();
        if( rc.Contains("glslify") )
            rc = GlslifyClient.GlslifyCode (rc);
        return rc;
    }
}


