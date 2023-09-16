using System.Collections.Generic;

using GK = Grasshopper.Kernel;

using ON = Rhino.Geometry;
using RH = Rhino;

using RhGL.Rhino;


namespace RhGL.IO;

static class HtmlExporter
{
    
    class GuiItem
    {
        public string Name { get; set; }
        public float Value { get; set; }
        public float Minimum { get; set; }
        public float Maximum { get; set; }
        public GL_Vec4 Color { get; set; }
        public bool IsColor { get; set; }
        public GuiItem (string name) { Name = name; }
    }

    public static void Export (string filename, Grasshopper.GHGL_ShaderComponentBase component)
    {
        var Pipeline = component.Pipeline;
        var Program = Pipeline.Program;
        var IterationData = Pipeline.IterationData;

        string baseFilename = System.IO.Path.GetFileNameWithoutExtension(filename);
        string dirName = System.IO.Path.GetDirectoryName(filename);

        // create the javascript companion file that contains the attribute data
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("var ghglAttributes = [");

        int meshCount = 0;
        if (IterationData.Count > 0)
            meshCount = IterationData[0].Meshes.Count;

        int count = meshCount > 0 ? meshCount : IterationData.Count;
        for( int i=0; i<count; i++ )
        {
            if (i > 0)
                sb.AppendLine("  , {");
            else
                sb.AppendLine("  {");

            PipelineData iterationData = meshCount>0 ?
                IterationData[0] : 
                IterationData[i];

            List<string> chunks = new List<string>();

            // var allUniforms = new List<UniformDescription>(Program.Shaders[(int)ShaderType.Vertex].GetUniforms());
            // allUniforms.AddRange(Program.Shaders[(int)ShaderType.Fragment].GetUniforms());
            var builtInDictionary = new Dictionary<string, UniformDescription>();
            // foreach (var uniform in allUniforms)
            foreach (var uniform in Program.Uniforms)
            {
                builtInDictionary[uniform.Name] = uniform;
            }
            var builtInsUsed = new List<UniformDescription>(builtInDictionary.Values);
            chunks.Add(iterationData.UniformsToJsonString(2, builtInsUsed));

            if( meshCount>0 )
            {
                var meshData = iterationData.Meshes[i];

                var indices = new int[3 * (meshData.Mesh.Faces.TriangleCount + 2 * meshData.Mesh.Faces.QuadCount)];
                int current = 0;
                foreach (var face in meshData.Mesh.Faces)
                {
                    indices[current++] = face.A;
                    indices[current++] = face.B;
                    indices[current++] = face.C;
                    if (face.IsQuad)
                    {
                        indices[current++] = face.C;
                        indices[current++] = face.D;
                        indices[current++] = face.A;
                    }
                }
                chunks.Add (_BufferToJsonString ("meshIndices", new List<int>(indices), 2, true));

                if( meshData.VertexVbo != 0)
                {
                    var pts = new List<ON.Point3f>(meshData.Mesh.Vertices.ToPoint3fArray());
                    chunks.Add (_BufferToJsonString ("_meshVertex", pts, 2));
                }
                if( meshData.NormalVbo != 0)
                {
                    var normals = new List<ON.Vector3f>(meshData.Mesh.Normals.Count);
                    foreach (var normal in meshData.Mesh.Normals)
                        normals.Add(normal);
                    chunks.Add (_BufferToJsonString ("_meshNormal", normals, 2));
                }
                if( meshData.TextureCoordVbo != 0)
                {
                    var tcs = new List<ON.Point2f>(meshData.Mesh.TextureCoordinates.Count);
                    foreach (var tc in meshData.Mesh.TextureCoordinates)
                        tcs.Add(tc);
                    chunks.Add (_BufferToJsonString ("_meshTextureCoordinate", tcs, 2));
                }
                if(meshData.ColorVbo != 0)
                {
                    List<GL_Vec4> colors = new List<GL_Vec4>(meshData.Mesh.VertexColors.Count);
                    foreach( var color in meshData.Mesh.VertexColors)
                    {
                        GL_Vec4 v4 = new GL_Vec4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
                        colors.Add(v4);
                    }
                    chunks.Add(_BufferToJsonString("_meshVertexColor", colors, 2));
                }
            }

            if (Program.VertexShaderCode.Contains ("gl_VertexID"))
            {
                int length = _MinAttributeLength(iterationData);
                int[] items = new int[length];
                for (int j = 0; j < items.Length; j++)
                    items[j] = j;
                Attribute<int> vertexIds = new Attribute<int>("_vertex_id", -1, items);
                chunks.Add(vertexIds.ToJsonString(2));
            }

            foreach (var attr in iterationData.IntAttribs)
            {
                string attrAsJson = attr.ToJsonString(2);
                if (!string.IsNullOrWhiteSpace(attrAsJson))
                    chunks.Add(attrAsJson);
            }
            foreach (var attr in iterationData.FloatAttribs)
            {
                string attrAsJson = attr.ToJsonString(2);
                if (!string.IsNullOrWhiteSpace(attrAsJson))
                    chunks.Add(attrAsJson);
            }
            foreach (var attr in iterationData.Vec2Attribs)
            {
                string attrAsJson = attr.ToJsonString(2);
                if (!string.IsNullOrWhiteSpace(attrAsJson))
                    chunks.Add(attrAsJson);
            }
            foreach (var attr in iterationData.Vec3Attribs)
            {
                string attrAsJson = attr.ToJsonString(2);
                if (!string.IsNullOrWhiteSpace(attrAsJson))
                    chunks.Add(attrAsJson);
            }
            foreach (var attr in iterationData.Vec4Attribs)
            {
                string attrAsJson = attr.ToJsonString(2);
                if (!string.IsNullOrWhiteSpace(attrAsJson))
                    chunks.Add(attrAsJson);
            }
            foreach (var attr in iterationData.Mat4Attribs)
            {
                string attrAsJson = attr.ToJsonString(2);
                if (!string.IsNullOrWhiteSpace(attrAsJson))
                    chunks.Add(attrAsJson);
            }
            for (int j = 0; j < chunks.Count; j++)
            {
                sb.Append(chunks[j]);
                if (j < (chunks.Count - 1))
                    sb.Append(",");
                sb.AppendLine();
            }
            sb.AppendLine("  }");
        }

        sb.AppendLine("];");
        string jsfilepath = System.IO.Path.Combine(dirName, baseFilename + ".js");
        string javascriptData = sb.ToString();
        System.IO.File.WriteAllText(jsfilepath, javascriptData);
        
        System.Reflection.Assembly a = typeof(Pipeline).Assembly;
        var stream = a.GetManifestResourceStream("ghgl.resources.threejs_template.html");
        var sr = new System.IO.StreamReader(stream);
        string contents = sr.ReadToEnd();
        stream.Close();

        contents = contents.Replace("_JAVASCRIPT_FILENAME_", baseFilename + ".js");

        sb = new System.Text.StringBuilder();
        bool positionHandled = false;
        string positionFiller = "";
        var attrs = IterationData[0];
        if (Program.VertexShaderCode.Contains("gl_VertexID"))
        {
            sb.AppendLine("      geometry.addAttribute('_vertex_id', new THREE.BufferAttribute( ghglAttributes[i]._vertex_id, 1 ));");
            positionFiller = "      geometry.addAttribute('position', new THREE.BufferAttribute( ghglAttributes[i]._vertex_id, 1 ));";
        }
        
        foreach (var attr in attrs.IntAttribs)
        {
            if (attr.Name == "position")
                positionHandled = true;
            sb.AppendLine($"          geometry.addAttribute('{attr.Name}', new THREE.BufferAttribute( ghglAttributes[i].{attr.Name}, 1 ));");
            positionFiller = $"          geometry.addAttribute('position', new THREE.BufferAttribute( ghglAttributes[i].{attr.Name}, 1 ));";
        }
        foreach( var attr in attrs.FloatAttribs)
        {
            if (attr.Name == "position")
                positionHandled = true;
            sb.AppendLine($"          geometry.addAttribute('{attr.Name}', new THREE.BufferAttribute( ghglAttributes[i].{attr.Name}, 1 ));");
            positionFiller = $"          geometry.addAttribute('position', new THREE.BufferAttribute( ghglAttributes[i].{attr.Name}, 1 ));";
        }
        foreach (var attr in attrs.Vec4Attribs)
        {
            if (attr.Name == "position")
                positionHandled = true;
            sb.AppendLine($"          geometry.addAttribute('{attr.Name}', new THREE.BufferAttribute( ghglAttributes[i].{attr.Name}, 4 ));");
            positionFiller = $"          geometry.addAttribute('position', new THREE.BufferAttribute( ghglAttributes[i].{attr.Name}, 4 ));";
        }
        //foreach (var attr in attrs.Mat4Attribs)
        //{
        //    if (attr.Name == "position")
        //        positionHandled = true;
        //    sb.AppendLine($"          geometry.addAttribute('{attr.Name}', new THREE.BufferAttribute( ghglAttributes[i].{attr.Name}, 4 ));");
        //    positionFiller = $"          geometry.addAttribute('position', new THREE.BufferAttribute( ghglAttributes[i].{attr.Name}, 4 ));";
        //}
        foreach (var attr in attrs.Vec2Attribs)
        {
            if (attr.Name == "position")
                positionHandled = true;
            sb.AppendLine($"          geometry.addAttribute('{attr.Name}', new THREE.BufferAttribute( ghglAttributes[i].{attr.Name}, 2 ));");
            positionFiller = $"          geometry.addAttribute('position', new THREE.BufferAttribute( ghglAttributes[i].{attr.Name}, 2 ));";
        }
        foreach (var attr in attrs.Vec3Attribs)
        {
            if (attr.Name == "position")
                positionHandled = true;
            sb.AppendLine($"          geometry.addAttribute('{attr.Name}', new THREE.BufferAttribute( ghglAttributes[i].{attr.Name}, 3 ));");
            positionFiller = $"          geometry.addAttribute('position', new THREE.BufferAttribute( ghglAttributes[i].{attr.Name}, 3 ));";
        }

        if( attrs.Meshes.Count>0)
        {
            sb.AppendLine($"          geometry.setIndex(ghglAttributes[i].meshIndices);");
            if (attrs.Meshes[0].VertexVbo!=0)
            {
                sb.AppendLine($"          geometry.addAttribute('_meshVertex', new THREE.BufferAttribute( ghglAttributes[i]._meshVertex, 3 ));");
                positionFiller = $"          geometry.addAttribute('position', new THREE.BufferAttribute( ghglAttributes[i]._meshVertex, 3 ));";
            }
            if(attrs.Meshes[0].NormalVbo!=0)
                sb.AppendLine($"          geometry.addAttribute('_meshNormal', new THREE.BufferAttribute( ghglAttributes[i]._meshNormal, 3 ));");
            if(attrs.Meshes[0].TextureCoordVbo!=0)
                sb.AppendLine($"          geometry.addAttribute('_meshTextureCoordinate', new THREE.BufferAttribute( ghglAttributes[i]._meshTextureCoordinate, 2 ));");
            if(attrs.Meshes[0].ColorVbo!=0)
                sb.AppendLine($"          geometry.addAttribute('_meshVertexColor', new THREE.BufferAttribute( ghglAttributes[i]._meshVertexColor, 4 ));");
        }

        if ( !positionHandled )
        {
            sb.AppendLine(positionFiller);
        }

        string geometry_attributes = sb.ToString();
        contents = contents.Replace("_GEOMETRY_ATTRIBUTES_", geometry_attributes);
        contents = contents.Replace("_VERTEX_SHADER_", Program.Shaders[(int)ShaderType.Vertex].ToWebGL1Code());
        contents = contents.Replace("_FRAGMENT_SHADER_", Program.Shaders[(int)ShaderType.Fragment].ToWebGL1Code());

        if (Program.DrawMode == OpenGL.GL_LINES)
            contents = contents.Replace("_THREE_OBJECT_TYPE_", "LineSegments");
        else if (Program.DrawMode == OpenGL.GL_LINE_STRIP)
            contents = contents.Replace("_THREE_OBJECT_TYPE_", "Line");
        else if (Program.DrawMode == OpenGL.GL_POINTS)
            contents = contents.Replace("_THREE_OBJECT_TYPE_", "Points");
        else
            contents = contents.Replace("_THREE_OBJECT_TYPE_", "Mesh");

        var bg = RH.ApplicationSettings.AppearanceSettings.ViewportBackgroundColor;
        string sBg = $"{bg.R / 255.0}, {bg.G / 255.0}, {bg.B / 255.0}";
        contents = contents.Replace("_BACKGROUND_COLOR_", sBg);

        var viewport = RH.RhinoDoc.ActiveDoc.Views.ActiveView.MainViewport;
        contents = contents.Replace("_CAMERA_LOCATION_", viewport.CameraLocation.ToString());
        double frLeft, frRight, frTop, frBottom, frNear, frFar;
        viewport.GetFrustum(out frLeft, out frRight, out frBottom, out frTop, out frNear, out frFar);
        contents = contents.Replace("_FRUSTUM_WIDTH_", (frRight - frLeft).ToString());
        contents = contents.Replace("_FRUSTUM_NEAR_", frNear.ToString());
        contents = contents.Replace("_FRUSTUM_FAR_", frFar.ToString());

        //Make some controls based on the component's input
        var guiControls = new List<GuiItem>();
        for (int i=0; i<component.Params.Input.Count; i++)
        {
            if (component.Params.Input[i].Sources[0] is GK.Special.GH_NumberSlider slider)
            {
                guiControls.Add(new GuiItem (component.Params.Input[i].Name)
                {
                    Value   = (float)slider.Slider.Value,
                    Minimum = (float)slider.Slider.Minimum,
                    Maximum = (float)slider.Slider.Maximum
                });
                continue;
            }

            GL_Vec4[] v4;
            if(Pipeline.TryGetVec4Uniform (0, component.Params.Input[i].Name, out v4))
            {
                if( v4.Length==1)
                {
                    guiControls.Add (new GuiItem (component.Params.Input[i].Name)
                    {
                        Color  = v4[0],
                        IsColor = true
                    });
                }
            }
        }

        sb = new System.Text.StringBuilder();
        if(guiControls.Count>0)
        {
            sb.AppendLine($"        var gui = new dat.GUI({{ height : {guiControls.Count} * 32 - 1 }});");
            sb.AppendLine("        var GuiControl = function() {");
            foreach(var ctrl in guiControls)
            {
                if (ctrl.IsColor)
                    sb.AppendLine($"            this.{ctrl.Name} = [{(int)(ctrl.Color._x*255)}," +
                        $" {(int)(ctrl.Color._y*255)}, {(int)(ctrl.Color._z*255)}, {(int)(ctrl.Color._w*255)}];");
                else
                    sb.AppendLine($"            this.{ctrl.Name} = {ctrl.Value};");
            }
            sb.AppendLine("        };");
            sb.AppendLine("        let text = new GuiControl();");

            foreach(var ctrl in guiControls)
            {
                if (ctrl.IsColor)
                {
                    sb.AppendLine($"        gui.addColor(text, '{ctrl.Name}').onChange(function(value) {{");
                    sb.AppendLine("             for (let i = 0; i < sceneObjects.length; i++) {");
                    sb.AppendLine($"                sceneObjects[i].material.uniforms.{ctrl.Name}.value = ");
                    sb.AppendLine($"                    new THREE.Vector4(value[0]/255.0,value[1]/255.0,value[2]/255.0,value[3]/255.0);");
                    sb.AppendLine("             }");
                    sb.AppendLine("        });");
                }
                else
                {
                    sb.AppendLine($"        gui.add(text, '{ctrl.Name}', {ctrl.Minimum}, {ctrl.Maximum}).onChange(function(value) {{");
                    sb.AppendLine("             for (let i = 0; i < sceneObjects.length; i++) {");
                    sb.AppendLine($"                sceneObjects[i].material.uniforms.{ctrl.Name}.value = value;");
                    sb.AppendLine("             }");
                    sb.AppendLine("        });");
                }
            }
        }
        contents = contents.Replace("_GUI_CONTROLS_", sb.ToString());

        string htmlfilepath = System.IO.Path.Combine(dirName, baseFilename + ".html");
        System.IO.File.WriteAllText(htmlfilepath, contents);
    }

    static string _BufferToJsonString<T> (string name, List<T> data, int indent, bool indices=false)
    {
        var sb = new System.Text.StringBuilder();
        string padding = "".PadLeft(indent);
        if( indices)
            sb.AppendLine(padding + $"{name} : [");
        else
            sb.AppendLine(padding + $"{name} : new Float32Array([");
        padding = "".PadLeft(indent + 2);
        int lineBreakOn = 6;
        bool startLine = true;
        for (int i = 0; i < data.Count; i++)
        {
            if (startLine)
                sb.Append(padding);
            startLine = false;
            sb.Append(data[i]!.ToString());
            if (i < (data.Count - 1))
                sb.Append(",");
            if (i % lineBreakOn == lineBreakOn)
            {
                sb.AppendLine();
                startLine = true;
            }
        }
        if (!startLine)
            sb.AppendLine();

        if(indices)
            sb.Append("".PadLeft(indent) + "]");
        else
            sb.Append("".PadLeft(indent) + "])");
        return sb.ToString();
    }

    static int _MinAttributeLength (PipelineData attrs)
    {
        int length = -1;
        foreach (var attr in attrs.IntAttribs)
        {
            if (length < 0 || attr.Items.Length < length)
                length = attr.Items.Length;
        }
        foreach (var attr in attrs.FloatAttribs)
        {
            if (length < 0 || attr.Items.Length < length)
                length = attr.Items.Length;
        }
        foreach (var attr in attrs.Vec2Attribs)
        {
            if (length < 0 || attr.Items.Length < length)
                length = attr.Items.Length;
        }
        foreach (var attr in attrs.Vec3Attribs)
        {
            if (length < 0 || attr.Items.Length < length)
                length = attr.Items.Length;
        }
        foreach (var attr in attrs.Vec4Attribs)
        {
            if (length < 0 || attr.Items.Length < length)
                length = attr.Items.Length;
        }
        foreach (var attr in attrs.Mat4Attribs)
        {
            if (length < 0 || attr.Items.Length < length)
                length = attr.Items.Length;
        }
        return length;
    }

}
