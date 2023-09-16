using System;


using RhGL.Rhino;
using Grasshopper.Kernel.Special;


namespace RhGL.UI;


class ShaderCodeEditorControl : Eto.CodeEditor.CodeEditor
{
    public ShaderCodeEditorControl(ShaderType type, Program model) 
        : base (Eto.CodeEditor.ProgrammingLanguage.GLSL)
    {
        _shaderType  = type;
        _prog       = model;
        Text         = model.GetCode (type);
        CharAdded   += ShaderControlCharAdded;
        TextChanged += ShaderControlTextChanged;
        MarkErrors();
        _compileTimer.Elapsed  += CompileTimerTick;
        _compileTimer.Interval  = 1; //every second
        IsFoldingMarginVisible  = true;
    }
 

    #region Properties


    readonly Program _prog;

    readonly ShaderType _shaderType;
    public ShaderType ShaderType { get => _shaderType; }

    public string Title
    {
        get
        {
            switch (_shaderType)
            {
                case ShaderType.Vertex                  : return "Vertex";
                case ShaderType.Geometry                : return "Geometry";
                case ShaderType.TessellationControl     : return "Tessellation Ctrl";
                case ShaderType.TessellationEval        : return "Tessellation Eval";
                case ShaderType.Fragment                : return "Fragment";
                case ShaderType.TransformFeedback : return "Transform Feedback Vertex";
            }
            return "";
        }
    }


    #endregion


    #region Error

    void MarkErrors ()
    {
        ClearAllErrorIndicators ();
        foreach (var error in _prog.AllCompileErrors())
        {
            if (error.Shader == null)
                continue;
            if (error.Shader.ShaderType == _shaderType)
                AddErrorIndicator(error.LineNumber, 0);
        }

    }

    #endregion


    #region Compilation

    // void _OnCompile ()
    // {
    //     // Grasshopper.GLBuiltInShader.ActivateGL ();
    //     RH_Renderer.ActivateGlContext ();
    //     _prog.CompileProgram ();
    //     RH_Renderer.AnimationTimerEnabled = true;
    //     MarkErrors ();
    //     ShaderCompiled?.Invoke (this, new EventArgs());
    // }
    // Debouncer _timer;

    public event EventHandler? ShaderCompiled;
    Eto.Forms.UITimer _compileTimer = new Eto.Forms.UITimer();
    private void ShaderControlTextChanged (object sender, EventArgs e)
    {
        _prog.SetCode (_shaderType, Text);
        if(_prog.GetShader (_shaderType).IsModified)
        {
            Conduit.AnimationTimerEnabled = false;
            _compileTimer.Stop();
            _compileTimer.Start();
        }
    }
    private void CompileTimerTick (object sender, EventArgs e)
    {
        _compileTimer.Stop();
        // Grasshopper.GLBuiltInShader.ActivateGL ();
        Conduit.ActivateGlContext ();
        _prog.CompileProgram ();
        Conduit.AnimationTimerEnabled = true;
        MarkErrors ();
        ShaderCompiled?.Invoke (this, new EventArgs());
    }

    #endregion


    #region Auto Complete

    static string[]? _keywords;
    static string[]? _builtins;
    private void ShaderControlCharAdded (object sender, Eto.CodeEditor.CharAddedEventArgs e)
    {
        if (AutoCompleteActive)
            return;

        int currentPos = CurrentPosition;
        int wordStartPos = WordStartPosition(currentPos, true);
        var lenEntered = currentPos - wordStartPos;
        if (lenEntered <= 0)
            return;

        if (lenEntered > 0)
        {
            string word = GetTextRange(wordStartPos, lenEntered);
            string items = "";
            if (_keywords == null)
            {
                string kw0 = "attribute layout uniform float int bool vec2 vec3 vec4 " +
                    "mat4 in out sampler2D if else return void flat discard";
                _keywords = kw0.Split(new char[] { ' ' });
                Array.Sort(_keywords);
            }
            if (_builtins == null)
            {
                var bis = BuiltIn.GetUniformBuiltIns();
                bis.AddRange(BuiltIn.GetAttributeBuiltIns());
                _builtins = new string[bis.Count];
                for (int i = 0; i < bis.Count; i++)
                    _builtins[i] = bis[i].Name;
                Array.Sort(_builtins);
            }
            string[] list = _keywords;
            if (word.StartsWith("_"))
                list = _builtins;
            foreach (var kw in list)
            {
                int startIndex = 0;
                bool add = true;
                foreach (var c in word)
                {
                    startIndex = kw.IndexOf(c, startIndex);
                    if (startIndex < 0)
                    {
                        add = false;
                        break;
                    }
                }
                if (add)
                    items += kw + " ";
            }
            items = items.Trim();
            if (items.Length > 0)
                AutoCompleteShow(lenEntered, items);
        }
    }

    #endregion

}
