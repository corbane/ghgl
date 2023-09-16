using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using RhGL.Rhino;


namespace RhGL.IO;


record Range (int Position, int Length);


static class MdExporter
{
    
    public static void Write (Program proram, string filename)
    {
        static bool IsNotAvailable (Shader shader) {
            return shader.Code == null || string.IsNullOrWhiteSpace (shader.Code);
        }

        static string ShaderTypeToHeader (ShaderType type) => type switch
        {
            ShaderType.Vertex
                => "\n## Vertex shader\n\n```glsl { type=VS }\n",

            ShaderType.TessellationControl
                => "\n## Tessellation control shader\n\n```glsl { type=TC }\n",

            ShaderType.TessellationEval
                => "\n## Tessellation eval\n\n```glsl { type=TE }\n",

            ShaderType.Geometry
                => "\n## Geometry shader\n\n```glsl { type=GS }\n",

            ShaderType.TransformFeedback
                => "\n## Transform feedback vertex shader\n\n```glsl { type=FB }\n",

            ShaderType.Fragment
                => "\n## Fragment shader\n\n```glsl { type=FS }\n",
            _ 
                => "\n\n## \n```glsl\n",
        };

        var text = new System.Text.StringBuilder();

        text.Append ("---\n");
        text.Append ($"{nameof (proram.DrawMode)}: {proram.DrawMode}\n");
        text.Append ($"{nameof (proram.PatchVertices)}: {proram.PatchVertices}\n");
        text.Append ($"{nameof (proram.glLineWidth)}: {proram.glLineWidth}\n");
        text.Append ($"{nameof (proram.glPointSize)}: {proram.glPointSize}\n");
        text.Append ($"{nameof (proram.DepthTestingEnabled)}: {proram.DepthTestingEnabled}\n");
        text.Append ($"{nameof (proram.DepthWritingEnabled)}: {proram.DepthWritingEnabled}\n");
        text.Append ("---\n");

        foreach (var shader in proram.Shaders)
        {
            if(IsNotAvailable (shader)) continue;
            text.Append (ShaderTypeToHeader (shader.ShaderType));
            text.Append (shader.Code);
            text.Append ("```\n");
        }
        
        System.IO.File.WriteAllText (filename, text.ToString());
    }


    public static void Read (Program program, string filepath)
    {
        var reCodeblock = new Regex (@"^(?:`{3,}|~{3,})glsl\s*(?:\{\s*type\s*=\s*([A-Z]+)\s*\})?");

        var lines = System.IO.File.ReadAllLines (filepath);

        var count = lines.Length;
        if (count == 0) return;
        
        var l = lines[0];
        var i = 0;
        if (l.StartsWith ("---"))
        {
            i = 1;
            while (i < count)
            {
                l = lines[i].Trim ();
                i++;
                if (l.StartsWith ("---")) break;

                var parts = l.Split (':');
                if (parts.Length != 2) continue;

                switch (parts[0].Trim ())
                {
                case nameof(program.DrawMode)            : setDrawMode (parts[1]); break;
                case nameof(program.PatchVertices)       : setPatchVertices (parts[1]); break;
                case nameof(program.glLineWidth)         : setglLineWidth (parts[1]); break;
                case nameof(program.glPointSize)         : setglPointSize (parts[1]); break;
                case nameof(program.DepthTestingEnabled) : setDepthTestingEnabled (parts[1]); break;
                case nameof(program.DepthWritingEnabled) : setDepthWritingEnabled (parts[1]); break;
                }
            }
            
            void setDrawMode            (string s) { if (uint.TryParse (s, out var v))    program.DrawMode            = v; }
            void setPatchVertices       (string s) { if (ushort.TryParse (s, out var v))  program.PatchVertices       = v; }
            void setglLineWidth         (string s) { if (int.TryParse (s, out var v))     program.glLineWidth         = v; }
            void setglPointSize         (string s) { if (double.TryParse (s, out var v))  program.glPointSize         = v; }
            void setDepthTestingEnabled (string s) { if (bool.TryParse (s, out var v))    program.DepthTestingEnabled = v; }
            void setDepthWritingEnabled (string s) { if (bool.TryParse (s, out var v))    program.DepthWritingEnabled = v; }
        }

        Match m;
        var codeLines = new List <string> ();
        for ( ; i < count ; i++)
        {
            l = lines[i];
            m = reCodeblock.Match (l);
            if (false == m.Success) continue;

            if (m.Groups[1] is null
            ||  m.Groups[1].Value is not string shaderType) continue;

            codeLines.Clear ();
            for (i++ ; i < count ; i++)
            {
                l = lines[i];
                if (l.StartsWith ("```")
                || l.StartsWith ("~~~")) break;

                codeLines.Add (l);
            }

            if (codeLines.Count () == 0) break;

            var code = string.Join ("\n", codeLines);
            switch (shaderType)
            {
            case "VS": program.VertexShaderCode            = code; break;
            case "TC": program.TessellationControlCode     = code; break;
            case "TE": program.TessellationEvalualtionCode = code; break;
            case "GS": program.GeometryShaderCode          = code; break;
            case "FB": program.TransformFeedbackShaderCode = code; break;
            case "FS": program.FragmentShaderCode          = code; break;
            }
        }
    }


    static List <Range> _GetRanges (string[] lines)
    {
        var ranges = new List <Range> ();
        return ranges;
    }
}