
using ON = Rhino.Geometry;


namespace RhGL.Rhino;


struct GL_Vec4
{
    public float _x, _y, _z, _w;
    public GL_Vec4 (float a, float b, float c, float d)
    {
        _x = a;
        _y = b;
        _z = c;
        _w = d;
    }
    public override string ToString()
    {
        return $"{_x},{_y},{_z},{_w}";
    }
}


struct GL_Mat4
{
    internal float _00, _01, _02, _03;
    internal float _10, _11, _12, _13;
    internal float _20, _21, _22, _23;
    internal float _30, _31, _32, _33;

    public GL_Mat4 (ON.Transform xform)
    {
        _00 = (float)xform.M00;
        _01 = (float)xform.M01;
        _02 = (float)xform.M02;
        _03 = (float)xform.M03;

        _10 = (float)xform.M10;
        _11 = (float)xform.M11;
        _12 = (float)xform.M12;
        _13 = (float)xform.M13;

        _20 = (float)xform.M20;
        _21 = (float)xform.M21;
        _22 = (float)xform.M22;
        _23 = (float)xform.M23;

        _30 = (float)xform.M30;
        _31 = (float)xform.M31;
        _32 = (float)xform.M32;
        _33 = (float)xform.M33;
    }

    public override string ToString()
    {
        return $"{_00},{_01},{_02},{_03},{_10},{_11},{_12},{_13},{_20},{_21},{_22},{_23},{_30},{_31},{_32},{_33}";
    }
}


// https://registry.khronos.org/OpenGL-Refpages/gl4/html/glGetActiveAttrib.xhtml
public enum GlslAttribType : uint
{
    Float   = OpenGL.GL_FLOAT,
    Vec2    = OpenGL.GL_FLOAT_VEC2,
    Vec3    = OpenGL.GL_FLOAT_VEC3,
    Vec4    = OpenGL.GL_FLOAT_VEC4,
    Mat2    = OpenGL.GL_FLOAT_MAT2,
    Mat3    = OpenGL.GL_FLOAT_MAT3,
    Mat4    = OpenGL.GL_FLOAT_MAT4,
    Mat2x3  = OpenGL.GL_FLOAT_MAT2x3,
    Mat2x4  = OpenGL.GL_FLOAT_MAT2x4,
    Mat3x2  = OpenGL.GL_FLOAT_MAT3x2,
    Mat3x4  = OpenGL.GL_FLOAT_MAT3x4,
    Mat4x2  = OpenGL.GL_FLOAT_MAT4x2,
    Mat4x3  = OpenGL.GL_FLOAT_MAT4x3,
    Int     = OpenGL.GL_INT,
    IVec2   = OpenGL.GL_INT_VEC2,
    IVec3   = OpenGL.GL_INT_VEC3,
    IVec4   = OpenGL.GL_INT_VEC4,
    UInt    = OpenGL.GL_UNSIGNED_INT,
    UVec2   = OpenGL.GL_UNSIGNED_INT_VEC2,
    UVec3   = OpenGL.GL_UNSIGNED_INT_VEC3,
    UVec4   = OpenGL.GL_UNSIGNED_INT_VEC4,
    Double  = OpenGL.GL_DOUBLE,
    DVec2   = OpenGL.GL_DOUBLE_VEC2,
    DVec3   = OpenGL.GL_DOUBLE_VEC3,
    DVec4   = OpenGL.GL_DOUBLE_VEC4,
    DMat2   = OpenGL.GL_DOUBLE_MAT2,
    DMat3   = OpenGL.GL_DOUBLE_MAT3,
    DMat4   = OpenGL.GL_DOUBLE_MAT4,
    DMat2x3 = OpenGL.GL_DOUBLE_MAT2x3,
    DMat2x4 = OpenGL.GL_DOUBLE_MAT2x4,
    DMat3x2 = OpenGL.GL_DOUBLE_MAT3x2,
    DMat3x4 = OpenGL.GL_DOUBLE_MAT3x4,
    DMat4x2 = OpenGL.GL_DOUBLE_MAT4x2,
    DMat4x3 = OpenGL.GL_DOUBLE_MAT4x3
}

// https://registry.khronos.org/OpenGL-Refpages/gl4/html/glGetActiveUniform.xhtml
public enum GlslUniformType : uint
{
    Float                = OpenGL.GL_FLOAT,
    Vec2                 = OpenGL.GL_FLOAT_VEC2,
    Vec3                 = OpenGL.GL_FLOAT_VEC3,
    Vec4                 = OpenGL.GL_FLOAT_VEC4,
    Double               = OpenGL.GL_DOUBLE,
    DVec2                = OpenGL.GL_DOUBLE_VEC2,
    DVec3                = OpenGL.GL_DOUBLE_VEC3,
    DVec4                = OpenGL.GL_DOUBLE_VEC4,
    Int                  = OpenGL.GL_INT,
    IVec2                = OpenGL.GL_INT_VEC2,
    IVec3                = OpenGL.GL_INT_VEC3,
    IVec4                = OpenGL.GL_INT_VEC4,
    UInt                 = OpenGL.GL_UNSIGNED_INT,
    UVec2                = OpenGL.GL_UNSIGNED_INT_VEC2,
    UVec3                = OpenGL.GL_UNSIGNED_INT_VEC3,
    UVec4                = OpenGL.GL_UNSIGNED_INT_VEC4,
    Bool                 = OpenGL.GL_BOOL,
    BVec2                = OpenGL.GL_BOOL_VEC2,
    BVec3                = OpenGL.GL_BOOL_VEC3,
    BVec4                = OpenGL.GL_BOOL_VEC4,
    Mat2                 = OpenGL.GL_FLOAT_MAT2,
    Mat3                 = OpenGL.GL_FLOAT_MAT3,
    Mat4                 = OpenGL.GL_FLOAT_MAT4,
    Mat2x3               = OpenGL.GL_FLOAT_MAT2x3,
    Mat2x4               = OpenGL.GL_FLOAT_MAT2x4,
    Mat3x2               = OpenGL.GL_FLOAT_MAT3x2,
    Mat3x4               = OpenGL.GL_FLOAT_MAT3x4,
    Mat4x2               = OpenGL.GL_FLOAT_MAT4x2,
    Mat4x3               = OpenGL.GL_FLOAT_MAT4x3,
    DMat2                = OpenGL.GL_DOUBLE_MAT2,
    DMat3                = OpenGL.GL_DOUBLE_MAT3,
    DMat4                = OpenGL.GL_DOUBLE_MAT4,
    DMat2x3              = OpenGL.GL_DOUBLE_MAT2x3,
    DMat2x4              = OpenGL.GL_DOUBLE_MAT2x4,
    DMat3x2              = OpenGL.GL_DOUBLE_MAT3x2,
    DMat3x4              = OpenGL.GL_DOUBLE_MAT3x4,
    DMat4x2              = OpenGL.GL_DOUBLE_MAT4x2,
    DMat4x3              = OpenGL.GL_DOUBLE_MAT4x3,
    Sampler1D            = OpenGL.GL_SAMPLER_1D,
    Sampler2D            = OpenGL.GL_SAMPLER_2D,
    Sampler3D            = OpenGL.GL_SAMPLER_3D,
    SamplerCube          = OpenGL.GL_SAMPLER_CUBE,
    Sampler1DShadow      = OpenGL.GL_SAMPLER_1D_SHADOW,
    Sampler2DShadow      = OpenGL.GL_SAMPLER_2D_SHADOW,
    Sampler1DArray       = OpenGL.GL_SAMPLER_1D_ARRAY,
    Sampler2DArray       = OpenGL.GL_SAMPLER_2D_ARRAY,
    Sampler1DArrayShadow = OpenGL.GL_SAMPLER_1D_ARRAY_SHADOW,
    Sampler2DArrayShadow = OpenGL.GL_SAMPLER_2D_ARRAY_SHADOW,
    Sampler2DMS          = OpenGL.GL_SAMPLER_2D_MULTISAMPLE,
    Sampler2DMSArray     = OpenGL.GL_SAMPLER_2D_MULTISAMPLE_ARRAY,
    SamplerCubeShadow    = OpenGL.GL_SAMPLER_CUBE_SHADOW,
    SamplerBuffer        = OpenGL.GL_SAMPLER_BUFFER,
    Sampler2DRect        = OpenGL.GL_SAMPLER_2D_RECT,
    Sampler2DRectShadow  = OpenGL.GL_SAMPLER_2D_RECT_SHADOW,
    ISampler1D           = OpenGL.GL_INT_SAMPLER_1D,
    ISampler2D           = OpenGL.GL_INT_SAMPLER_2D,
    ISampler3D           = OpenGL.GL_INT_SAMPLER_3D,
    ISamplerCube         = OpenGL.GL_INT_SAMPLER_CUBE,
    ISampler1DArray      = OpenGL.GL_INT_SAMPLER_1D_ARRAY,
    ISampler2DArray      = OpenGL.GL_INT_SAMPLER_2D_ARRAY,
    ISampler2DMS         = OpenGL.GL_INT_SAMPLER_2D_MULTISAMPLE,
    ISampler2DMSArray    = OpenGL.GL_INT_SAMPLER_2D_MULTISAMPLE_ARRAY,
    ISamplerBuffer       = OpenGL.GL_INT_SAMPLER_BUFFER,
    ISampler2DRect       = OpenGL.GL_INT_SAMPLER_2D_RECT,
    USampler1D           = OpenGL.GL_UNSIGNED_INT_SAMPLER_1D,
    USampler2D           = OpenGL.GL_UNSIGNED_INT_SAMPLER_2D,
    USampler3D           = OpenGL.GL_UNSIGNED_INT_SAMPLER_3D,
    USamplerCube         = OpenGL.GL_UNSIGNED_INT_SAMPLER_CUBE,
    USampler1DArray      = OpenGL.GL_UNSIGNED_INT_SAMPLER_1D_ARRAY,
    USampler2DArray      = OpenGL.GL_UNSIGNED_INT_SAMPLER_2D_ARRAY,
    USampler2DMS         = OpenGL.GL_UNSIGNED_INT_SAMPLER_2D_MULTISAMPLE,
    USampler2DMSArray    = OpenGL.GL_UNSIGNED_INT_SAMPLER_2D_MULTISAMPLE_ARRAY,
    USamplerBuffer       = OpenGL.GL_UNSIGNED_INT_SAMPLER_BUFFER,
    USampler2DRect       = OpenGL.GL_UNSIGNED_INT_SAMPLER_2D_RECT,
    Image1D              = OpenGL.GL_IMAGE_1D,
    Image2D              = OpenGL.GL_IMAGE_2D,
    Image3D              = OpenGL.GL_IMAGE_3D,
    Image2DRect          = OpenGL.GL_IMAGE_2D_RECT,
    ImageCube            = OpenGL.GL_IMAGE_CUBE,
    ImageBuffer          = OpenGL.GL_IMAGE_BUFFER,
    Image1DArray         = OpenGL.GL_IMAGE_1D_ARRAY,
    Image2DArray         = OpenGL.GL_IMAGE_2D_ARRAY,
    Image2DMS            = OpenGL.GL_IMAGE_2D_MULTISAMPLE,
    Image2DMSArray       = OpenGL.GL_IMAGE_2D_MULTISAMPLE_ARRAY,
    IImage1D             = OpenGL.GL_INT_IMAGE_1D,
    IImage2D             = OpenGL.GL_INT_IMAGE_2D,
    IImage3D             = OpenGL.GL_INT_IMAGE_3D,
    IImage2DRect         = OpenGL.GL_INT_IMAGE_2D_RECT,
    IImageCube           = OpenGL.GL_INT_IMAGE_CUBE,
    IImageBuffer         = OpenGL.GL_INT_IMAGE_BUFFER,
    IImage1DArray        = OpenGL.GL_INT_IMAGE_1D_ARRAY,
    IImage2DArray        = OpenGL.GL_INT_IMAGE_2D_ARRAY,
    IImage2DMS           = OpenGL.GL_INT_IMAGE_2D_MULTISAMPLE,
    IImage2DMSArray      = OpenGL.GL_INT_IMAGE_2D_MULTISAMPLE_ARRAY,
    UImage1D             = OpenGL.GL_UNSIGNED_INT_IMAGE_1D,
    UImage2D             = OpenGL.GL_UNSIGNED_INT_IMAGE_2D,
    UImage3D             = OpenGL.GL_UNSIGNED_INT_IMAGE_3D,
    UImage2DRect         = OpenGL.GL_UNSIGNED_INT_IMAGE_2D_RECT,
    UImageCube           = OpenGL.GL_UNSIGNED_INT_IMAGE_CUBE,
    UImageBuffer         = OpenGL.GL_UNSIGNED_INT_IMAGE_BUFFER,
    UImage1DArray        = OpenGL.GL_UNSIGNED_INT_IMAGE_1D_ARRAY,
    UImage2DArray        = OpenGL.GL_UNSIGNED_INT_IMAGE_2D_ARRAY,
    UImage2DMS           = OpenGL.GL_UNSIGNED_INT_IMAGE_2D_MULTISAMPLE,
    UImage2DMSArray      = OpenGL.GL_UNSIGNED_INT_IMAGE_2D_MULTISAMPLE_ARRAY,
    AtomicUInt           = OpenGL.GL_UNSIGNED_INT_ATOMIC_COUNTER,
}