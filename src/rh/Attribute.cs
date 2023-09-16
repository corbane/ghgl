
using ON = Rhino.Geometry;


namespace RhGL.Rhino;


public class AttributeDescription
{
    public string Name { get; set; }
    public int Location { get; set; }
    //public string DataType { get; set; }
    public GlslAttribType DataType { get; set; }

    // public AttributeDescription (string dataType, string name, int location = 0) {
    //     DataType = dataType;
    //     Name     = name;
    //     Location = location;
    // }
    public AttributeDescription (GlslAttribType dataType, string name, int location = 0) {

        Name     = name;
        DataType = dataType;
        Location = location;
    }
}


// GL_AttributeData like UniformData & SamplerUniformData ???
class Attribute <T>
{
    public Attribute(string name, int location, T[] items)
    {
        Name     = name;
        Location = location;
        Items    = items;
    }
    
    public string Name { get; }

    public T[] Items { get; }

    public int Location { get; set; }

    uint _vboHandle;
    public uint VboHandle
    {
        get { return _vboHandle; }
        set {
            if (_vboHandle == value) return;
            Scheduler.AddVboToDeleteList(_vboHandle);
            _vboHandle = value;
        }
    }

    public string ToJsonString(int indent)
    {
        var sb = new System.Text.StringBuilder ();
        string padding = "".PadLeft (indent);
        sb.AppendLine (padding + $"{Name} : new Float32Array([");
        padding = "".PadLeft (indent + 2);
        int lineBreakOn = 6;
        bool startLine = true;
        for (int i = 0; i < Items.Length; i++)
        {
            if (startLine)
                sb.Append (padding);
            startLine = false;
            sb.Append (Items[i]!.ToString ());
            if (i < (Items.Length - 1))
                sb.Append (",");
            if (i % lineBreakOn == lineBreakOn)
            {
                sb.AppendLine ();
                startLine = true;
            }
        }
        if (!startLine)
            sb.AppendLine ();
            
        sb.Append ("".PadLeft(indent) + "])");
        return sb.ToString ();
    }
}


public class MeshData
{

    public MeshData (ON.Mesh mesh)
    {
        Mesh = mesh;
    }

    public ON.Mesh Mesh { get; private set; }

    uint _triangleIndexBuffer;
    public uint TriangleIndexBuffer
    {
        get { return _triangleIndexBuffer; }
        set {
            if (_triangleIndexBuffer == value) return;
            Scheduler.AddVboToDeleteList(_triangleIndexBuffer);
            _triangleIndexBuffer = value;
        }
    }

    uint _linesIndexBuffer;
    public uint LinesIndexBuffer
    {
        get { return _linesIndexBuffer; }
        set {
            if (_linesIndexBuffer == value) return;
            Scheduler.AddVboToDeleteList(_linesIndexBuffer);
            _linesIndexBuffer = value;
        }
    }

    uint _vertexVbo;
    public uint VertexVbo
    {
        get { return _vertexVbo; }
        set {
            if (_vertexVbo == value) return;
            Scheduler.AddVboToDeleteList(_vertexVbo);
            _vertexVbo = value;
        }
    }

    uint _normalVbo;
    public uint NormalVbo
    {
        get { return _normalVbo; }
        set {
            if (_normalVbo == value) return;
            Scheduler.AddVboToDeleteList(_normalVbo);
            _normalVbo = value;
        }
    }

    uint _textureCoordVbo;
    public uint TextureCoordVbo
    {
        get { return _textureCoordVbo; }
        set {
            if (_textureCoordVbo == value) return;
            Scheduler.AddVboToDeleteList(_textureCoordVbo);
            _textureCoordVbo = value;
        }
    }

    uint _colorVbo;
    public uint ColorVbo
    {
        get { return _colorVbo; }
        set {
            if (_colorVbo == value) return;
            Scheduler.AddVboToDeleteList(_colorVbo);
            _colorVbo = value;
        }
    }

}

