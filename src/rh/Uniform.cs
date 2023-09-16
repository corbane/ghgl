using System;
using SD = System.Drawing;

using ON = Rhino.Geometry; // juste une ref dans UniformData.ToJsonString()


namespace RhGL.Rhino;


public class UniformDescription
{
    public string Name { get; }
    
    public GlslUniformType DataType { get; }
    // public string DataType { get; }

    public int ArrayLength { get; }

    // public UniformDescription (string name, string dataType, int arrayLength) {
    //     Name        = name;
    //     DataType    = dataType;
    //     ArrayLength = arrayLength;
    // }
    public UniformDescription (string name, GlslUniformType dataType, int arrayLength) {
        Name        = name;
        DataType    = dataType;
        ArrayLength = arrayLength;
    }
}


public class UniformData <T>
{
    public UniformData (string name, int arrayLength, T[] value)
    {
        Name        = name;
        ArrayLength = arrayLength;
        Data        = value;
    }

    public string Name { get; }

    public int ArrayLength { get; }

    public T[] Data { get; }

    public string ToJsonString (int indent)
    {
        string threeTypeValue = "";
        if (typeof(T) == typeof(int) || typeof(T) == typeof(float))
            threeTypeValue = $"type:\"float\", value: {Data[0]}";
        if (typeof(T) == typeof(ON.Point3f))
        {
            threeTypeValue = $"type:\"vec3\", value: new THREE.Vector3({Data[0]})";
        }
        if (typeof(T) == typeof(GL_Vec4))
            threeTypeValue = $"type:\"vec4\", value: new THREE.Vector4({Data[0]})";

        string s = "".PadLeft(indent) + $"{Name} : {{ {threeTypeValue} }}";
        return s;
    }
}


public class UniformSamplerData
{
    public UniformSamplerData (string name, string path)
    {
        Name = name;
        Path = path;
    }

    public UniformSamplerData (string name, SD.Bitmap bmp)
    {
        Name = name;
        _bitmap = bmp;
        Path = "<bitmap>";
    }
    
    public string Name { get; private set; }
    
    public string Path { get; private set; }
    
    SD.Bitmap? _bitmap;
    public SD.Bitmap? GetBitmap()
    {
        if (_bitmap == null)
        {
            try
            {
                string localPath = Path;
                if (Path.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase) ||
                    Path.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
                {
                    using (var client = new System.Net.WebClient ())
                    {
                        var stream = client.OpenRead (Path);
                        var bmp = new SD.Bitmap(stream);
                        _bitmap = bmp;
                    }
                }
                else
                {
                    var bmp = new SD.Bitmap(localPath);
                    _bitmap = bmp;
                }
            }
            catch(Exception)
            {

            }
        }
        return _bitmap;
    }

    uint _textureId;
    public uint TextureId
    {
        get { return _textureId; }
        set
        {
            if (_textureId != value)
            {
                Scheduler.AddTextureToDeleteList(_textureId);
                _textureId = value;
            }
        }
    }
    
    public static uint CreateTexture (SD.Bitmap? bmp)
    {
        uint textureId;
        try
        {
            var rect = new SD.Rectangle(0, 0, bmp!.Width, bmp.Height);
            uint[] textures = { 0 };
            OpenGL.glGenTextures(1, textures);
            OpenGL.glBindTexture(OpenGL.GL_TEXTURE_2D, textures[0]);

            if (bmp.PixelFormat == SD.Imaging.PixelFormat.Format24bppRgb)
            {
                var bmpData = bmp.LockBits(rect, SD.Imaging.ImageLockMode.ReadOnly, SD.Imaging.PixelFormat.Format24bppRgb);
                OpenGL.glTexImage2D(OpenGL.GL_TEXTURE_2D, 0, (int)OpenGL.GL_RGB, bmpData.Width, bmpData.Height, 0, OpenGL.GL_BGR, OpenGL.GL_UNSIGNED_BYTE, bmpData.Scan0);
                bmp.UnlockBits(bmpData);
            }
            else
            {
                var bmpData = bmp.LockBits(rect, SD.Imaging.ImageLockMode.ReadOnly, SD.Imaging.PixelFormat.Format32bppArgb);
                OpenGL.glTexImage2D(OpenGL.GL_TEXTURE_2D, 0, (int)OpenGL.GL_RGBA, bmpData.Width, bmpData.Height, 0, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, bmpData.Scan0);
                bmp.UnlockBits(bmpData);
            }
            textureId = textures[0];
            // See warning on
            // https://www.khronos.org/opengl/wiki/Common_Mistakes#Automatic_mipmap_generation
            OpenGL.glEnable(OpenGL.GL_TEXTURE_2D);
            OpenGL.glGenerateMipmap(OpenGL.GL_TEXTURE_2D);
            OpenGL.glTexParameteri(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, (int)OpenGL.GL_REPEAT);
            OpenGL.glTexParameteri(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, (int)OpenGL.GL_REPEAT);
            OpenGL.glTexParameteri(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, (int)OpenGL.GL_LINEAR_MIPMAP_LINEAR);
            OpenGL.glTexParameteri(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, (int)OpenGL.GL_LINEAR);
            OpenGL.glBindTexture(OpenGL.GL_TEXTURE_2D, 0);
        }
        catch (Exception)
        {
            textureId = 0;
        }

        return textureId;
    }
}

