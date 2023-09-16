
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Xml.Linq;
using System.IO;
using Path = System.IO.Path;
using File = System.IO.File;
using Dir = System.IO.Directory;

//using GK = Grasshopper.Kernel;
using FileWatcherEvents = Grasshopper.Kernel.GH_FileWatcherEvents;
using FileWatcher = Grasshopper.Kernel.GH_FileWatcher;

using RhGL.Rhino;
using Rhino.Render.ChangeQueue;


namespace RhGL.IO;


public static class AssetIO
{
    #region Fie path


    public static readonly string[] CS_EXT = { ".comp.glsl", ".comp" };
    public static readonly string[] VS_EXT = { ".vert.glsl", ".vs.glsl", ".vert", ".vs" };
    public static readonly string[] TC_EXT = { ".tesc.glsl", ".tesc" };
    public static readonly string[] TE_EXT = { ".tese.glsl", ".tese" };
    public static readonly string[] GS_EXT = { ".geom.glsl", ".gs.glsl", ".geom", ".gs" };
    public static readonly string[] TF_EXT = { ".feed.glsl", ".feed" };
    public static readonly string[] FS_EXT = { ".frag.glsl", ".fs.glsl", ".frag", ".fs" };

    public static bool IsValidJsonAsset (string? path)
    {
        if (path == null) return false;
        var ext = Path.GetExtension (path);
        if (ext == null) return false;

        return ext == ".xml"
            && File.Exists (path);
    }

    public static bool IsShaderFile (string path)
    {
        var ext = Path.GetExtension (path);
        if (Array.IndexOf (VS_EXT, ext) > -1) return File.Exists (path);
        if (Array.IndexOf (TC_EXT, ext) > -1) return File.Exists (path);
        if (Array.IndexOf (TE_EXT, ext) > -1) return File.Exists (path);
        if (Array.IndexOf (GS_EXT, ext) > -1) return File.Exists (path);
        if (Array.IndexOf (TF_EXT, ext) > -1) return File.Exists (path);
        if (Array.IndexOf (FS_EXT, ext) > -1) return File.Exists (path);
        return false;
    }

    public static bool TryGetShaderTypeFromPath (string path, out ShaderType type)
    {
        type = 0;
        var ext = Path.GetExtension (path);
        if (Array.IndexOf (VS_EXT, ext) > -1) { type = ShaderType.Vertex; return true; }
        if (Array.IndexOf (TC_EXT, ext) > -1) { type = ShaderType.TessellationControl; return true; }
        if (Array.IndexOf (TE_EXT, ext) > -1) { type = ShaderType.TessellationEval; return true; }
        if (Array.IndexOf (GS_EXT, ext) > -1) { type = ShaderType.Geometry; return true; }
        if (Array.IndexOf (TF_EXT, ext) > -1) { type = ShaderType.TransformFeedback; return true; }
        if (Array.IndexOf (FS_EXT, ext) > -1) { type = ShaderType.Fragment; return true; }
        return false;
    }

    public static bool FindShaderPath (ShaderType type, string filepathBase, out string? filepath)
    {
        filepath = null;
        var filename = PathLib.GetPathWithoutExtension (filepathBase);
        var extensions = type switch
        {
            ShaderType.Vertex               => VS_EXT,
            ShaderType.TessellationControl  => TC_EXT,
            ShaderType.TessellationEval     => TE_EXT,
            ShaderType.Geometry             => GS_EXT,
            ShaderType.TransformFeedback    => TF_EXT,
            ShaderType.Fragment             => FS_EXT,
            _ => Array.Empty <string> ()
        };

        foreach (var ext in extensions)
        {
            if (File.Exists (filename + ext)) {
                filepath = filename + ext;
                return true;
            }
        }

        return false;
    }


    #endregion


    #region Import


    public static void ExportAsXML (Program program, string jsonpath, bool saveShaders)
    {
        if (jsonpath.EndsWith (".xml") == false)
            jsonpath = jsonpath + ".xml";

        var doc = new XDocument (
            new XElement ("rhgl")
        );
        doc.Root.Add (
            new XElement (nameof(program.DrawMode), program.DrawMode),
            new XElement (nameof(program.PatchVertices), program.PatchVertices),
            new XElement (nameof(program.glLineWidth), program.glLineWidth),
            new XElement (nameof(program.glPointSize), program.glPointSize),
            new XElement (nameof(program.DepthTestingEnabled), program.DepthTestingEnabled),
            new XElement (nameof(program.DepthWritingEnabled), program.DepthWritingEnabled)
        );

        doc.Save (jsonpath);

        if (saveShaders == false) return;

        var basepath = Path.Combine (
            Path.GetDirectoryName (jsonpath),
            Path.GetFileNameWithoutExtension (jsonpath)
        );
        void saveCode (string? s, string p) { if (s != null && string.IsNullOrWhiteSpace (s) == false) File.WriteAllText (p, s); }
        saveCode (program.VertexShaderCode            , basepath + ".vert");
        saveCode (program.TessellationControlCode     , basepath + ".tesc");
        saveCode (program.TessellationEvalualtionCode , basepath + ".tese");
        saveCode (program.GeometryShaderCode          , basepath + ".geom");
        saveCode (program.TransformFeedbackShaderCode , basepath + ".feed");
        saveCode (program.FragmentShaderCode          , basepath + ".frag");
    }

    public static void ImportAsXML (Program program, string jsonpath, bool importShaders)
    {
        if (File.Exists (jsonpath) == false)
            return;

        XDocument doc;
        try {
            doc = XDocument.Parse (File.ReadAllText (jsonpath));
        } catch {
            return;
        }

        void setDrawMode            (XElement e) { if (e != null && uint.TryParse (e.Value, out var v))    program.DrawMode            = v; }
        void setPatchVertices       (XElement e) { if (e != null && ushort.TryParse (e.Value, out var v))  program.PatchVertices       = v; }
        void setglLineWidth         (XElement e) { if (e != null && int.TryParse (e.Value, out var v))     program.glLineWidth         = v; }
        void setglPointSize         (XElement e) { if (e != null && double.TryParse (e.Value, out var v))  program.glPointSize         = v; }
        void setDepthTestingEnabled (XElement e) { if (e != null && bool.TryParse (e.Value, out var v))    program.DepthTestingEnabled = v; }
        void setDepthWritingEnabled (XElement e) { if (e != null && bool.TryParse (e.Value, out var v))    program.DepthWritingEnabled = v; }
        
        var root = doc.Root;
        setDrawMode            (root.Element (nameof(program.DrawMode)));
        setPatchVertices       (root.Element (nameof(program.PatchVertices)));
        setglLineWidth         (root.Element (nameof(program.glLineWidth)));
        setglPointSize         (root.Element (nameof(program.glPointSize)));
        setDepthTestingEnabled (root.Element (nameof(program.DepthTestingEnabled)));
        setDepthWritingEnabled (root.Element (nameof(program.DepthWritingEnabled)));
    
        if (importShaders)
            ImportShaders (program, jsonpath);
    }

    public static void ImportJsonAsset (Program program, string jsonpath, bool importShaders)
    {
        if (Path.GetExtension (jsonpath) != ".json")
            return;

        JsonDocument doc;
        try {
            doc = JsonDocument.Parse (File.ReadAllBytes (jsonpath));
        } catch {
            return;
        }

        if (doc.RootElement.ValueKind != JsonValueKind.Object)
            return;

        void setDrawMode            (JsonElement e) { if (e.TryGetUInt32 (out var v)) program.DrawMode      = v; }
        void setPatchVertices       (JsonElement e) { if (e.TryGetUInt16 (out var v)) program.PatchVertices = v; }
        void setglLineWidth         (JsonElement e) { if (e.TryGetDouble (out var v)) program.glLineWidth   = v; }
        void setglPointSize         (JsonElement e) { if (e.TryGetDouble (out var v)) program.glPointSize   = v; }
        void setDepthTestingEnabled (JsonElement e) { program.DepthTestingEnabled = e.ValueKind == JsonValueKind.True; }
        void setDepthWritingEnabled (JsonElement e) { program.DepthWritingEnabled = e.ValueKind == JsonValueKind.True; }
    
        foreach (var e in doc.RootElement.EnumerateObject ())
        {
            switch (e.Name)
            {
            case nameof(program.DrawMode)            : setDrawMode (e.Value)            ; break;
            case nameof(program.PatchVertices)       : setPatchVertices (e.Value)       ; break;
            case nameof(program.glLineWidth)         : setglLineWidth (e.Value)         ; break;
            case nameof(program.glPointSize)         : setglPointSize (e.Value)         ; break;
            case nameof(program.DepthTestingEnabled) : setDepthTestingEnabled (e.Value) ; break;
            case nameof(program.DepthWritingEnabled) : setDepthWritingEnabled (e.Value) ; break;
            }
        }
    
        if (importShaders)
            ImportShaders (program, jsonpath);
    }

    public static void ImportShaders (Program program, string referencePath)
    {
        if (false == File.Exists (referencePath)) {
            return;
        }

        string? path;

        program.VertexShaderCode
            = FindShaderPath (ShaderType.Vertex, referencePath, out path)
            ? File.ReadAllText (path) : "";
        
        program.TessellationControlCode
            = FindShaderPath (ShaderType.TessellationControl, referencePath!, out path)
            ? File.ReadAllText (path) : "";
        
        program.TessellationEvalualtionCode
            = FindShaderPath (ShaderType.TessellationEval, referencePath!, out path)
            ? File.ReadAllText (path) : "";

        program.GeometryShaderCode
            = FindShaderPath (ShaderType.Geometry, referencePath!, out path)
            ? File.ReadAllText (path) : "";

        program.TransformFeedbackShaderCode
            = FindShaderPath (ShaderType.TransformFeedback, referencePath!, out path)
            ? File.ReadAllText (path) : "";

        program.FragmentShaderCode
            = FindShaderPath (ShaderType.Fragment, referencePath!, out path)
            ? File.ReadAllText (path) : "";
    }


    #endregion


    #region File watcher

    /*/
        AttachWatcher ("~/directory/glsl.vert", (string filepath) => { });
        AttachWatcher ("~/directory/glsl.json", (string filepath) => { });

        handle any "~/directory/glsl.*"
    /*/

    public delegate void FileWatcherHandle (string filepath);
    record DirectoryEntry (
        FileWatcher Watcher,
        Dictionary <string, FileWatcherHandle> HandleMap
    );
    static Dictionary <string, DirectoryEntry> _directoryMap = new ();

    public static void DetachWatcher (string referencePath)
    {
        string dir
            = File.Exists (referencePath)
            ? Path.GetDirectoryName (referencePath)
            : referencePath;
            
        if (_directoryMap.ContainsKey (dir) == false)
            return;
        var entry = _directoryMap[dir];

        var basepath = Path.GetFileNameWithoutExtension (referencePath);
        if (basepath == null
        ||  entry.HandleMap.ContainsKey (basepath) == false
        ) return;

        entry.HandleMap.Remove (basepath);
        if (entry.HandleMap.Count == 0) {
            entry.Watcher.Dispose ();
            _directoryMap.Remove (dir);
        }
    }

    public static void AttachWatcher (string referencePath, FileWatcherHandle handle)
    {
        string dir
            = File.Exists (referencePath)
            ? Path.GetDirectoryName (referencePath)
            : referencePath;
        if (Dir.Exists (dir) == false)
            return;

        DirectoryEntry entry;
        if (_directoryMap.ContainsKey (dir))
            entry = _directoryMap[dir];
        else { 
            _directoryMap[dir] = entry = new DirectoryEntry (
                FileWatcher.CreateDirectoryWatcher (dir, "*", FileWatcherEvents.All, _OnFileChanged),
                new ()
            );
            entry.Watcher.Active = true;
        }

        var basepath = PathLib.GetPathWithoutExtension (referencePath);
        if (basepath == null
        ||  entry.HandleMap.ContainsKey (basepath)
        ) return;
        
        entry.HandleMap.Add (basepath, handle);
    }

    static void _OnFileChanged (string filepath)
    {
        var dir = Path.GetDirectoryName (filepath);

        if (_directoryMap.ContainsKey (dir) == false)
            return;
        var entry = _directoryMap[dir];
        
        var basepath = PathLib.GetPathWithoutExtension (filepath);
        if (basepath == null
        ||  entry.HandleMap.ContainsKey (basepath) == false
        ) return;
        
        entry.HandleMap[basepath].Invoke (filepath);
    }


    #endregion
}
