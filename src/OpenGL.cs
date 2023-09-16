﻿#nullable disable

using System;
using System.Runtime.InteropServices;

using GLuint = System.UInt32;
using GLenum = System.UInt32;
using GLboolean = System.Byte;
using GLbitfield = System.UInt32;
using GLbyte = System.SByte;
using GLshort = System.Int16;
using GLint = System.Int32;
using GLsizei = System.Int32;
using GLintptr = System.IntPtr;
using GLsizeiptr = System.IntPtr;
//typedef unsigned char GLubyte;
//typedef unsigned short GLushort;
//typedef unsigned int GLuint;
using GLfloat = System.Single;
//typedef float GLclampf;
//typedef double GLdouble;
//typedef double GLclampd;
//typedef void GLvoid;
using HDC = System.IntPtr;
using HGLRC = System.IntPtr;

using RR = Rhino.Runtime;

namespace RhGL
{
    /// <summary>
    /// Simple wrapper around OpenGL SDK for the needs of this project
    /// </summary>
    class OpenGL
    {

        public static bool Initialized { get; set; }
        public static bool IsAvailable { get; set; }

        public static void Initialize()
        {
            Initialized = true;
            IsAvailable = true;
            try
            {
                PlatformGL.IPlatformGetProc procBuilder = null;
                if (RR.HostUtils.RunningOnWindows)
                    procBuilder = new PlatformGL.WindowsGL();
                else
                    procBuilder = new PlatformGL.MacGL();
                _glBlendFunc = (glBlendFuncProc)procBuilder.GetProc<glBlendFuncProc>();
                _glLineWidth = (glLineWidthProc)procBuilder.GetProc<glLineWidthProc>();
                _glPointSize = (glPointSizeProc)procBuilder.GetProc<glPointSizeProc>();
                _glGetError = (glGetErrorProc)procBuilder.GetProc<glGetErrorProc>();
                _glEnable = (glEnableProc)procBuilder.GetProc<glEnableProc>();
                _glDisable = (glDisableProc)procBuilder.GetProc<glDisableProc>();
                _glIsEnabled = (glIsEnabledProc)procBuilder.GetProc<glIsEnabledProc>();
                _glDepthMask = (glDepthMaskProc)procBuilder.GetProc<glDepthMaskProc>();
                _glDrawArrays = (glDrawArraysProc)procBuilder.GetProc<glDrawArraysProc>();
                _glDrawElements = (glDrawElementsProc)procBuilder.GetProc<glDrawElementsProc>();
                _glBindTexture = (glBindTextureProc)procBuilder.GetProc<glBindTextureProc>();
                _glGenTextures = (glGenTexturesProc)procBuilder.GetProc<glGenTexturesProc>();
                _glTexImage2D = (glTexImage2DProc)procBuilder.GetProc<glTexImage2DProc>();
                _glDeleteTextures = (glDeleteTexturesProc)procBuilder.GetProc<glDeleteTexturesProc>();
                _glTexParameteri = (glTexParameteriProc)procBuilder.GetProc<glTexParameteriProc>();
                _glGenerateTextureMipMap = (glGenerateTextureMipmapProc)procBuilder.GetProc<glGenerateTextureMipmapProc>();
                _glBindBuffer = (glBindBufferProc)procBuilder.GetProc<glBindBufferProc>();
                _glDeleteBuffers = (glDeleteBuffersProc)procBuilder.GetProc<glDeleteBuffersProc>();
                _glGenBuffers = (glGenBuffersProc)procBuilder.GetProc<glGenBuffersProc>();
                _glGenQueries = (glGenQueriesProc)procBuilder.GetProc<glGenQueriesProc>();
                _glBufferData = (glBufferDataProc)procBuilder.GetProc<glBufferDataProc>();
                _glBufferSubData = (glBufferSubDataProc)procBuilder.GetProc<glBufferSubDataProc>();
                _glBeginTransformFeedback = (glBeginTransformFeedbackProc)procBuilder.GetProc<glBeginTransformFeedbackProc>();
                _glEndTransformFeedback = (glEndTransformFeedbackProc)procBuilder.GetProc<glEndTransformFeedbackProc>();
                _glTransformFeedbackVaryings = (glTransformFeedbackVaryingsProc)procBuilder.GetProc<glTransformFeedbackVaryingsProc>();
                _glBindBufferBase = (glBindBufferBaseProc)procBuilder.GetProc<glBindBufferBaseProc>();
                _glGetBufferSubData = (glGetBufferSubDataProc)procBuilder.GetProc<glGetBufferSubDataProc>();
                _glBeginQuery = (glBeginQueryProc)procBuilder.GetProc<glBeginQueryProc>();
                _glEndQuery = (glEndQueryProc)procBuilder.GetProc<glEndQueryProc>();
                _glGetQueryObjectuiv = (glGetQueryObjectuivProc)procBuilder.GetProc<glGetQueryObjectuivProc>();

                _glAttachShader = (glAttachShaderProc)procBuilder.GetProc<glAttachShaderProc>();
                _glCompileShader = (glCompileShaderProc)procBuilder.GetProc<glCompileShaderProc>();
                _glDeleteShader = (glDeleteShaderProc)procBuilder.GetProc<glDeleteShaderProc>();
                _glDeleteProgram = (glDeleteProgramProc)procBuilder.GetProc<glDeleteProgramProc>();
                _glCreateShader = (glCreateShaderProc)procBuilder.GetProc<glCreateShaderProc>();
                _glCreateProgram = (glCreateProgramProc)procBuilder.GetProc<glCreateProgramProc>();
                _glShaderSource = (glShaderSourceProc)procBuilder.GetProc<glShaderSourceProc>();
                _glUseProgram = (glUseProgramProc)procBuilder.GetProc<glUseProgramProc>();
                _glGetProgramiv = (glGetProgramivProc)procBuilder.GetProc<glGetProgramivProc>();
                _glGetShaderiv = (glGetShaderivProc)procBuilder.GetProc<glGetShaderivProc>();
                _glGetShaderInfoLog = (glGetShaderInfoLogProc)procBuilder.GetProc<glGetShaderInfoLogProc>();
                _glLinkProgram = (glLinkProgramProc)procBuilder.GetProc<glLinkProgramProc>();
                _glGetAttribLocation = (glGetAttribLocationProc)procBuilder.GetProc<glGetAttribLocationProc>();
                _glGetUniformLocation = (glGetUniformLocationProc)procBuilder.GetProc<glGetUniformLocationProc>();
                _glGetActiveAttrib = (glGetActiveAttribProc)procBuilder.GetProc<glGetActiveAttribProc>();
                _glGetActiveUniform = (glGetActiveUniformProc)procBuilder.GetProc<glGetActiveUniformProc>();
                _glDisableVertexAttribArray = (glDisableVertexAttribArrayProc)procBuilder.GetProc<glDisableVertexAttribArrayProc>();
                _glEnableVertexAttribArray = (glEnableVertexAttribArrayProc)procBuilder.GetProc<glEnableVertexAttribArrayProc>();
                _glUniform1f = (glUniform1fProc)procBuilder.GetProc<glUniform1fProc>();
                _glUniform2f = (glUniform2fProc)procBuilder.GetProc<glUniform2fProc>();
                _glUniform3f = (glUniform3fProc)procBuilder.GetProc<glUniform3fProc>();
                _glUniform4f = (glUniform4fProc)procBuilder.GetProc<glUniform4fProc>();
                _glUniform1i = (glUniform1iProc)procBuilder.GetProc<glUniform1iProc>();
                _glUniform2i = (glUniform2iProc)procBuilder.GetProc<glUniform2iProc>();
                _glUniform3i = (glUniform3iProc)procBuilder.GetProc<glUniform3iProc>();
                _glUniform4i = (glUniform4iProc)procBuilder.GetProc<glUniform4iProc>();
                _glUniform1fv = (glUniform1fvProc)procBuilder.GetProc<glUniform1fvProc>();
                _glUniform2fv = (glUniform2fvProc)procBuilder.GetProc<glUniform2fvProc>();
                _glUniform3fv = (glUniform3fvProc)procBuilder.GetProc<glUniform3fvProc>();
                _glUniform4fv = (glUniform4fvProc)procBuilder.GetProc<glUniform4fvProc>();
                _glUniform1iv = (glUniform1ivProc)procBuilder.GetProc<glUniform1ivProc>();

                _glVertexAttribI1i = (glVertexAttribI1iProc)procBuilder.GetProc<glVertexAttribI1iProc>();
                _glVertexAttribI2i = (glVertexAttribI2iProc)procBuilder.GetProc<glVertexAttribI2iProc>();
                _glVertexAttribI3i = (glVertexAttribI3iProc)procBuilder.GetProc<glVertexAttribI3iProc>();
                _glVertexAttribI4i = (glVertexAttribI4iProc)procBuilder.GetProc<glVertexAttribI4iProc>();
                _glVertexAttribPointer = (glVertexAttribPointerProc)procBuilder.GetProc<glVertexAttribPointerProc>();
                _glVertexAttrib1f = (glVertexAttrib1fProc)procBuilder.GetProc<glVertexAttrib1fProc>();
                _glVertexAttrib2f = (glVertexAttrib2fProc)procBuilder.GetProc<glVertexAttrib2fProc>();
                _glVertexAttrib3f = (glVertexAttrib3fProc)procBuilder.GetProc<glVertexAttrib3fProc>();
                _glVertexAttrib4f = (glVertexAttrib4fProc)procBuilder.GetProc<glVertexAttrib4fProc>();
                _glPatchParameteri = (glPatchParameteriProc)procBuilder.GetProc<glPatchParameteriProc>();
                _glUniformMatrix3fv = (glUniformMatrix3fvProc)procBuilder.GetProc<glUniformMatrix3fvProc>();
                _glUniformMatrix4fv = (glUniformMatrix4fvProc)procBuilder.GetProc<glUniformMatrix4fvProc>();
                _glBindVertexArray = (glBindVertexArrayProc)procBuilder.GetProc<glBindVertexArrayProc>();
                _glDeleteVertexArrays = (glDeleteVertexArraysProc)procBuilder.GetProc<glDeleteVertexArraysProc>();
                _glGenVertexArrays = (glGenVertexArraysProc)procBuilder.GetProc<glGenVertexArraysProc>();
                _glActiveTexture = (glActiveTextureProc)procBuilder.GetProc<glActiveTextureProc>();
                _glGenerateMipmap = (glGenerateMipmapProc)procBuilder.GetProc<glGenerateMipmapProc>();
                _glTexStorage2D = (glTexStorage2DProc)procBuilder.GetProc<glTexStorage2DProc>();
            }
            catch (Exception)
            {
                IsAvailable = false;
            }
        }


        public const uint GL_TRUE = 1;
        public const uint GL_FALSE = 0;

        /* BlendingFactorDest */
        public const uint GL_ZERO = 0;
        public const uint GL_ONE = 1;
        public const uint GL_SRC_COLOR = 0x0300;
        public const uint GL_ONE_MINUS_SRC_COLOR = 0x0301;
        public const uint GL_SRC_ALPHA = 0x0302;
        public const uint GL_ONE_MINUS_SRC_ALPHA = 0x0303;
        public const uint GL_DST_ALPHA = 0x0304;
        public const uint GL_ONE_MINUS_DST_ALPHA = 0x0305;


        public const uint GL_BYTE = 0x1400;
        public const uint GL_UNSIGNED_BYTE = 0x1401;
        public const uint GL_SHORT = 0x1402;
        public const uint GL_UNSIGNED_SHORT = 0x1403;
        public const uint GL_INT = 0x1404;
        public const uint GL_UNSIGNED_INT = 0x1405;
        public const uint GL_FLOAT = 0x1406;
        public const uint GL_2_BYTES = 0x1407;
        public const uint GL_3_BYTES = 0x1408;
        public const uint GL_4_BYTES = 0x1409;
        public const uint GL_DOUBLE = 0x140A;

        public const int GL_FLOAT_VEC2 = 0x8B50;
        public const int GL_FLOAT_VEC3 = 0x8B51;
        public const int GL_FLOAT_VEC4 = 0x8B52;
        public const int GL_INT_VEC2 = 0x8B53;
        public const int GL_INT_VEC3 = 0x8B54;
        public const int GL_INT_VEC4 = 0x8B55;
        public const int GL_BOOL = 0x8B56;
        public const int GL_BOOL_VEC2 = 0x8B57;
        public const int GL_BOOL_VEC3 = 0x8B58;
        public const int GL_BOOL_VEC4 = 0x8B59;
        public const int GL_FLOAT_MAT2 = 0x8B5A;
        public const int GL_FLOAT_MAT3 = 0x8B5B;
        public const int GL_FLOAT_MAT4 = 0x8B5C;
        public const int GL_SAMPLER_1D = 0x8B5D;
        public const int GL_SAMPLER_2D = 0x8B5E;
        public const int GL_SAMPLER_3D = 0x8B5F;
        public const int GL_SAMPLER_CUBE = 0x8B60;
        public const int GL_SAMPLER_1D_SHADOW = 0x8B61;
        public const int GL_SAMPLER_2D_SHADOW = 0x8B62;

        public const int GL_FLOAT_MAT2x3 = 0x8B65;
        public const int GL_FLOAT_MAT2x4 = 0x8B66;
        public const int GL_FLOAT_MAT3x2 = 0x8B67;
        public const int GL_FLOAT_MAT3x4 = 0x8B68;
        public const int GL_FLOAT_MAT4x2 = 0x8B69;
        public const int GL_FLOAT_MAT4x3 = 0x8B6A;

        public const int GL_DOUBLE_VEC2 = 0x8FFC;
        public const int GL_DOUBLE_VEC3 = 0x8FFD;
        public const int GL_DOUBLE_VEC4 = 0x8FFE;
        public const int GL_DOUBLE_MAT2 = 0x8F46;
        public const int GL_DOUBLE_MAT3 = 0x8F47;
        public const int GL_DOUBLE_MAT4 = 0x8F48;
        public const int GL_DOUBLE_MAT2x3 = 0x8F49;
        public const int GL_DOUBLE_MAT2x4 = 0x8F4A;
        public const int GL_DOUBLE_MAT3x2 = 0x8F4B;
        public const int GL_DOUBLE_MAT3x4 = 0x8F4C;
        public const int GL_DOUBLE_MAT4x2 = 0x8F4D;
        public const int GL_DOUBLE_MAT4x3 = 0x8F4E;

        public const int GL_SAMPLER_1D_ARRAY = 0x8DC0;
        public const int GL_SAMPLER_2D_ARRAY = 0x8DC1;
        public const int GL_SAMPLER_1D_ARRAY_SHADOW = 0x8DC3;
        public const int GL_SAMPLER_2D_ARRAY_SHADOW = 0x8DC4;
        public const int GL_SAMPLER_CUBE_SHADOW = 0x8DC5;
        public const int GL_UNSIGNED_INT_VEC2 = 0x8DC6;
        public const int GL_UNSIGNED_INT_VEC3 = 0x8DC7;
        public const int GL_UNSIGNED_INT_VEC4 = 0x8DC8;
        public const int GL_INT_SAMPLER_1D = 0x8DC9;
        public const int GL_INT_SAMPLER_2D = 0x8DCA;
        public const int GL_INT_SAMPLER_3D = 0x8DCB;
        public const int GL_INT_SAMPLER_CUBE = 0x8DCC;
        public const int GL_INT_SAMPLER_1D_ARRAY = 0x8DCE;
        public const int GL_INT_SAMPLER_2D_ARRAY = 0x8DCF;
        public const int GL_UNSIGNED_INT_SAMPLER_1D = 0x8DD1;
        public const int GL_UNSIGNED_INT_SAMPLER_2D = 0x8DD2;
        public const int GL_UNSIGNED_INT_SAMPLER_3D = 0x8DD3;
        public const int GL_UNSIGNED_INT_SAMPLER_CUBE = 0x8DD4;
        public const int GL_UNSIGNED_INT_SAMPLER_1D_ARRAY = 0x8DD6;
        public const int GL_UNSIGNED_INT_SAMPLER_2D_ARRAY = 0x8DD7;

        public const int GL_SAMPLER_2D_RECT = 0x8B63;
        public const int GL_SAMPLER_2D_RECT_SHADOW = 0x8B64;
        public const int GL_SAMPLER_BUFFER = 0x8DC2;
        public const int GL_INT_SAMPLER_2D_RECT = 0x8DCD;
        public const int GL_INT_SAMPLER_BUFFER = 0x8DD0;
        public const int GL_UNSIGNED_INT_SAMPLER_2D_RECT = 0x8DD5;
        public const int GL_UNSIGNED_INT_SAMPLER_BUFFER = 0x8DD8;

        public const int GL_SAMPLER_2D_MULTISAMPLE = 0x9108;
        public const int GL_INT_SAMPLER_2D_MULTISAMPLE = 0x9109;
        public const int GL_UNSIGNED_INT_SAMPLER_2D_MULTISAMPLE = 0x910A;
        public const int GL_SAMPLER_2D_MULTISAMPLE_ARRAY = 0x910B;
        public const int GL_INT_SAMPLER_2D_MULTISAMPLE_ARRAY = 0x910C;
        public const int GL_UNSIGNED_INT_SAMPLER_2D_MULTISAMPLE_ARRAY = 0x910D;

        public const int GL_UNSIGNED_INT_ATOMIC_COUNTER = 0x92DB;

        public const int GL_IMAGE_1D = 0x904C;
        public const int GL_IMAGE_2D = 0x904D;
        public const int GL_IMAGE_3D = 0x904E;
        public const int GL_IMAGE_2D_RECT = 0x904F;
        public const int GL_IMAGE_CUBE = 0x9050;
        public const int GL_IMAGE_BUFFER = 0x9051;
        public const int GL_IMAGE_1D_ARRAY = 0x9052;
        public const int GL_IMAGE_2D_ARRAY = 0x9053;
        public const int GL_IMAGE_CUBE_MAP_ARRAY = 0x9054;
        public const int GL_IMAGE_2D_MULTISAMPLE = 0x9055;
        public const int GL_IMAGE_2D_MULTISAMPLE_ARRAY = 0x9056;
        public const int GL_INT_IMAGE_1D = 0x9057;
        public const int GL_INT_IMAGE_2D = 0x9058;
        public const int GL_INT_IMAGE_3D = 0x9059;
        public const int GL_INT_IMAGE_2D_RECT = 0x905A;
        public const int GL_INT_IMAGE_CUBE = 0x905B;
        public const int GL_INT_IMAGE_BUFFER = 0x905C;
        public const int GL_INT_IMAGE_1D_ARRAY = 0x905D;
        public const int GL_INT_IMAGE_2D_ARRAY = 0x905E;
        public const int GL_INT_IMAGE_CUBE_MAP_ARRAY = 0x905F;
        public const int GL_INT_IMAGE_2D_MULTISAMPLE = 0x9060;
        public const int GL_INT_IMAGE_2D_MULTISAMPLE_ARRAY = 0x9061;
        public const int GL_UNSIGNED_INT_IMAGE_1D = 0x9062;
        public const int GL_UNSIGNED_INT_IMAGE_2D = 0x9063;
        public const int GL_UNSIGNED_INT_IMAGE_3D = 0x9064;
        public const int GL_UNSIGNED_INT_IMAGE_2D_RECT = 0x9065;
        public const int GL_UNSIGNED_INT_IMAGE_CUBE = 0x9066;
        public const int GL_UNSIGNED_INT_IMAGE_BUFFER = 0x9067;
        public const int GL_UNSIGNED_INT_IMAGE_1D_ARRAY = 0x9068;
        public const int GL_UNSIGNED_INT_IMAGE_2D_ARRAY = 0x9069;
        public const int GL_UNSIGNED_INT_IMAGE_CUBE_MAP_ARRAY = 0x906A;
        public const int GL_UNSIGNED_INT_IMAGE_2D_MULTISAMPLE = 0x906B;
        public const int GL_UNSIGNED_INT_IMAGE_2D_MULTISAMPLE_ARRAY = 0x906C;

        public const uint GL_DEPTH_TEST = 0x0B71;
        public const uint GL_VERTEX_PROGRAM_POINT_SIZE = 0x8642;
        public const uint GL_BLEND = 0x0BE2;

        public const uint GL_NO_ERROR = 0;

        public const uint GL_ARRAY_BUFFER = 0x8892;
        public const uint GL_ELEMENT_ARRAY_BUFFER = 0x8893;

        public const uint GL_TRANSFORM_FEEDBACK_BUFFER = 0x8C8E;
        public const uint GL_INTERLEAVED_ATTRIBS = 0x8C8C;
        public const uint GL_SEPARATE_ATTRIBS = 0x8C8D;

        public const uint GL_READ_ONLY = 0x88B8;
        public const uint GL_WRITE_ONLY = 0x88B9;
        public const uint GL_READ_WRITE = 0x88BA;
        public const uint GL_STREAM_DRAW = 0x88E0;
        public const uint GL_STREAM_READ = 0x88E1;
        public const uint GL_STREAM_COPY = 0x88E2;
        public const uint GL_STATIC_DRAW = 0x88E4;
        public const uint GL_STATIC_READ = 0x88E5;
        public const uint GL_STATIC_COPY = 0x88E6;
        public const uint GL_DYNAMIC_DRAW = 0x88E8;
        public const uint GL_DYNAMIC_READ = 0x88E9;
        public const uint GL_DYNAMIC_COPY = 0x88EA;

        public const uint GL_TRANSFORM_FEEDBACK_PRIMITIVES_WRITTEN = 0x8C88;
        public const uint GL_PRIMITIVES_GENERATED = 0x8C87;
        public const uint GL_QUERY_RESULT = 0x8866;


        public const int GL_FRAGMENT_SHADER = 0x8B30;
        public const int GL_VERTEX_SHADER = 0x8B31;
        public const int GL_GEOMETRY_SHADER = 0x8DD9;
        public const int GL_TESS_EVALUATION_SHADER = 0x8E87;
        public const int GL_TESS_CONTROL_SHADER = 0x8E88;

        //DRAW MODE
        public const uint GL_POINTS = 0x0000;
        public const uint GL_LINES = 0x0001;
        public const uint GL_LINE_LOOP = 0x0002;
        public const uint GL_LINE_STRIP = 0x0003;
        public const uint GL_TRIANGLES = 0x0004;
        public const uint GL_TRIANGLE_STRIP = 0x0005;
        public const uint GL_TRIANGLE_FAN = 0x0006;
        public const uint GL_QUADS = 0x0007;
        public const uint GL_QUAD_STRIP = 0x0008;
        public const uint GL_POLYGON = 0x0009;
        public const uint GL_LINES_ADJACENCY = 0x000A;
        public const uint GL_LINE_STRIP_ADJACENCY = 0x000B;
        public const uint GL_TRIANGLES_ADJACENCY = 0x000C;
        public const uint GL_TRIANGLE_STRIP_ADJACENCY = 0x000D;
        public const uint GL_PATCHES = 0x000E;

        public const uint GL_PATCH_VERTICES = 0x8E72;


        public const GLuint GL_DELETE_STATUS = 0x8B80;
        public const GLuint GL_COMPILE_STATUS = 0x8B81;
        public const GLuint GL_LINK_STATUS = 0x8B82;
        public const GLuint GL_VALIDATE_STATUS = 0x8B83;
        public const GLuint GL_INFO_LOG_LENGTH = 0x8B84;


        public const uint GL_TEXTURE0 = 0x84C0;
        public const uint GL_TEXTURE1 = 0x84C1;
        public const uint GL_TEXTURE2 = 0x84C2;
        public const uint GL_TEXTURE3 = 0x84C3;
        public const uint GL_TEXTURE4 = 0x84C4;
        public const uint GL_TEXTURE5 = 0x84C5;
        public const uint GL_TEXTURE6 = 0x84C6;
        public const uint GL_TEXTURE7 = 0x84C7;
        public const uint GL_TEXTURE8 = 0x84C8;
        public const uint GL_TEXTURE9 = 0x84C9;
        public const uint GL_TEXTURE10 = 0x84CA;
        public const uint GL_TEXTURE11 = 0x84CB;
        public const uint GL_TEXTURE12 = 0x84CC;
        public const uint GL_TEXTURE13 = 0x84CD;
        public const uint GL_TEXTURE14 = 0x84CE;
        public const uint GL_TEXTURE15 = 0x84CF;
        public const uint GL_TEXTURE16 = 0x84D0;
        public const uint GL_TEXTURE17 = 0x84D1;
        public const uint GL_TEXTURE18 = 0x84D2;
        public const uint GL_TEXTURE19 = 0x84D3;
        public const uint GL_TEXTURE20 = 0x84D4;
        public const uint GL_TEXTURE21 = 0x84D5;
        public const uint GL_TEXTURE22 = 0x84D6;
        public const uint GL_TEXTURE23 = 0x84D7;
        public const uint GL_TEXTURE24 = 0x84D8;
        public const uint GL_TEXTURE25 = 0x84D9;
        public const uint GL_TEXTURE26 = 0x84DA;
        public const uint GL_TEXTURE27 = 0x84DB;
        public const uint GL_TEXTURE28 = 0x84DC;
        public const uint GL_TEXTURE29 = 0x84DD;
        public const uint GL_TEXTURE30 = 0x84DE;
        public const uint GL_TEXTURE31 = 0x84DF;
        public const uint GL_ACTIVE_TEXTURE = 0x84E0;

        public const uint GL_TEXTURE_1D = 0x0DE0;
        public const uint GL_TEXTURE_2D = 0x0DE1;
        public const uint GL_RGB = 0x1907;
        public const uint GL_RGBA = 0x1908;
        public const uint GL_BGR = 0x80E0;
        public const uint GL_BGRA = 0x80E1;
        public const uint GL_RGB8 = 0x8051;
        public const uint GL_RGBA8 = 0x8058;

        public const uint GL_TEXTURE_MAG_FILTER = 0x2800;
        public const uint GL_TEXTURE_MIN_FILTER = 0x2801;
        public const uint GL_TEXTURE_WRAP_S = 0x2802;
        public const uint GL_TEXTURE_WRAP_T = 0x2803;
        public const uint GL_NEAREST = 0x2600;
        public const uint GL_LINEAR = 0x2601;
        public const uint GL_NEAREST_MIPMAP_NEAREST = 0x2700;
        public const uint GL_LINEAR_MIPMAP_NEAREST = 0x2701;
        public const uint GL_NEAREST_MIPMAP_LINEAR = 0x2702;
        public const uint GL_LINEAR_MIPMAP_LINEAR = 0x2703;
        public const uint GL_CLAMP = 0x2900;
        public const uint GL_REPEAT = 0x2901;

        public const int GL_ACTIVE_UNIFORMS = 0x8B86;
        public const int GL_ACTIVE_UNIFORM_MAX_LENGTH = 0x8B87;
        public const int GL_ACTIVE_ATTRIBUTES = 0x8B89;
        public const int GL_ACTIVE_ATTRIBUTE_MAX_LENGTH = 0x8B8A;


        delegate void glBlendFuncProc(GLenum sfactor, GLenum dfactor);
        static glBlendFuncProc _glBlendFunc;
        public static void glBlendFunc(GLenum sfactor, GLenum dfactor)
        {
            _glBlendFunc(sfactor, dfactor);
        }

        delegate void glLineWidthProc(float width);
        static glLineWidthProc _glLineWidth;
        public static void glLineWidth(float width)
        {
            _glLineWidth(width);
        }

        delegate void glPointSizeProc(float size);
        static glPointSizeProc _glPointSize;
        public static void glPointSize(float size)
        {
            _glPointSize(size);
        }

        delegate GLenum glGetErrorProc();
        static glGetErrorProc _glGetError;
        public static GLenum glGetError()
        {
            return _glGetError();
        }

        delegate void glEnableProc(GLenum cap);
        static glEnableProc _glEnable;
        public static void glEnable(GLenum cap)
        {
            _glEnable(cap);
        }

        delegate void glDisableProc(GLenum cap);
        static glDisableProc _glDisable;
        public static void glDisable(GLenum cap)
        {
            _glDisable(cap);
        }

        delegate GLboolean glIsEnabledProc(GLenum cap);
        static glIsEnabledProc _glIsEnabled;
        public static GLboolean glIsEnabled(GLenum cap)
        {
            return _glIsEnabled(cap);
        }

        public static bool IsEnabled(GLenum cap)
        {
            return glIsEnabled(cap) != 0;
        }


        delegate void glDepthMaskProc(GLboolean flag);
        static glDepthMaskProc _glDepthMask;
        public static void glDepthMask(GLboolean flag)
        {
            _glDepthMask(flag);
        }

        delegate void glDrawArraysProc(GLenum mode, GLint first, GLsizei count);
        static glDrawArraysProc _glDrawArrays;
        public static void glDrawArrays(GLenum mode, GLint first, GLsizei count)
        {
            _glDrawArrays(mode, first, count);
        }

        delegate void glDrawElementsProc(GLenum mode, GLsizei count, GLenum type, System.IntPtr indices);
        static glDrawElementsProc _glDrawElements;
        public static void glDrawElements(GLenum mode, GLsizei count, GLenum type, System.IntPtr indices)
        {
            _glDrawElements(mode, count, type, indices);
        }

        delegate void glBindTextureProc(GLenum target, GLuint texture);
        static glBindTextureProc _glBindTexture;
        public static void glBindTexture(GLenum target, GLuint texture)
        {
            _glBindTexture(target, texture);
        }

        delegate void glGenTexturesProc(GLsizei n, [MarshalAs(UnmanagedType.LPArray)]GLuint[] textures);
        static glGenTexturesProc _glGenTextures;
        public static void glGenTextures(GLsizei n, GLuint[] textures)
        {
            _glGenTextures(n, textures);
        }

        delegate void glTexImage2DProc(GLenum target, GLint level, GLint internalformat, GLsizei width, GLsizei height, GLint border, GLenum format, GLenum type, IntPtr pixels);
        static glTexImage2DProc _glTexImage2D;
        public static void glTexImage2D(GLenum target, GLint level, GLint internalformat, GLsizei width, GLsizei height, GLint border, GLenum format, GLenum type, IntPtr pixels)
        {
            _glTexImage2D(target, level, internalformat, width, height, border, format, type, pixels);
        }

        delegate void glDeleteTexturesProc(GLsizei n, [MarshalAs(UnmanagedType.LPArray)]GLuint[] textures);
        static glDeleteTexturesProc _glDeleteTextures;
        public static void glDeleteTextures(GLsizei n, GLuint[] textures)
        {
            _glDeleteTextures(n, textures);
        }

        delegate void glTexParameteriProc(GLenum target, GLenum pname, GLint param);
        static glTexParameteriProc _glTexParameteri;
        public static void glTexParameteri(GLenum target, GLenum pname, GLint param)
        {
            _glTexParameteri(target, pname, param);
        }

        delegate void glGenerateTextureMipmapProc(GLuint texture);
        static glGenerateTextureMipmapProc _glGenerateTextureMipMap;
        public static void glGenerateTextureMipmap(GLuint texture)
        {
            _glGenerateTextureMipMap(texture);
        }

        delegate void glBindBufferProc(GLenum target, GLuint buffer);
        static glBindBufferProc _glBindBuffer;
        public static void glBindBuffer(GLenum target, GLuint buffer)
        {
            _glBindBuffer(target, buffer);
        }

        delegate void glDeleteBuffersProc(GLsizei n, GLuint[] buffers);
        static glDeleteBuffersProc _glDeleteBuffers;
        public static void glDeleteBuffers(GLsizei n, GLuint[] buffers)
        {
            _glDeleteBuffers(n, buffers);
        }

        delegate void glGenBuffersProc(GLsizei n, [MarshalAs(UnmanagedType.LPArray)]GLuint[] buffers);
        static glGenBuffersProc _glGenBuffers;
        public static void glGenBuffers(GLsizei n, out GLuint[] buffers)
        {
            buffers = new GLuint[n];
            _glGenBuffers(n, buffers);
        }

        delegate void glGenQueriesProc(GLsizei n, [MarshalAs(UnmanagedType.LPArray)] GLuint[] buffers);
        static glGenQueriesProc _glGenQueries;
        public static void glGenQueries(GLsizei n, out GLuint[] buffers)
        {
            buffers = new GLuint[n];
            _glGenQueries(n, buffers);
        }

        delegate void glBufferDataProc(GLenum target, GLsizeiptr size, IntPtr data, GLenum usage);
        static glBufferDataProc _glBufferData;
        public static void glBufferData(GLenum target, GLsizeiptr size, IntPtr data, GLenum usage)
        {
            _glBufferData(target, size, data, usage);
        }

        delegate void glBufferSubDataProc(GLenum target, GLintptr offset, GLsizeiptr size, IntPtr data);
        static glBufferSubDataProc _glBufferSubData;
        public static void glBufferSubData(GLenum target, GLintptr offset, GLsizeiptr size, IntPtr data)
        {
            _glBufferSubData(target, offset, size, data);
        }

        delegate void glBeginTransformFeedbackProc(GLenum primitiveMode);
        static glBeginTransformFeedbackProc _glBeginTransformFeedback;
        public static void glBeginTransformFeedback(GLenum primitiveMode)
        {
            _glBeginTransformFeedback(primitiveMode);
        }

        delegate void glEndTransformFeedbackProc();
        static glEndTransformFeedbackProc _glEndTransformFeedback;
        public static void glEndTransformFeedback()
        {
            _glEndTransformFeedback();
        }

        delegate void glTransformFeedbackVaryingsProc(GLuint program, GLsizei count, string[] varyings, GLenum bufferMode);
        static glTransformFeedbackVaryingsProc _glTransformFeedbackVaryings;
        public static void glTransformFeedbackVaryings(GLuint program, GLsizei count, string[] varyings, GLenum bufferMode)
        {
            _glTransformFeedbackVaryings(program, count, varyings, bufferMode);
        }

        delegate void glBindBufferBaseProc(GLenum target, GLuint index, GLuint buffer);
        static glBindBufferBaseProc _glBindBufferBase;
        public static void glBindBufferBase(GLenum target, GLuint index, GLuint buffer)
        {
            _glBindBufferBase(target, index, buffer);
        }

        delegate void glGetBufferSubDataProc(GLenum target, GLintptr offset, GLsizeiptr size, IntPtr data);
        static glGetBufferSubDataProc _glGetBufferSubData;
        public static void glGetBufferSubData(GLenum target, GLintptr offset, GLsizeiptr size, IntPtr data)
        {
            _glGetBufferSubData(target, offset, size, data);
        }

        delegate void glBeginQueryProc(GLenum target, GLuint id);
        static glBeginQueryProc _glBeginQuery;
        public static void glBeginQuery(GLenum target, GLuint id)
        {
            _glBeginQuery(target, id);
        }

        delegate void glEndQueryProc(GLenum target);
        static glEndQueryProc _glEndQuery;
        public static void glEndQuery(GLenum target)
        {
            _glEndQuery(target);
        }

        delegate void glGetQueryObjectuivProc(GLuint id, GLenum pname, [MarshalAs(UnmanagedType.LPArray)] GLuint[] _params);
        static glGetQueryObjectuivProc _glGetQueryObjectuiv;
        public static void glGetQueryObjectuiv(GLuint id, GLenum pname, out GLuint[] _params)
        {
            _params = new GLuint[1];
            _glGetQueryObjectuiv(id, pname, _params);
        }

        delegate void glAttachShaderProc(GLuint program, GLuint shader);
        static glAttachShaderProc _glAttachShader;
        public static void glAttachShader(GLuint program, GLuint shader)
        {
            _glAttachShader(program, shader);
        }

        delegate void glCompileShaderProc(GLuint shader);
        static glCompileShaderProc _glCompileShader;
        public static void glCompileShader(GLuint shader)
        {
           _glCompileShader(shader);
        }

        delegate void glDeleteShaderProc(GLuint shader);
        static glDeleteShaderProc _glDeleteShader;
        public static void glDeleteShader(GLuint shader)
        {
            _glDeleteShader(shader);
        }

        delegate void glDeleteProgramProc(GLuint shader);
        static glDeleteProgramProc _glDeleteProgram;
        public static void glDeleteProgram(GLuint program)
        {
            _glDeleteProgram(program);
        }

        delegate GLuint glCreateShaderProc(GLenum type);
        static glCreateShaderProc _glCreateShader;
        public static GLuint glCreateShader(GLenum type)
        {
            return _glCreateShader(type);
        }

        delegate GLuint glCreateProgramProc();
        static glCreateProgramProc _glCreateProgram;
        public static GLuint glCreateProgram()
        {
            return _glCreateProgram();
        }

        delegate void glShaderSourceProc(GLuint shader, GLsizei count, string[] source, GLint[] length);
        static glShaderSourceProc _glShaderSource;
        public static void glShaderSource(GLuint shader, GLsizei count, string[] source, GLint[] length)
        {
            _glShaderSource(shader, count, source, length);
        }

        delegate void glUseProgramProc(GLuint program);
        static glUseProgramProc _glUseProgram;
        public static void glUseProgram(GLuint program)
        {
            _glUseProgram(program);
        }

        // void  (*glGetProgramiv) (GLuint  program, GLenum  pname, GLint * params);
        delegate void glGetProgramivProc(GLuint program, GLenum pname, out GLint parameter);
        static glGetProgramivProc _glGetProgramiv;
        public static void glGetProgramiv(GLuint shader, GLenum pname, out GLint parameter)
        {
            _glGetProgramiv(shader, pname, out parameter);
        }

        // void  (*glGetShaderiv) (GLuint  shader, GLenum  pname, GLint * params);
        delegate void glGetShaderivProc(GLuint shader, GLenum pname, out GLint parameter);
        static glGetShaderivProc _glGetShaderiv;
        public static void glGetShaderiv(GLuint shader, GLenum pname, out GLint parameter)
        {
            _glGetShaderiv(shader, pname, out parameter);
        }

        delegate void glGetShaderInfoLogProc(GLuint shader, GLsizei bufSize, out GLsizei length, System.Text.StringBuilder infoLog);
        static glGetShaderInfoLogProc _glGetShaderInfoLog;
        public static void glGetShaderInfoLog(GLuint shader, GLsizei bufSize, out GLsizei length, System.Text.StringBuilder infoLog)
        {
            _glGetShaderInfoLog(shader, bufSize, out length, infoLog);
        }

        delegate void glLinkProgramProc(GLuint program);
        static glLinkProgramProc _glLinkProgram;
        public static void glLinkProgram(GLuint program)
        {
            _glLinkProgram(program);
        }

        delegate GLint glGetAttribLocationProc(GLuint program, string name);
        static glGetAttribLocationProc _glGetAttribLocation;
        public static GLint glGetAttribLocation(GLuint program, string name)
        {
            return _glGetAttribLocation(program, name);
        }

        delegate GLint glGetUniformLocationProc(GLuint program, string name);
        static glGetUniformLocationProc _glGetUniformLocation;
        public static GLint glGetUniformLocation(GLuint program, string name)
        {
            return _glGetUniformLocation(program, name);
        }

        delegate void glGetActiveAttribProc (GLuint  program, GLuint  index, GLsizei  bufSize, out GLsizei length, out GLint size, out GLenum type, System.Text.StringBuilder name);
        static glGetActiveAttribProc _glGetActiveAttrib;
        public static void glGetActiveAttrib (GLuint  program, GLuint  index, GLsizei  bufSize, out GLsizei length, out GLint size, out GLenum type, System.Text.StringBuilder name)
        {
            _glGetActiveAttrib (program, index, bufSize, out length, out size, out type, name);
        }

        delegate void glGetActiveUniformProc (GLuint  program, GLuint  index, GLsizei  bufSize, out GLsizei length, out GLint size, out GLenum type, System.Text.StringBuilder name);
        static glGetActiveUniformProc _glGetActiveUniform;
        public static void glGetActiveUniform (GLuint  program, GLuint  index, GLsizei  bufSize, out GLsizei length, out GLint size, out GLenum type, System.Text.StringBuilder name)
        {
            _glGetActiveUniform (program, index, bufSize, out length, out size, out type, name);
        }

        delegate void glDisableVertexAttribArrayProc(GLuint index);
        static glDisableVertexAttribArrayProc _glDisableVertexAttribArray;
        public static void glDisableVertexAttribArray(GLuint index)
        {
            _glDisableVertexAttribArray(index);
        }

        delegate void glEnableVertexAttribArrayProc(GLuint index);
        static glEnableVertexAttribArrayProc _glEnableVertexAttribArray;
        public static void glEnableVertexAttribArray(GLuint index)
        {
            _glEnableVertexAttribArray(index);
        }

        delegate void glUniform1fProc(GLint location, GLfloat v0);
        static glUniform1fProc _glUniform1f;
        public static void glUniform1f(GLint location, GLfloat v0)
        {
            _glUniform1f(location, v0);
        }

        delegate void glUniform2fProc(GLint location, GLfloat v0, GLfloat v1);
        static glUniform2fProc _glUniform2f;
        public static void glUniform2f(GLint location, GLfloat v0, GLfloat v1)
        {
            _glUniform2f(location, v0, v1);
        }

        delegate void glUniform3fProc(GLint location, GLfloat v0, GLfloat v1, GLfloat v2);
        static glUniform3fProc _glUniform3f;
        public static void glUniform3f(GLint location, GLfloat v0, GLfloat v1, GLfloat v2)
        {
            _glUniform3f(location, v0, v1, v2);
        }

        delegate void glUniform4fProc(GLint location, GLfloat v0, GLfloat v1, GLfloat v2, GLfloat v3);
        static glUniform4fProc _glUniform4f;
        public static void glUniform4f(GLint location, GLfloat v0, GLfloat v1, GLfloat v2, GLfloat v3)
        {
            _glUniform4f(location, v0, v1, v2, v3);
        }

        delegate void glUniform1iProc(GLint location, GLint v0);
        static glUniform1iProc _glUniform1i;
        public static void glUniform1i(GLint location, GLint v0)
        {
            _glUniform1i(location, v0);
        }

        delegate void glUniform2iProc(GLint location, GLint v0, GLint v1);
        static glUniform2iProc _glUniform2i;
        public static void glUniform2i(GLint location, GLint v0, GLint v1)
        {
            _glUniform2i(location, v0, v1);
        }

        delegate void glUniform3iProc(GLint location, GLint v0, GLint v1, GLint v2);
        static glUniform3iProc _glUniform3i;
        public static void glUniform3i(GLint location, GLint v0, GLint v1, GLint v2)
        {
            _glUniform3i(location, v0, v1, v2);
        }

        delegate void glUniform4iProc(GLint location, GLint v0, GLint v1, GLint v2, GLint v3);
        static glUniform4iProc _glUniform4i;
        public static void glUniform4i(GLint location, GLint v0, GLint v1, GLint v2, GLint v3)
        {
            _glUniform4i(location, v0, v1, v2, v3);
        }

        delegate void glUniform1fvProc(GLint location, GLsizei count, [MarshalAs(UnmanagedType.LPArray)]GLfloat[] value);
        static glUniform1fvProc _glUniform1fv;
        public static void glUniform1fv(GLint location, GLsizei count, GLfloat[] value)
        {
            _glUniform1fv(location, count, value);
        }

        delegate void glUniform2fvProc(GLint location, GLsizei count, [MarshalAs(UnmanagedType.LPArray)]GLfloat[] value);
        static glUniform2fvProc _glUniform2fv;
        public static void glUniform2fv(GLint location, GLsizei count, GLfloat[] value)
        {
            _glUniform2fv(location, count, value);
        }

        delegate void glUniform3fvProc(GLint location, GLsizei count, [MarshalAs(UnmanagedType.LPArray)]GLfloat[] value);
        static glUniform3fvProc _glUniform3fv;
        public static void glUniform3fv(GLint location, GLsizei count, GLfloat[] value)
        {
            _glUniform3fv(location, count, value);
        }

        delegate void glUniform4fvProc(GLint location, GLsizei count, [MarshalAs(UnmanagedType.LPArray)]GLfloat[] value);
        static glUniform4fvProc _glUniform4fv;
        public static void glUniform4fv(GLint location, GLsizei count, GLfloat[] value)
        {
            _glUniform4fv(location, count, value);
        }

        delegate void glUniform1ivProc(GLint location, GLsizei count, [MarshalAs(UnmanagedType.LPArray)]GLint[] value);
        static glUniform1ivProc _glUniform1iv;
        public static void glUniform1iv(GLint location, GLsizei count, GLint[] value)
        {
            _glUniform1iv(location, count, value);
        }



        delegate void glVertexAttribI1iProc(GLuint index, GLint x);
        static glVertexAttribI1iProc _glVertexAttribI1i;
        public static void glVertexAttribI1i(GLuint index, GLint x)
        {
            _glVertexAttribI1i(index, x);
        }

        delegate void glVertexAttribI2iProc(GLuint index, GLint x, GLint y);
        static glVertexAttribI2iProc _glVertexAttribI2i;
        public static void glVertexAttribI2i(GLuint index, GLint x, GLint y)
        {
            _glVertexAttribI2i(index, x, y);
        }

        delegate void glVertexAttribI3iProc(GLuint index, GLint x, GLint y, GLint z);
        static glVertexAttribI3iProc _glVertexAttribI3i;
        public static void glVertexAttribI3i(GLuint index, GLint x, GLint y, GLint z)
        {
            _glVertexAttribI3i(index, x, y, z);
        }

        delegate void glVertexAttribI4iProc(GLuint index, GLint x, GLint y, GLint z, GLint w);
        static glVertexAttribI4iProc _glVertexAttribI4i;
        public static void glVertexAttribI4i(GLuint index, GLint x, GLint y, GLint z, GLint w)
        {
            _glVertexAttribI4i(index, x, y, z, w);
        }

        delegate void glVertexAttrib1fProc(GLuint index, GLfloat x);
        static glVertexAttrib1fProc _glVertexAttrib1f;
        public static void glVertexAttrib1f(GLuint index, GLfloat x)
        {
            _glVertexAttrib1f(index, x);
        }

        delegate void glVertexAttrib2fProc(GLuint index, GLfloat x, GLfloat y);
        static glVertexAttrib2fProc _glVertexAttrib2f;
        public static void glVertexAttrib2f(GLuint index, GLfloat x, GLfloat y)
        {
            _glVertexAttrib2f(index, x, y);
        }

        delegate void glVertexAttrib3fProc(GLuint index, GLfloat x, GLfloat y, GLfloat z);
        static glVertexAttrib3fProc _glVertexAttrib3f;
        public static void glVertexAttrib3f(GLuint index, GLfloat x, GLfloat y, GLfloat z)
        {
            _glVertexAttrib3f(index, x, y, z);
        }

        delegate void glVertexAttrib4fProc(GLuint index, GLfloat x, GLfloat y, GLfloat z, GLfloat w);
        static glVertexAttrib4fProc _glVertexAttrib4f;
        public static void glVertexAttrib4f(GLuint index, GLfloat x, GLfloat y, GLfloat z, GLfloat w)
        {
            _glVertexAttrib4f(index, x, y, z, w);
        }

        delegate void glVertexAttribPointerProc(GLuint index, GLint size, GLenum type, GLboolean normalized, GLsizei stride, IntPtr pointer);
        static glVertexAttribPointerProc _glVertexAttribPointer;
        public static void glVertexAttribPointer(GLuint index, GLint size, GLenum type, GLboolean normalized, GLsizei stride, IntPtr pointer)
        {
            _glVertexAttribPointer(index, size, type, normalized, stride, pointer);
        }

        delegate void glPatchParameteriProc(GLenum pname, GLint value);
        static glPatchParameteriProc _glPatchParameteri;
        public static void glPatchParameteri(GLenum pname, GLint value)
        {
            _glPatchParameteri(pname, value);
        }

        delegate void glUniformMatrix3fvProc(GLint location, GLsizei count, GLboolean transpose, [MarshalAs(UnmanagedType.LPArray)]GLfloat[] value);
        static glUniformMatrix3fvProc _glUniformMatrix3fv;
        public static void glUniformMatrix3fv(GLint location, GLsizei count, bool transpose, GLfloat[] value)
        {
            _glUniformMatrix3fv(location, count, transpose ? (byte)1 : (byte)0, value);
        }

        delegate void glUniformMatrix4fvProc(GLint location, GLsizei count, GLboolean transpose, [MarshalAs(UnmanagedType.LPArray)]GLfloat[] value);
        static glUniformMatrix4fvProc _glUniformMatrix4fv;
        public static void glUniformMatrix4fv(GLint location, GLsizei count, bool transpose, GLfloat[] value)
        {
            _glUniformMatrix4fv(location, count, transpose ? (byte)1 : (byte)0, value);
        }

        delegate void glBindVertexArrayProc(GLuint array);
        static glBindVertexArrayProc _glBindVertexArray;
        public static void glBindVertexArray(GLuint array)
        {
            _glBindVertexArray(array);
        }

        delegate void glDeleteVertexArraysProc(GLsizei n, GLuint[] arrays);
        static glDeleteVertexArraysProc _glDeleteVertexArrays;
        public static void glDeleteVertexArrays(GLsizei n, GLuint[] arrays)
        {
            _glDeleteVertexArrays(n, arrays);
        }

        delegate void glGenVertexArraysProc(GLsizei n, [MarshalAs(UnmanagedType.LPArray)]GLuint[] arrays);
        static glGenVertexArraysProc _glGenVertexArrays;
        public static void glGenVertexArrays(GLsizei n, out GLuint[] arrays)
        {
            arrays = new GLuint[n];
            _glGenVertexArrays(n, arrays);
        }

        delegate void glActiveTextureProc(GLenum id);
        static glActiveTextureProc _glActiveTexture;
        public static void glActiveTexture(GLenum id)
        {
            _glActiveTexture(id);
        }

        delegate void glGenerateMipmapProc(GLenum id);
        static glGenerateMipmapProc _glGenerateMipmap;
        public static void glGenerateMipmap(GLenum id)
        {
            _glGenerateMipmap(id);
        }

        delegate void glTexStorage2DProc(GLenum target,
                                         GLsizei levels,
                                         GLenum internalformat,
                                         GLsizei width,
                                         GLsizei height);
        static glTexStorage2DProc _glTexStorage2D;
        public static void glTexStorage2D(GLenum target,
                                         GLsizei levels,
                                         GLenum internalformat,
                                         GLsizei width,
                                         GLsizei height)
        {
            _glTexStorage2D(target, levels, internalformat, width, height);
        }


        //    glActiveTexture(GL_TEXTURE0);
        //    glBindTexture(GL_TEXTURE_2D, Base);
        public static HGLRC wglGetCurrentContext()
        {
            if (RR.HostUtils.RunningOnWindows)
                return PlatformGL.WindowsGL.wglGetCurrentContext();
            return IntPtr.Zero;
        }

        public static bool wglMakeCurrent(HDC hdc, HGLRC hglrc)
        {
            if (RR.HostUtils.RunningOnWindows)
                return PlatformGL.WindowsGL.wglMakeCurrent(hdc, hglrc);
            return false;
        }

        public static HDC GetDC(IntPtr hWnd)
        {
            if (RR.HostUtils.RunningOnWindows)
                return PlatformGL.WindowsGL.GetDC(hWnd);
            return IntPtr.Zero;
        }


        public static bool ErrorOccurred(out string errorMessage)
        {
            errorMessage = "";
            GLenum nGLError = glGetError();
            bool error_occurred = (nGLError != GL_NO_ERROR);
            if (error_occurred)
            {
                string glerrStr = "";

                if (nGLError == 0x0506)
                    glerrStr = "Invalid Framebuffer operation";
                else
                {
                    if (RR.HostUtils.RunningOnWindows)
                    {
                        byte[] err = PlatformGL.WindowsGL.gluErrorString(nGLError);
                        glerrStr = System.Text.Encoding.ASCII.GetString(err);
                    }
                }
                errorMessage = $"OpenGL error {nGLError} : {glerrStr}";
            }
            return error_occurred;
        }
    }
}

namespace RhGL.PlatformGL
{
    interface IPlatformGetProc
    {
        Delegate GetProc<T>();
    }

    class WindowsGL : IPlatformGetProc
    {
        System.Collections.Generic.Dictionary<string, System.Reflection.MethodInfo> _glMethods;

        public Delegate GetProc<T>()
        {
            if (_glMethods == null)
            {
                _glMethods = new System.Collections.Generic.Dictionary<string, System.Reflection.MethodInfo>();
                var methods = GetType().GetMethods();
                foreach (var method in methods)
                {
                    if (method.Name.StartsWith("gl", StringComparison.Ordinal))
                        _glMethods.Add(method.Name, method);
                }
            }

            string name = typeof(T).Name;
            name = name.Substring(0, name.Length - "Proc".Length);

            // Check to see if a static function in this class exists with the name
            if (_glMethods.ContainsKey(name))
            {
                return _glMethods[name].CreateDelegate(typeof(T));
            }

            IntPtr ptr = wglGetProcAddress(name);
            return Marshal.GetDelegateForFunctionPointer(ptr, typeof(T));
        }
        const string OPENGL_LIB = "opengl32.dll";


        [DllImport(OPENGL_LIB)]
        public static extern IntPtr wglGetProcAddress(string function);

        [DllImport(OPENGL_LIB)]
        public static extern void glBlendFunc(GLenum sfactor, GLenum dfactor);

        [DllImport(OPENGL_LIB)]
        public static extern void glLineWidth(float width);

        [DllImport(OPENGL_LIB)]
        public static extern void glPointSize(float size);

        [DllImport(OPENGL_LIB)]
        public static extern GLenum glGetError();

        [DllImport(OPENGL_LIB)]
        public static extern void glEnable(GLenum cap);

        [DllImport(OPENGL_LIB)]
        public static extern void glDisable(GLenum cap);

        [DllImport(OPENGL_LIB)]
        public static extern GLboolean glIsEnabled(GLenum cap);

        [DllImport(OPENGL_LIB)]
        public static extern void glDepthMask(GLboolean flag);

        [DllImport(OPENGL_LIB)]
        public static extern void glDrawArrays(GLenum mode, GLint first, GLsizei count);

        [DllImport(OPENGL_LIB)]
        public static extern void glDrawElements(GLenum mode, GLsizei count, GLenum type, System.IntPtr indices);

        [DllImport(OPENGL_LIB)]
        public static extern void glBindTexture(GLenum target, GLuint texture);

        [DllImport(OPENGL_LIB)]
        public static extern void glGenTextures(GLsizei n, [MarshalAs(UnmanagedType.LPArray)]GLuint[] textures);

        [DllImport(OPENGL_LIB)]
        public static extern void glTexImage2D(GLenum target, GLint level, GLint internalformat, GLsizei width, GLsizei height, GLint border, GLenum format, GLenum type, IntPtr pixels);

        [DllImport(OPENGL_LIB)]
        public static extern void glDeleteTextures(GLsizei n, [MarshalAs(UnmanagedType.LPArray)]GLuint[] textures);

        [DllImport(OPENGL_LIB)]
        public static extern void glTexParameteri(GLenum target, GLenum pname, GLint param);

        [DllImport(OPENGL_LIB)]
        public static extern void glGenerateTextureMipmap(GLuint texture);

        [DllImport(OPENGL_LIB)]
        public static extern HGLRC wglGetCurrentContext();

        [DllImport(OPENGL_LIB)]
        public static extern bool wglMakeCurrent(HDC hdc, HGLRC hglrc);

        [DllImport("User32.dll")]
        public static extern HDC GetDC(IntPtr hWnd);

        [DllImport("gdi32.dll")]
        public static extern byte[] gluErrorString(uint errCode);
    }


    class MacGL : IPlatformGetProc
    {
        System.Collections.Generic.Dictionary<string, System.Reflection.MethodInfo> _glMethods;

        public Delegate GetProc<T>()
        {
            if (_glMethods == null)
            {
                _glMethods = new System.Collections.Generic.Dictionary<string, System.Reflection.MethodInfo>();
                var methods = GetType().GetMethods();
                foreach (var method in methods)
                {
                    if (method.Name.StartsWith("gl", StringComparison.Ordinal))
                        _glMethods.Add(method.Name, method);
                }
            }

            string name = typeof(T).Name;
            name = name.Substring(0, name.Length - "Proc".Length);

            // Check to see if a static function in this class exists with the name
            if (_glMethods.ContainsKey(name))
            {
                return _glMethods[name].CreateDelegate(typeof(T));
            }
            return null;
        }
        const string OPENGL_LIB = "__Internal";


        [DllImport(OPENGL_LIB)]
        public static extern void glBlendFunc(GLenum sfactor, GLenum dfactor);

        [DllImport(OPENGL_LIB)]
        public static extern void glLineWidth(float width);

        [DllImport(OPENGL_LIB)]
        public static extern void glPointSize(float size);

        [DllImport(OPENGL_LIB)]
        public static extern GLenum glGetError();

        [DllImport(OPENGL_LIB)]
        public static extern void glEnable(GLenum cap);

        [DllImport(OPENGL_LIB)]
        public static extern void glDisable(GLenum cap);

        [DllImport(OPENGL_LIB)]
        public static extern GLboolean glIsEnabled(GLenum cap);

        [DllImport(OPENGL_LIB)]
        public static extern void glDepthMask(GLboolean flag);

        [DllImport(OPENGL_LIB)]
        public static extern void glDrawArrays(GLenum mode, GLint first, GLsizei count);

        [DllImport(OPENGL_LIB)]
        public static extern void glDrawElements(GLenum mode, GLsizei count, GLenum type, System.IntPtr indices);

        [DllImport(OPENGL_LIB)]
        public static extern void glBindTexture(GLenum target, GLuint texture);

        [DllImport(OPENGL_LIB)]
        public static extern void glGenTextures(GLsizei n, [MarshalAs(UnmanagedType.LPArray)]GLuint[] textures);

        [DllImport(OPENGL_LIB)]
        public static extern void glTexImage2D(GLenum target, GLint level, GLint internalformat, GLsizei width, GLsizei height, GLint border, GLenum format, GLenum type, IntPtr pixels);

        [DllImport(OPENGL_LIB)]
        public static extern void glDeleteTextures(GLsizei n, [MarshalAs(UnmanagedType.LPArray)]GLuint[] textures);

        [DllImport(OPENGL_LIB)]
        public static extern void glTexParameteri(GLenum target, GLenum pname, GLint param);

        [DllImport(OPENGL_LIB)]
        public static extern void glGenerateTextureMipmap(GLuint texture);

        [DllImport(OPENGL_LIB)]
        public static extern void glBindBuffer(GLenum target, GLuint buffer);

        [DllImport(OPENGL_LIB)]
        public static extern void glDeleteBuffers(GLsizei n, GLuint[] buffers);

        [DllImport(OPENGL_LIB)]
        public static extern void glGenBuffers(GLsizei n, [MarshalAs(UnmanagedType.LPArray)]GLuint[] buffers);

        [DllImport(OPENGL_LIB)]
        public static extern void glBufferData(GLenum target, GLsizeiptr size, IntPtr data, GLenum usage);

        [DllImport(OPENGL_LIB)]
        public static extern void glAttachShader(GLuint program, GLuint shader);

        [DllImport(OPENGL_LIB)]
        public static extern void glCompileShader(GLuint shader);

        [DllImport(OPENGL_LIB)]
        public static extern void glDeleteShader(GLuint shader);

        [DllImport(OPENGL_LIB)]
        public static extern void glDeleteProgram(GLuint program);

        [DllImport(OPENGL_LIB)]
        public static extern GLuint glCreateShader(GLenum type);

        [DllImport(OPENGL_LIB)]
        public static extern GLuint glCreateProgram();

        [DllImport(OPENGL_LIB)]
        public static extern void glShaderSource(GLuint shader, GLsizei count, string[] source, GLint[] length);

        [DllImport(OPENGL_LIB)]
        public static extern void glUseProgram(GLuint program);

        [DllImport(OPENGL_LIB)]
        public static extern void glGetProgramiv(GLuint program, GLenum pname, out GLint parameter);

        [DllImport(OPENGL_LIB)]
        public static extern void glGetShaderiv(GLuint shader, GLenum pname, out GLint parameter);

        [DllImport(OPENGL_LIB)]
        public static extern void glGetShaderInfoLog(GLuint shader, GLsizei bufSize, out GLsizei length, System.Text.StringBuilder infoLog);

        [DllImport(OPENGL_LIB)]
        public static extern void glLinkProgram(GLuint program);

        [DllImport(OPENGL_LIB)]
        public static extern GLint glGetAttribLocation(GLuint program, string name);

        [DllImport(OPENGL_LIB)]
        public static extern GLint glGetUniformLocation(GLuint program, string name);

        [DllImport(OPENGL_LIB)]
        public static extern void glDisableVertexAttribArray(GLuint index);

        [DllImport(OPENGL_LIB)]
        public static extern void glEnableVertexAttribArray(GLuint index);

        [DllImport(OPENGL_LIB)]
        public static extern void glUniform1f(GLint location, GLfloat v0);

        [DllImport(OPENGL_LIB)]
        public static extern void glUniform2f(GLint location, GLfloat v0, GLfloat v1);

        [DllImport(OPENGL_LIB)]
        public static extern void glUniform3f(GLint location, GLfloat v0, GLfloat v1, GLfloat v2);

        [DllImport(OPENGL_LIB)]
        public static extern void glUniform4f(GLint location, GLfloat v0, GLfloat v1, GLfloat v2, GLfloat v3);

        [DllImport(OPENGL_LIB)]
        public static extern void glUniform1i(GLint location, GLint v0);

        [DllImport(OPENGL_LIB)]
        public static extern void glUniform2i(GLint location, GLint v0, GLint v1);

        [DllImport(OPENGL_LIB)]
        public static extern void glUniform3i(GLint location, GLint v0, GLint v1, GLint v2);

        [DllImport(OPENGL_LIB)]
        public static extern void glUniform4i(GLint location, GLint v0, GLint v1, GLint v2, GLint v3);

        [DllImport(OPENGL_LIB)]
        public static extern void glUniform1fv(GLint location, GLsizei count, [MarshalAs(UnmanagedType.LPArray)]GLfloat[] value);

        [DllImport(OPENGL_LIB)]
        public static extern void glUniform2fv(GLint location, GLsizei count, [MarshalAs(UnmanagedType.LPArray)]GLfloat[] value);

        [DllImport(OPENGL_LIB)]
        public static extern void glUniform3fv(GLint location, GLsizei count, [MarshalAs(UnmanagedType.LPArray)]GLfloat[] value);

        [DllImport(OPENGL_LIB)]
        public static extern void glUniform4fv(GLint location, GLsizei count, [MarshalAs(UnmanagedType.LPArray)]GLfloat[] value);

        [DllImport(OPENGL_LIB)]
        public static extern void glUniform1iv(GLint location, GLsizei count, [MarshalAs(UnmanagedType.LPArray)]GLint[] value);

        [DllImport(OPENGL_LIB)]
        public static extern void glVertexAttribI1i(GLuint index, GLint x);

        [DllImport(OPENGL_LIB)]
        public static extern void glVertexAttribI2i(GLuint index, GLint x, GLint y);

        [DllImport(OPENGL_LIB)]
        public static extern void glVertexAttribI3i(GLuint index, GLint x, GLint y, GLint z);

        [DllImport(OPENGL_LIB)]
        public static extern void glVertexAttribI4i(GLuint index, GLint x, GLint y, GLint z, GLint w);

        [DllImport(OPENGL_LIB)]
        public static extern void glVertexAttrib1f(GLuint index, GLfloat x);

        [DllImport(OPENGL_LIB)]
        public static extern void glVertexAttrib2f(GLuint index, GLfloat x, GLfloat y);

        [DllImport(OPENGL_LIB)]
        public static extern void glVertexAttrib3f(GLuint index, GLfloat x, GLfloat y, GLfloat z);

        [DllImport(OPENGL_LIB)]
        public static extern void glVertexAttrib4f(GLuint index, GLfloat x, GLfloat y, GLfloat z, GLfloat w);

        [DllImport(OPENGL_LIB)]
        public static extern void glVertexAttribPointer(GLuint index, GLint size, GLenum type, GLboolean normalized, GLsizei stride, IntPtr pointer);

        [DllImport(OPENGL_LIB)]
        public static extern void glPatchParameteri(GLenum pname, GLint value);

        [DllImport(OPENGL_LIB)]
        public static extern void glUniformMatrix3fv(GLint location, GLsizei count, GLboolean transpose, [MarshalAs(UnmanagedType.LPArray)]GLfloat[] value);

        [DllImport(OPENGL_LIB)]
        public static extern void glUniformMatrix4fv(GLint location, GLsizei count, GLboolean transpose, [MarshalAs(UnmanagedType.LPArray)]GLfloat[] value);

        [DllImport(OPENGL_LIB)]
        public static extern void glBindVertexArray(GLuint array);

        [DllImport(OPENGL_LIB)]
        public static extern void glDeleteVertexArrays(GLsizei n, GLuint[] arrays);

        [DllImport(OPENGL_LIB)]
        public static extern void glGenVertexArrays(GLsizei n, [MarshalAs(UnmanagedType.LPArray)]GLuint[] arrays);

        [DllImport(OPENGL_LIB)]
        public static extern void glActiveTexture(GLenum id);

        [DllImport(OPENGL_LIB)]
        public static extern void glGenerateMipmap(GLenum id);

        [DllImport(OPENGL_LIB)]
        public static extern void glTexStorage2D(GLenum target,
                                         GLsizei levels,
                                         GLenum internalformat,
                                         GLsizei width,
                                         GLsizei height);

    }
}

