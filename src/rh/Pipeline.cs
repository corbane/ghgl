
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using ON = Rhino.Geometry;
using RH = Rhino;
using RD = Rhino.Display;


namespace RhGL.Rhino;


using GL_int = Int32;
using GL_float = Single;


class PipelineData
{
    public PipelineData (List<UniformSamplerData> samplerCache)
    {
        _samplerCache = samplerCache;
    }


    #region Uniforms


    readonly List <UniformSamplerData> _samplerCache;


    readonly List <UniformData <GL_int>>     _intUniforms       = new ();
    readonly List <UniformData <GL_float>>   _floatUniforms     = new ();
    readonly List <UniformData <ON.Point3f>> _vec3Uniforms      = new ();
    readonly List <UniformData <GL_Vec4>>    _vec4Uniforms      = new ();
    readonly List <UniformData <GL_Mat4>>    _mat4Uniforms      = new ();
    readonly List <UniformSamplerData>       _sampler2DUniforms = new ();


    public IReadOnlyList <UniformData <GL_int>>     IntUniforms   => _intUniforms;
    public IReadOnlyList <UniformData <GL_float>>   FloatUniforms => _floatUniforms;
    public IReadOnlyList <UniformData <ON.Point3f>> Vec3Uniforms  => _vec3Uniforms;
    public IReadOnlyList <UniformData <GL_Vec4>>    Vec4Uniforms  => _vec4Uniforms;
    public IReadOnlyList <UniformData <GL_Mat4>>    Mat4Uniforms  => _mat4Uniforms;


    public void AddUniform (string name, GL_int[] values, int arrayLength)
    {
        _intUniforms.Add(new UniformData<int>(name, arrayLength, values));
    }
    public void AddUniform (string name, GL_float[] values, int arrayLength)
    {
        _floatUniforms.Add(new UniformData<float>(name, arrayLength, values));
    }
    public void AddUniform (string name, ON.Point3f[] values, int arrayLength)
    {
        _vec3Uniforms.Add(new UniformData<ON.Point3f>(name, arrayLength, values));
    }
    public void AddUniform (string name, GL_Vec4[] values, int arrayLength)
    {
        _vec4Uniforms.Add(new UniformData<GL_Vec4>(name, arrayLength, values));
    }
    public void AddUniform (string name, GL_Mat4[] values, int arrayLength)
    {
        _mat4Uniforms.Add(new UniformData<GL_Mat4>(name, arrayLength, values));
    }
    
    public void AddSampler2DUniform (string name, System.Drawing.Bitmap bmp)
    {
        var data = new UniformSamplerData(name, bmp);
        _sampler2DUniforms.Add(data);
    }
    public void AddSampler2DUniform (string name, string path)
    {
        var data = new UniformSamplerData (name, path);

        bool isComponentOutput = path.EndsWith(":color") || path.EndsWith(":depth");
        if (isComponentOutput == false)
        {
            //try to find a cached item first
            for (int i = 0; i < _samplerCache.Count; i++)
            {
                var sampler = _samplerCache[i];
                if (string.Equals (sampler.Path, path, StringComparison.OrdinalIgnoreCase))
                {
                    data.TextureId = sampler.TextureId;
                    _samplerCache.RemoveAt(i);
                    break;
                }
            }
        }
        _sampler2DUniforms.Add(data);
    }
    public string[] GetComponentSamplers ()
    {
        HashSet <string> items = new ();
        foreach(var item in _sampler2DUniforms)
        {
            if (item.Path.EndsWith (":color") || item.Path.EndsWith (":depth"))
                items.Add (item.Path);
        }
        string[] rc = new string[items.Count];
        items.CopyTo (rc);
        return rc;
    }


    public string UniformsToJsonString (int indent, List<UniformDescription> builtInsUsed)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("".PadLeft(2) + "uniforms: {");
        var chunks = new List<string>();

        foreach(var builtin in builtInsUsed)
        {
            string threeTypeValue = "";
            if (builtin.DataType == GlslUniformType.Float)
            // if (builtin.DataType == "float")
                threeTypeValue = "type:\"float\", value: 0.0";
            if (builtin.DataType == GlslUniformType.Vec3)
            //if (builtin.DataType == "vec3")
            {
                threeTypeValue = "type:\"vec3\", value: new THREE.Vector3(0,0,0)";
                if( builtin.ArrayLength>0)
                    threeTypeValue = "type:\"vec3v\", value: [new THREE.Vector3(1,-1,-3),new THREE.Vector3(0,0,0),new THREE.Vector3(0,0,0),new THREE.Vector3(0,0,0)]";
            }
            if (builtin.DataType == GlslUniformType.Vec4)
            //if (builtin.DataType == "vec4")
                threeTypeValue = "type:\"vec4\", value: new THREE.Vector4(0,0,0,0)";
            if (builtin.DataType == GlslUniformType.Mat3)
            //if (builtin.DataType == "mat3")
                threeTypeValue = "type:\"mat3\", value: new Float32Array( [1,0,0,0,1,0,0,0,1] )";
            string s = "".PadLeft(indent) + $"{builtin.Name} : {{ {threeTypeValue} }}";
            chunks.Add(s);
        }

        _intUniforms.ForEach((u) => chunks.Add(u.ToJsonString(indent + 2)));
        _floatUniforms.ForEach((u) => chunks.Add(u.ToJsonString(indent + 2)));
        _vec3Uniforms.ForEach((u) => chunks.Add(u.ToJsonString(indent + 2)));
        _vec4Uniforms.ForEach((u) => chunks.Add(u.ToJsonString(indent + 2)));
        _mat4Uniforms.ForEach((u) => chunks.Add(u.ToJsonString(indent + 2)));

        for (int i=0; i<chunks.Count; i++)
        {
            sb.Append(chunks[i]);
            if (i < (chunks.Count - 1))
                sb.Append(",");
            sb.AppendLine();
        }

        sb.Append("".PadLeft(2) + "}");
        return sb.ToString();
    }


    #endregion


    #region  Attributes


    readonly List <MeshData> _meshes = new ();

    readonly  List <Attribute <GL_int>>     _intAttribs   = new ();
    readonly  List <Attribute <GL_float>>   _floatAttribs = new ();
    readonly  List <Attribute <ON.Point2f>> _vec2Attribs  = new ();
    readonly  List <Attribute <ON.Point3f>> _vec3Attribs  = new ();
    readonly  List <Attribute <GL_Vec4>>    _vec4Attribs  = new ();
    readonly  List <Attribute <GL_Mat4>>    _mat4Attribs  = new ();


    public IReadOnlyList <MeshData> Meshes => _meshes;

    public IReadOnlyList <Attribute <GL_int>>     IntAttribs   => _intAttribs;
    public IReadOnlyList <Attribute <GL_float>>   FloatAttribs => _floatAttribs;
    public IReadOnlyList <Attribute <ON.Point2f>> Vec2Attribs  => _vec2Attribs;
    public IReadOnlyList <Attribute <ON.Point3f>> Vec3Attribs  => _vec3Attribs;
    public IReadOnlyList <Attribute <GL_Vec4>>    Vec4Attribs  => _vec4Attribs;
    public IReadOnlyList <Attribute <GL_Mat4>>    Mat4Attribs  => _mat4Attribs;


    public void AddMesh (ON.Mesh mesh)
    {
        _meshes.Add(new MeshData(mesh));
    }

    public void AddAttribute (string name, int location, GL_int[] value)
    {
        _intAttribs.Add(new Attribute<int>(name, location, value));
    }
    public void AddAttribute (string name, int location, GL_float[] value)
    {
        _floatAttribs.Add(new Attribute<float>(name, location, value));
    }
    public void AddAttribute (string name, int location, ON.Point2f[] value)
    {
        _vec2Attribs.Add(new Attribute<ON.Point2f>(name, location, value));
    }
    public void AddAttribute (string name, int location, ON.Point3f[] value)
    {
        _vec3Attribs.Add(new Attribute<ON.Point3f>(name, location, value));
    }
    public void AddAttribute (string name, int location, GL_Vec4[] value)
    {
        _vec4Attribs.Add(new Attribute<GL_Vec4>(name, location, value));
    }
    public void AddAttribute (string name, int location, GL_Mat4[] value)
    {
        _mat4Attribs.Add(new Attribute<GL_Mat4>(name, location, value));
    }



    #endregion


    public void Draw (RD.DisplayPipeline display, uint programId, uint drawMode)
    {
        int totalCount = 1;
        if (_meshes != null && _meshes.Count > 1)
            totalCount = _meshes.Count;

        for (int i = 0; i < totalCount; i++)
        {
            SetupGLUniforms (programId, i);

            int element_count = SetupGLAttributes (i, programId);
            if (element_count < 1) continue;

            if (_meshes!.Count > i)
            {
                var data = _meshes[i];
                if (drawMode == OpenGL.GL_LINES && data.LinesIndexBuffer == 0)
                {
RH.RhinoApp.WriteLine ("Set Mesh vertex buffer: drawMode: " + drawMode);
                    uint[] buffers;
                    OpenGL.glGenBuffers(1, out buffers);
                    data.LinesIndexBuffer = buffers[0];
                    OpenGL.glBindBuffer(OpenGL.GL_ELEMENT_ARRAY_BUFFER, data.LinesIndexBuffer);

                    int[] indices = new int[6 * data.Mesh.Faces.TriangleCount + 8 * data.Mesh.Faces.QuadCount];
                    int current = 0;
                    foreach (var face in data.Mesh.Faces)
                    {
                        indices[current++] = face.A;
                        indices[current++] = face.B;
                        indices[current++] = face.B;
                        indices[current++] = face.C;
                        if (face.IsTriangle)
                        {
                            indices[current++] = face.C;
                            indices[current++] = face.A;
                        }
                        if (face.IsQuad)
                        {
                            indices[current++] = face.C;
                            indices[current++] = face.D;
                            indices[current++] = face.D;
                            indices[current++] = face.A;
                        }
                    }

                    var handle = GCHandle.Alloc(indices, GCHandleType.Pinned);
                    IntPtr pointer = handle.AddrOfPinnedObject();
                    IntPtr size = new IntPtr(sizeof(int) * indices.Length);
                    OpenGL.glBufferData(OpenGL.GL_ELEMENT_ARRAY_BUFFER, size, pointer, OpenGL.GL_STATIC_DRAW);
                    handle.Free();
                }
                if (drawMode != OpenGL.GL_LINES && data.TriangleIndexBuffer == 0)
                {
RH.RhinoApp.WriteLine ("Set Mesh vertex buffer: drawMode: " + drawMode);
                    uint[] buffers;
                    OpenGL.glGenBuffers(1, out buffers);
                    data.TriangleIndexBuffer = buffers[0];
                    OpenGL.glBindBuffer(OpenGL.GL_ELEMENT_ARRAY_BUFFER, data.TriangleIndexBuffer);
                    int[] indices = new int[3 * (data.Mesh.Faces.TriangleCount + 2 * data.Mesh.Faces.QuadCount)];
                    int current = 0;
                    foreach (var face in data.Mesh.Faces)
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

                    var handle = GCHandle.Alloc(indices, GCHandleType.Pinned);
                    IntPtr pointer = handle.AddrOfPinnedObject();
                    IntPtr size = new IntPtr(sizeof(int) * indices.Length);
                    OpenGL.glBufferData(OpenGL.GL_ELEMENT_ARRAY_BUFFER, size, pointer, OpenGL.GL_STATIC_DRAW);
                    handle.Free();
                }

                if (drawMode == OpenGL.GL_LINES && data.LinesIndexBuffer != 0)
                {
                    OpenGL.glBindBuffer(OpenGL.GL_ELEMENT_ARRAY_BUFFER, data.LinesIndexBuffer);
                    int indexCount = 6 * data.Mesh.Faces.TriangleCount + 8 * data.Mesh.Faces.QuadCount;
                    OpenGL.glDrawElements(drawMode, indexCount, OpenGL.GL_UNSIGNED_INT, IntPtr.Zero);
                    OpenGL.glBindBuffer(OpenGL.GL_ELEMENT_ARRAY_BUFFER, 0);
                }
                if (drawMode != OpenGL.GL_LINES && data.TriangleIndexBuffer != 0)
                {
                    OpenGL.glBindBuffer(OpenGL.GL_ELEMENT_ARRAY_BUFFER, data.TriangleIndexBuffer);
                    int indexCount = 3 * (data.Mesh.Faces.TriangleCount + 2 * data.Mesh.Faces.QuadCount);
                    OpenGL.glDrawElements(drawMode, indexCount, OpenGL.GL_UNSIGNED_INT, IntPtr.Zero);
                    OpenGL.glBindBuffer(OpenGL.GL_ELEMENT_ARRAY_BUFFER, 0);
                }
            }
            else
            {
                OpenGL.glDrawArrays (drawMode, 0, element_count);
                OpenGL.glBindBuffer (OpenGL.GL_ARRAY_BUFFER, 0);
            }
        }

        static void DisableVertexAttribArray (int location) {
            if (location >= 0)
                OpenGL.glDisableVertexAttribArray ((uint)location);
        }
        foreach (var item in _intAttribs)   DisableVertexAttribArray (item.Location);
        foreach (var item in _floatAttribs) DisableVertexAttribArray (item.Location);
        foreach (var item in _vec2Attribs)  DisableVertexAttribArray (item.Location);
        foreach (var item in _vec3Attribs)  DisableVertexAttribArray (item.Location);
        foreach (var item in _vec4Attribs)  DisableVertexAttribArray (item.Location);
        foreach (var item in _mat4Attribs)  DisableVertexAttribArray (item.Location);
    }

    public int SetupGLAttributes (int index, uint programId)
    {
        int element_count = 0;
        if (_meshes.Count >= (index + 1) && _meshes[index]!=null)
        {
            var data = _meshes[index];
            var mesh = data.Mesh;
            if (mesh == null)
                return 0;
            element_count = mesh.Vertices.Count;
            int location = OpenGL.glGetAttribLocation(programId, "_meshVertex");
            if (location >= 0)
            {
                if (data.VertexVbo == 0)
                {
                    uint[] buffers;
                    OpenGL.glGenBuffers(1, out buffers);
                    data.VertexVbo = buffers[0];
                    OpenGL.glBindBuffer(OpenGL.GL_ARRAY_BUFFER, data.VertexVbo);
                    IntPtr size = new IntPtr(3 * sizeof(float) * mesh.Vertices.Count);
                    var points = mesh.Vertices.ToPoint3fArray();
                    var handle = GCHandle.Alloc(points, GCHandleType.Pinned);
                    IntPtr pointer = handle.AddrOfPinnedObject();
                    OpenGL.glBufferData(OpenGL.GL_ARRAY_BUFFER, size, pointer, OpenGL.GL_STREAM_DRAW);
                    handle.Free();
                }
                OpenGL.glBindBuffer(OpenGL.GL_ARRAY_BUFFER, data.VertexVbo);
                OpenGL.glEnableVertexAttribArray((uint)location);
                OpenGL.glVertexAttribPointer((uint)location, 3, OpenGL.GL_FLOAT, 0, 0, IntPtr.Zero);
            }
            location = OpenGL.glGetAttribLocation(programId, "_meshNormal");
            if (location >= 0)
            {
                if (data.NormalVbo == 0 && mesh.Normals.Count == mesh.Vertices.Count)
                {
                    uint[] buffers;
                    OpenGL.glGenBuffers(1, out buffers);
                    data.NormalVbo = buffers[0];
                    OpenGL.glBindBuffer(OpenGL.GL_ARRAY_BUFFER, data.NormalVbo);
                    IntPtr size = new IntPtr(3 * sizeof(float) * mesh.Normals.Count);
                    var normals = mesh.Normals.ToFloatArray();
                    var handle = GCHandle.Alloc(normals, GCHandleType.Pinned);
                    IntPtr pointer = handle.AddrOfPinnedObject();
                    OpenGL.glBufferData(OpenGL.GL_ARRAY_BUFFER, size, pointer, OpenGL.GL_STREAM_DRAW);
                    handle.Free();
                }
                if (data.NormalVbo != 0)
                {
                    OpenGL.glBindBuffer(OpenGL.GL_ARRAY_BUFFER, data.NormalVbo);
                    OpenGL.glEnableVertexAttribArray((uint)location);
                    OpenGL.glVertexAttribPointer((uint)location, 3, OpenGL.GL_FLOAT, 0, 0, IntPtr.Zero);
                }
                else
                {
                    OpenGL.glDisableVertexAttribArray((uint)location);
                    OpenGL.glVertexAttrib3f((uint)location, 0, 0, 0);
                }
            }

            location = OpenGL.glGetAttribLocation(programId, "_meshTextureCoordinate");
            if (location >= 0)
            {
                if (data.TextureCoordVbo == 0 && mesh.TextureCoordinates.Count == mesh.Vertices.Count)
                {
                    uint[] buffers;
                    OpenGL.glGenBuffers(1, out buffers);
                    data.TextureCoordVbo = buffers[0];
                    OpenGL.glBindBuffer(OpenGL.GL_ARRAY_BUFFER, data.TextureCoordVbo);
                    IntPtr size = new IntPtr(2 * sizeof(float) * mesh.TextureCoordinates.Count);
                    var tcs = mesh.TextureCoordinates.ToFloatArray();
                    var handle = GCHandle.Alloc(tcs, GCHandleType.Pinned);
                    IntPtr pointer = handle.AddrOfPinnedObject();
                    OpenGL.glBufferData(OpenGL.GL_ARRAY_BUFFER, size, pointer, OpenGL.GL_STREAM_DRAW);
                    handle.Free();
                }
                if (data.TextureCoordVbo != 0)
                {
                    OpenGL.glBindBuffer(OpenGL.GL_ARRAY_BUFFER, data.TextureCoordVbo);
                    OpenGL.glEnableVertexAttribArray((uint)location);
                    OpenGL.glVertexAttribPointer((uint)location, 2, OpenGL.GL_FLOAT, 0, 0, IntPtr.Zero);
                }
                else
                {
                    OpenGL.glDisableVertexAttribArray((uint)location);
                    OpenGL.glVertexAttrib2f((uint)location, 0, 0);
                }
            }

            location = OpenGL.glGetAttribLocation(programId, "_meshVertexColor");
            if (location >= 0)
            {
                if (data.ColorVbo == 0 && mesh.VertexColors.Count == mesh.Vertices.Count)
                {
                    uint[] buffers;
                    OpenGL.glGenBuffers(1, out buffers);
                    data.ColorVbo = buffers[0];
                    OpenGL.glBindBuffer(OpenGL.GL_ARRAY_BUFFER, data.ColorVbo);
                    IntPtr size = new IntPtr(4 * sizeof(float) * mesh.VertexColors.Count);
                    int colorCount = mesh.VertexColors.Count;

                    float[] colors = new float[colorCount * 4];
                    for (int i = 0; i < colorCount; i++)
                    {
                        var color = mesh.VertexColors[i];
                        colors[4 * i] = color.R / 255.0f;
                        colors[4 * i + 1] = color.G / 255.0f;
                        colors[4 * i + 2] = color.B / 255.0f;
                        colors[4 * i + 3] = color.A / 255.0f;
                    }
                    var handle = GCHandle.Alloc(colors, GCHandleType.Pinned);
                    IntPtr pointer = handle.AddrOfPinnedObject();
                    OpenGL.glBufferData(OpenGL.GL_ARRAY_BUFFER, size, pointer, OpenGL.GL_STREAM_DRAW);
                    handle.Free();
                }
                if (data.ColorVbo != 0)
                {
                    OpenGL.glBindBuffer(OpenGL.GL_ARRAY_BUFFER, data.ColorVbo);
                    OpenGL.glEnableVertexAttribArray((uint)location);
                    OpenGL.glVertexAttribPointer((uint)location, 4, OpenGL.GL_FLOAT, 0, 0, IntPtr.Zero);
                }
                else
                {
                    OpenGL.glDisableVertexAttribArray((uint)location);
                    OpenGL.glVertexAttrib2f((uint)location, 0, 0);
                }
            }
        }

        foreach (var item in _intAttribs)
        {
            if (element_count == 0)
                element_count = item.Items.Length;
            if (element_count > item.Items.Length && item.Items.Length > 1)
                element_count = item.Items.Length;

            if (item.Location < 0)
                item.Location = OpenGL.glGetAttribLocation(programId, item.Name);
            if (item.Location < 0) continue;
        
            uint location = (uint)item.Location;
            if (1 == item.Items.Length)
            {
                OpenGL.glDisableVertexAttribArray (location);
                OpenGL.glVertexAttribI1i (location, item.Items[0]);
            }
            else
            {
                if (item.VboHandle == 0)
                {
                    OpenGL.glGenBuffers (1, out var buffers);
                    item.VboHandle = buffers[0];
                    OpenGL.glBindBuffer (OpenGL.GL_ARRAY_BUFFER, item.VboHandle);
                    var size    = new IntPtr (sizeof(int) * item.Items.Length);
                    var handle  = GCHandle.Alloc (item.Items, GCHandleType.Pinned);
                    var pointer = handle.AddrOfPinnedObject ();
                    OpenGL.glBufferData (OpenGL.GL_ARRAY_BUFFER, size, pointer, OpenGL.GL_STREAM_DRAW);
                    handle.Free ();
                }
                OpenGL.glBindBuffer(OpenGL.GL_ARRAY_BUFFER, item.VboHandle);
                OpenGL.glEnableVertexAttribArray(location);
                OpenGL.glVertexAttribPointer(location, 1, OpenGL.GL_INT, 0, sizeof(int), IntPtr.Zero);
            }
        }
        foreach (var item in _floatAttribs)
        {
            if (element_count == 0)
                element_count = item.Items.Length;
            if (element_count > item.Items.Length && item.Items.Length > 1)
                element_count = item.Items.Length;

            if (item.Location < 0)
                item.Location = OpenGL.glGetAttribLocation(programId, item.Name);
            if (item.Location < 0) continue;
        
            uint location = (uint)item.Location;
            if (1 == item.Items.Length)
            {
                OpenGL.glDisableVertexAttribArray(location);
                OpenGL.glVertexAttrib1f(location, item.Items[0]);
            }
            else
            {
                if (item.VboHandle == 0)
                {
                    uint[] buffers;
                    OpenGL.glGenBuffers(1, out buffers);
                    item.VboHandle = buffers[0];
                    OpenGL.glBindBuffer(OpenGL.GL_ARRAY_BUFFER, item.VboHandle);
                    IntPtr size = new IntPtr(sizeof(float) * item.Items.Length);
                    var handle = GCHandle.Alloc(item.Items, GCHandleType.Pinned);
                    IntPtr pointer = handle.AddrOfPinnedObject();
                    OpenGL.glBufferData(OpenGL.GL_ARRAY_BUFFER, size, pointer, OpenGL.GL_STREAM_DRAW);
                    handle.Free();
                }
                OpenGL.glBindBuffer(OpenGL.GL_ARRAY_BUFFER, item.VboHandle);
                OpenGL.glEnableVertexAttribArray(location);
                OpenGL.glVertexAttribPointer(location, 1, OpenGL.GL_FLOAT, 0, sizeof(float), IntPtr.Zero);
            }
        }
        foreach (var item in _vec2Attribs)
        {
            if (element_count == 0)
                element_count = item.Items.Length;
            if (element_count > item.Items.Length && item.Items.Length > 1)
                element_count = item.Items.Length;

            if (item.Location < 0)
                item.Location = OpenGL.glGetAttribLocation(programId, item.Name);
            if (item.Location < 0) continue;
        
            uint location = (uint)item.Location;
            if (1 == item.Items.Length)
            {
                OpenGL.glDisableVertexAttribArray(location);
                ON.Point2f v = item.Items[0];
                OpenGL.glVertexAttrib2f(location, v.X, v.Y);
            }
            else
            {
                if (item.VboHandle == 0)
                {
                    uint[] buffers;
                    OpenGL.glGenBuffers(1, out buffers);
                    item.VboHandle = buffers[0];
                    OpenGL.glBindBuffer(OpenGL.GL_ARRAY_BUFFER, item.VboHandle);
                    IntPtr size = new IntPtr(2 * sizeof(float) * item.Items.Length);
                    var handle = GCHandle.Alloc(item.Items, GCHandleType.Pinned);
                    IntPtr pointer = handle.AddrOfPinnedObject();
                    OpenGL.glBufferData(OpenGL.GL_ARRAY_BUFFER, size, pointer, OpenGL.GL_STREAM_DRAW);
                    handle.Free();
                }
                OpenGL.glBindBuffer(OpenGL.GL_ARRAY_BUFFER, item.VboHandle);
                OpenGL.glEnableVertexAttribArray(location);
                OpenGL.glVertexAttribPointer(location, 2, OpenGL.GL_FLOAT, 0, 2 * sizeof(float), IntPtr.Zero);
            }
        }
        foreach (var item in _vec3Attribs)
        {
            if (element_count == 0)
                element_count = item.Items.Length;
            if (element_count > item.Items.Length && item.Items.Length > 1)
                element_count = item.Items.Length;

            if (item.Location < 0)
                item.Location = OpenGL.glGetAttribLocation(programId, item.Name);
            if (item.Location < 0) continue;
        
            uint location = (uint)item.Location;
            if (1 == item.Items.Length)
            {
                OpenGL.glDisableVertexAttribArray(location);
                ON.Point3f v = item.Items[0];
                OpenGL.glVertexAttrib3f(location, v.X, v.Y, v.Z);
            }
            else
            {
                if (item.VboHandle == 0)
                {
                    OpenGL.glGenBuffers (1, out var buffers);
                    item.VboHandle = buffers[0];
                    OpenGL.glBindBuffer (OpenGL.GL_ARRAY_BUFFER, item.VboHandle);
                    var size    = new IntPtr(3 * sizeof(float) * item.Items.Length);
                    var handle  = GCHandle.Alloc(item.Items, GCHandleType.Pinned);
                    var pointer = handle.AddrOfPinnedObject ();
                    OpenGL.glBufferData (OpenGL.GL_ARRAY_BUFFER, size, pointer, OpenGL.GL_STREAM_DRAW);
                    handle.Free ();
                }
                OpenGL.glBindBuffer(OpenGL.GL_ARRAY_BUFFER, item.VboHandle);
                OpenGL.glEnableVertexAttribArray(location);
                OpenGL.glVertexAttribPointer(location, 3, OpenGL.GL_FLOAT, 0, 3 * sizeof(float), IntPtr.Zero);
            }
        }
        foreach (var item in _vec4Attribs)
        {
            if (element_count == 0)
                element_count = item.Items.Length;
            if (element_count > item.Items.Length && item.Items.Length > 1)
                element_count = item.Items.Length;

            if (item.Location < 0)
                item.Location = OpenGL.glGetAttribLocation(programId, item.Name);
            if (item.Location < 0) continue;
        
            uint location = (uint)item.Location;
            if (1 == item.Items.Length)
            {
                OpenGL.glDisableVertexAttribArray(location);
                GL_Vec4 v = item.Items[0];
                OpenGL.glVertexAttrib4f(location, v._x, v._y, v._z, v._w);
            }
            else
            {
                if (item.VboHandle == 0)
                {
                    OpenGL.glGenBuffers (1, out var buffers);
                    item.VboHandle = buffers[0];
                    OpenGL.glBindBuffer (OpenGL.GL_ARRAY_BUFFER, item.VboHandle);
                    var size    = new IntPtr (4 * sizeof(float) * item.Items.Length);
                    var handle  = GCHandle.Alloc (item.Items, GCHandleType.Pinned);
                    var pointer = handle.AddrOfPinnedObject ();
                    OpenGL.glBufferData (OpenGL.GL_ARRAY_BUFFER, size, pointer, OpenGL.GL_STREAM_DRAW);
                    handle.Free ();
                }
                OpenGL.glBindBuffer(OpenGL.GL_ARRAY_BUFFER, item.VboHandle);
                OpenGL.glEnableVertexAttribArray(location);
                OpenGL.glVertexAttribPointer(location, 4, OpenGL.GL_FLOAT, 0, 4 * sizeof(float), IntPtr.Zero);
            }
        }
        foreach (var item in _mat4Attribs)
        {
            if (element_count == 0)
                element_count = item.Items.Length;
            if (element_count > item.Items.Length && item.Items.Length > 1)
                element_count = item.Items.Length;

            if (item.Location < 0)
                item.Location = OpenGL.glGetAttribLocation(programId, item.Name);
            if (item.Location < 0) continue;
        
            uint location = (uint)item.Location;
            if (item.VboHandle == 0)
            {
                uint[] buffers;
                OpenGL.glGenBuffers(1, out buffers);
                item.VboHandle = buffers[0];
                OpenGL.glBindBuffer(OpenGL.GL_ARRAY_BUFFER, item.VboHandle);
                IntPtr size = new IntPtr(16 * sizeof(float) * item.Items.Length);
                var handle = GCHandle.Alloc(item.Items, GCHandleType.Pinned);
                IntPtr pointer = handle.AddrOfPinnedObject();
                OpenGL.glBufferData(OpenGL.GL_ARRAY_BUFFER, size, pointer, OpenGL.GL_STREAM_DRAW);
                handle.Free();
            }
            OpenGL.glBindBuffer(OpenGL.GL_ARRAY_BUFFER, item.VboHandle);
            OpenGL.glEnableVertexAttribArray(location);
            OpenGL.glVertexAttribPointer(location, 16, OpenGL.GL_FLOAT, 0, 16 * sizeof(float), IntPtr.Zero);
        }

        return element_count;
    }

    public void SetupGLUniforms (uint programId, int itemIndex)
    {
        foreach (var uniform in _intUniforms)
        {
            int arrayLength = uniform.ArrayLength;
            int location = OpenGL.glGetUniformLocation(programId, uniform.Name);
            if (location < 0) continue;

            if (arrayLength < 1)
            {
                int dataIndex = itemIndex;
                if (itemIndex >= uniform.Data.Length)
                    dataIndex = uniform.Data.Length - 1;
                OpenGL.glUniform1i(location, uniform.Data[dataIndex]);
            }
            else if (uniform.Data.Length >= arrayLength)
            {
                OpenGL.glUniform1iv(location, arrayLength, uniform.Data);
            }
        }
        foreach (var uniform in _floatUniforms)
        {
            int arrayLength = uniform.ArrayLength;
            int location = OpenGL.glGetUniformLocation(programId, uniform.Name);
            if (location < 0) continue;

            if (arrayLength < 1)
            {
                int dataIndex = itemIndex;
                if (itemIndex >= uniform.Data.Length)
                    dataIndex = uniform.Data.Length - 1;
                OpenGL.glUniform1f(location, uniform.Data[dataIndex]);
            }
            else if (uniform.Data.Length >= arrayLength)
            {
                OpenGL.glUniform1fv(location, arrayLength, uniform.Data);
            }
        }
        foreach (var uniform in _vec3Uniforms)
        {
            int arrayLength = uniform.ArrayLength;
            int location = OpenGL.glGetUniformLocation(programId, uniform.Name);
            if (location < 0) continue;

            if (arrayLength < 1)
            {
                int dataIndex = itemIndex;
                if (itemIndex >= uniform.Data.Length)
                    dataIndex = uniform.Data.Length - 1;
                OpenGL.glUniform3f(location, uniform.Data[dataIndex].X, uniform.Data[dataIndex].Y, uniform.Data[dataIndex].Z);
            }
            else if (uniform.Data.Length >= arrayLength)
            {
                float[] data = new float[arrayLength * 3];
                for (int i = 0; i < arrayLength; i++)
                {
                    data[i * 3] = uniform.Data[i].X;
                    data[i * 3 + 1] = uniform.Data[i].Y;
                    data[i * 3 + 2] = uniform.Data[i].Z;
                }
                OpenGL.glUniform3fv(location, arrayLength, data);
            }
        }
        foreach (var uniform in _vec4Uniforms)
        {
            int arrayLength = uniform.ArrayLength;
            int location = OpenGL.glGetUniformLocation(programId, uniform.Name);
            if (location < 0) continue;

            if (arrayLength < 1)
            {
                int dataIndex = itemIndex;
                if (itemIndex >= uniform.Data.Length)
                    dataIndex = uniform.Data.Length - 1;
                OpenGL.glUniform4f(location, uniform.Data[dataIndex]._x, uniform.Data[dataIndex]._y, uniform.Data[dataIndex]._z, uniform.Data[dataIndex]._w);
            }
            else if (uniform.Data.Length >= arrayLength)
            {
                float[] data = new float[arrayLength * 4];
                for (int i = 0; i < arrayLength; i++)
                {
                    data[i * 4] = uniform.Data[i]._x;
                    data[i * 4 + 1] = uniform.Data[i]._y;
                    data[i * 4 + 2] = uniform.Data[i]._z;
                    data[i * 4 + 3] = uniform.Data[i]._w;
                }
                OpenGL.glUniform4fv(location, arrayLength, data);
            }
        }
        foreach (var uniform in _mat4Uniforms)
        {
            int arrayLength = uniform.ArrayLength;
            if (arrayLength == 0)
                arrayLength = 1;
            int location = OpenGL.glGetUniformLocation(programId, uniform.Name);
            if (location < 0) continue;

            float[] data = new float[arrayLength * 16];
            for (int i = 0; i < arrayLength; i++)
            {
                data[i * 16] = uniform.Data[i]._00;
                data[i * 16 + 1] = uniform.Data[i]._01;
                data[i * 16 + 2] = uniform.Data[i]._02;
                data[i * 16 + 3] = uniform.Data[i]._03;

                data[i * 16 + 4] = uniform.Data[i]._10;
                data[i * 16 + 5] = uniform.Data[i]._11;
                data[i * 16 + 6] = uniform.Data[i]._12;
                data[i * 16 + 7] = uniform.Data[i]._13;

                data[i * 16 + 8] = uniform.Data[i]._20;
                data[i * 16 + 9] = uniform.Data[i]._21;
                data[i * 16 + 10] = uniform.Data[i]._22;
                data[i * 16 + 11] = uniform.Data[i]._23;

                data[i * 16 + 12] = uniform.Data[i]._30;
                data[i * 16 + 13] = uniform.Data[i]._31;
                data[i * 16 + 14] = uniform.Data[i]._32;
                data[i * 16 + 15] = uniform.Data[i]._33;
            }
            OpenGL.glUniformMatrix4fv(location, uniform.Data.Length, true, data);
        }

        int currentTexture = (int)SamplerTextureUnit.BaseSampler;
        foreach (var uniform in _sampler2DUniforms)
        {
            int location = OpenGL.glGetUniformLocation(programId, uniform.Name);
            if (location < 0) continue;

            if (uniform.Path.EndsWith(":color") || uniform.Path.EndsWith(":depth"))
                uniform.TextureId = PerFrameCache.GetTextureId(uniform.Path);

            if (0 == uniform.TextureId)
            {
                if (uniform.Path.EndsWith(":color") || uniform.Path.EndsWith(":depth"))
                    uniform.TextureId = PerFrameCache.GetTextureId(uniform.Path);
                else
                    uniform.TextureId = UniformSamplerData.CreateTexture (uniform.GetBitmap ());
            }
            if (uniform.TextureId != 0)
            {
                OpenGL.glActiveTexture(OpenGL.GL_TEXTURE0 + (uint)currentTexture);
                OpenGL.glBindTexture(OpenGL.GL_TEXTURE_2D, uniform.TextureId);
                OpenGL.glUniform1i(location, currentTexture);
                OpenGL.glActiveTexture(OpenGL.GL_TEXTURE0);
                currentTexture++;
            }
        }
    }


    public void ClearData ()
    {
        foreach (var data in _meshes)
        {
            data.TriangleIndexBuffer = 0;
            data.LinesIndexBuffer = 0;
            data.NormalVbo = 0;
            data.TextureCoordVbo = 0;
            data.VertexVbo = 0;
        }
        _meshes.Clear();
        _intUniforms.Clear();
        _floatUniforms.Clear();
        _vec3Uniforms.Clear();
        _vec4Uniforms.Clear();
        _mat4Uniforms.Clear();

        foreach (var attr in _intAttribs) Scheduler.AddVboToDeleteList(attr.VboHandle);
        foreach (var attr in _floatAttribs) Scheduler.AddVboToDeleteList(attr.VboHandle);
        foreach (var attr in _vec2Attribs) Scheduler.AddVboToDeleteList(attr.VboHandle);
        foreach (var attr in _vec3Attribs) Scheduler.AddVboToDeleteList(attr.VboHandle);
        foreach (var attr in _vec4Attribs) Scheduler.AddVboToDeleteList(attr.VboHandle);
        foreach (var attr in _mat4Attribs) Scheduler.AddVboToDeleteList(attr.VboHandle);

        _intAttribs.Clear();
        _floatAttribs.Clear();
        _vec2Attribs.Clear();
        _vec3Attribs.Clear();
        _vec4Attribs.Clear();
        _mat4Attribs.Clear();

        _samplerCache.AddRange(_sampler2DUniforms);
        while (_samplerCache.Count > 10)
        {
            var sampler = _samplerCache[0];
            Scheduler.AddTextureToDeleteList(sampler.TextureId);
            _samplerCache.RemoveAt(0);
        }
        _sampler2DUniforms.Clear();

    }

}


class Pipeline
{

    public Pipeline (Program program)
    {
        Program = program;
    }
    

    #region Property


    public Guid Guid { get; } = Guid.NewGuid ();

    public Program Program { get; }
    
    readonly List <UniformSamplerData> _samplerCache = new ();

    public bool Hidden { get; set; }

    
    #endregion


    // ------------------------------------------------------------------
    // If this component uses texture inputs that are the result of other
    // components, then those other components must be drawn before this
    // component (they are "upstream")

    // ???: This function is called inside the render loop, it seems unnecessary
    //      Renderer.EnableConduit() > Rhino PostDrawObjects event
    //          > Scheduler.OnDrawObjects() > Scheduler.SortComponents()
    internal HashSet <Pipeline> RequiredUpstreamComponents ()
    {
        var upstream = new HashSet <Pipeline> ();

        // Just assume that all sampler inputs are the same across iterations.
        var uniforms = GetIterationData (0);
        var samplers = uniforms.GetComponentSamplers ();
        var ids = new HashSet <Guid> ();
        foreach(var sampler in samplers)
        {
            string id = sampler.Substring (0, sampler.IndexOf (":"));
            if (Guid.TryParse (id, out Guid componentId))
                ids.Add (componentId);
        }
        foreach(var id in ids)
        {
            foreach (var pipeline in Scheduler.RenderList)
            {
                if (pipeline == this)
                    continue;
                //if (component.InstanceGuid == id)
                if (pipeline.Guid == id)
                    upstream.Add (pipeline);
            }
        }
        
        return upstream;
    }


    #region Iteration Data


    readonly List <PipelineData> _iterationData = new ();

    public IReadOnlyList <PipelineData> IterationData => _iterationData;

    public PipelineData GetIterationData (int iteration)
    {
        while (iteration >= _iterationData.Count)
            _iterationData.Add(new PipelineData(_samplerCache));
        return _iterationData[iteration];
    }

    public void ClearIterationData ()
    {
        foreach (var iteration in _iterationData)
            iteration.ClearData();
        _iterationData.Clear();
    }

    public bool TryGetVec4Uniform (int iteration, string name, out GL_Vec4[] data)
    {
        data = new GL_Vec4[0];
        foreach(var u in _iterationData[iteration].Vec4Uniforms)
        {
            if( u.Name == name)
            {
                data = u.Data;
                return true;
            }
        }
        return false;
    }


    #endregion


    #region Render function

    public virtual void Draw (RD.DisplayPipeline display)
    {
        var prog = Program;

        uint programId = prog.ProgramId;
        if (programId == 0) {
            // prog.CompileProgram ();
            return;
        }

        bool currentDepthTestingEnabled = OpenGL.IsEnabled(OpenGL.GL_DEPTH_TEST);
        if (currentDepthTestingEnabled != prog.DepthTestingEnabled)
        {
            if (prog.DepthTestingEnabled)
                OpenGL.glEnable (OpenGL.GL_DEPTH_TEST);
            else
                OpenGL.glDisable (OpenGL.GL_DEPTH_TEST);
        }
        if (!prog.DepthWritingEnabled)
            OpenGL.glDepthMask ((byte)OpenGL.GL_FALSE);

        OpenGL.glGenVertexArrays (1, out var vao);
        OpenGL.glBindVertexArray (vao[0]);
        OpenGL.glUseProgram (programId);

        // TODO: Parse shader and figure out the proper number to place here
        if (OpenGL.GL_PATCHES == prog.DrawMode)
            OpenGL.glPatchParameteri (OpenGL.GL_PATCH_VERTICES, prog.PatchVertices);

        OpenGL.glLineWidth ((float)prog.glLineWidth);
        OpenGL.glPointSize ((float)prog.glPointSize);

        // Define standard uniforms
        foreach (var builtin in BuiltIn.GetUniformBuiltIns ())
            builtin.Setup (programId, display);

        if (OpenGL.GL_POINTS == prog.DrawMode)
            OpenGL.glEnable(OpenGL.GL_VERTEX_PROGRAM_POINT_SIZE);
        OpenGL.glEnable(OpenGL.GL_BLEND);
        OpenGL.glBlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);

        foreach (var iteration in IterationData)
            iteration.Draw (display, programId, prog.DrawMode);

        OpenGL.glDisable (OpenGL.GL_BLEND);
        OpenGL.glBindVertexArray (0);
        OpenGL.glDeleteVertexArrays (1, vao);
        OpenGL.glUseProgram (0);

        if (currentDepthTestingEnabled != prog.DepthTestingEnabled)
        {
            if (currentDepthTestingEnabled)
                OpenGL.glEnable (OpenGL.GL_DEPTH_TEST);
            else
                OpenGL.glDisable (OpenGL.GL_DEPTH_TEST);
        }
        if (!prog.DepthWritingEnabled)
            OpenGL.glDepthMask ((byte)OpenGL.GL_TRUE);

        // capture output color and depth buffer if they are needed downstream
        bool saveColor = true;//            PerFrameCache.IsColorTextureUsed(component);
        if( saveColor )
        {
            IntPtr texture2dPtr = Rhino7NativeMethods.RhTexture2dCreate();
            if (Rhino7NativeMethods.RhTexture2dCapture(display, texture2dPtr, Rhino7NativeMethods.CaptureFormat.kRGBA))
                PerFrameCache.SaveColorTexture (Guid, texture2dPtr);
        }
        bool saveDepth = true;//            PerFrameCache.IsDepthTextureUsed(component);
        if(saveDepth)
        {
            IntPtr texture2dPtr = Rhino7NativeMethods.RhTexture2dCreate();
            if (Rhino7NativeMethods.RhTexture2dCapture(display, texture2dPtr, Rhino7NativeMethods.CaptureFormat.kDEPTH24))
                PerFrameCache.SaveDepthTexture (Guid, texture2dPtr);
        }
    }


    #endregion
}


