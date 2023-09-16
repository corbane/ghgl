using System;
using System.ComponentModel;

using EF = Eto.Forms;
using RH = Rhino;
using RUI = Rhino.UI;

using RhGL.Rhino;
using RhGL.IO;


namespace RhGL.UI;


class ProgramEditorForm : EF.Form
{
    Program _program;


    public ProgramEditorForm (Program program, string componentName)
    {
        _dlgOpenCount++;

        _program = program;
        DataContext = program;

        Title = $"GLSL Shader - {componentName}";
        Resizable = true;
        Size = new Eto.Drawing.Size(600, 600);


        _shaderControls = new EditorPage[(int)ShaderType.Fragment + 1];
        for (int i = 0; i < _shaderControls.Length; i++)
            _shaderControls[i] = new EditorPage ();

        var checkCommand = new EF.CheckCommand[_shaderControls.Length];
        for (int i = 0; i < _shaderControls.Length; i++)
            checkCommand[i] = new ShaderToogleCommand (this, _shaderControls[i], (ShaderType)i);


        var uniformBuiltinMenu = new EF.ButtonMenuItem { Text = "Insert Built-In Uniform" };
        foreach (var bi in BuiltIn.GetUniformBuiltIns ())
            uniformBuiltinMenu.Items.Add(new InsertBuiltInCommand (this, bi, true));

        var attributeBuiltinMenu = new EF.ButtonMenuItem { Text = "Insert Built-In Attribute" };
        foreach (var bi in BuiltIn.GetAttributeBuiltIns ())
            attributeBuiltinMenu.Items.Add(new InsertBuiltInCommand (this, bi, false));

        var functionBuiltinMenu = new EF.ButtonMenuItem { Text = "Insert Function (glslify)" };
        var glslBuiltinMenu     = new EF.ButtonMenuItem { Text = "StackGL Items" };
        foreach (var package in GlslifyClient.AvailablePackages) {
            var ownerItem = package.IsStackGl ? glslBuiltinMenu : functionBuiltinMenu;
            ownerItem.Items.Add (new InsertGlslifyFunctionCommand (this, package));
        }
        functionBuiltinMenu.Items.Add (glslBuiltinMenu);


        Menu = new EF.MenuBar
        {
            Items = {
                new EF.ButtonMenuItem {
                    Text = "&File",
                    Items = {
                        new SaveCommand (this),
                        new OpenXMLAssetCommand (this),
                        new SaveXMLAssetCommand (this),
                        new OpenMdCommand (this),
                        new SaveMdCommand (this),
                    }
                },
                new EF.ButtonMenuItem {
                    Text = "&Edit",
                    Items = {
                        uniformBuiltinMenu,
                        attributeBuiltinMenu,
                        functionBuiltinMenu,
                        new EF.SeparatorMenuItem (),
                        new GlslifyCommand (this)
                    }
                },
                new EF.ButtonMenuItem {
                    Text = "&View",
                    Items = {
                        checkCommand[(int)ShaderType.Vertex],
                        checkCommand[(int)ShaderType.TessellationControl],
                        checkCommand[(int)ShaderType.TessellationEval],
                        checkCommand[(int)ShaderType.Geometry],
                        checkCommand[(int)ShaderType.Fragment],
                        checkCommand[(int)ShaderType.TransformFeedback],
                    }
                }
            }
        };


        _tabarea = new EF.TabControl();
        static bool HasSourceCode (string code) => !string.IsNullOrWhiteSpace (code);
        ShowTab (ShaderType.Vertex);
        if (HasSourceCode (program.TessellationControlCode)) ShowTab(ShaderType.TessellationControl);
        if (HasSourceCode (program.TessellationEvalualtionCode)) ShowTab(ShaderType.TessellationEval);
        if (HasSourceCode (program.GeometryShaderCode)) ShowTab (ShaderType.Geometry);
        ShowTab (ShaderType.Fragment);
        if (HasSourceCode (program.TransformFeedbackShaderCode)) ShowTab (ShaderType.TransformFeedback);
        _tabarea.SelectedIndex = 0;


        Content = new EF.StackLayout
        {
            Orientation = EF.Orientation.Vertical,
            HorizontalContentAlignment = EF.HorizontalAlignment.Stretch,
            Padding = new Eto.Drawing.Padding(5),
            Spacing = 5,
            Items = {
                new EF.StackLayoutItem { Control = _tabarea, Expand = true },
                (_errorList = new EF.ListBox { Height = 40 }),
                new EF.StackLayout {
                    Padding = 5,
                    Orientation = EF.Orientation.Horizontal,
                    Items = {
                        null,
                        new EF.Button() { Text = "OK", Command = new SimpleCommand ("OK", () => Close ()) },
                        new EF.Button() { Text = "Cancel", Command = new SimpleCommand ("Cancel", () => _Cancel ()) }
                    }
                }
            },
        };

        Conduit.AnimationTimerEnabled = true;
        _OnShadersCompiled (null, EventArgs.Empty);

    }


    static int _dlgOpenCount;
    public static bool EditorsOpen { get { return _dlgOpenCount > 0; } }

    public bool Canceled { get; set; }

    void _Cancel ()
    {
        Canceled = true;
        Close();
    }

    protected override void OnClosed (EventArgs e)
    {
        _dlgOpenCount--;
        base.OnClosed(e);
    }


    #region Tabs

    class EditorPage : INotifyPropertyChanged
    {
        bool _visible;
        public bool Visible
        {
            get => _visible;
            set {
                if (_visible != value)
                    _visible = value;
            }
        }
        
        public UI.ShaderCodeEditorControl? Control { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    readonly EditorPage[] _shaderControls;
    readonly EF.TabControl _tabarea;

    EditorPage ShowTab (ShaderType type)
    {
        var sc = _shaderControls[(int)type];
        if (sc!.Control == null) {
            var program = DataContext as Program;
            sc.Control = new ShaderCodeEditorControl (type, program!);
            sc.Control.ShaderCompiled += _OnShadersCompiled;
        }

        int index = -1;
        for (int i = 0; i < _tabarea.Pages.Count; i++)
        {
            var ctrl = _tabarea.Pages[i].Content as ShaderCodeEditorControl;
            if (ctrl!.ShaderType == type)
                return sc;

            if((int)ctrl.ShaderType > (int)type)
            {
                _tabarea.Pages.Insert(i, new EF.TabPage() { Text = sc.Control.Title, Content = sc.Control});
                index = i;
                break;
            }
        }
        if (-1 == index)
        {
            _tabarea.Pages.Add(new EF.TabPage() { Text = sc.Control.Title, Content = sc.Control });
            index = _tabarea.Pages.Count - 1;
        }
        _tabarea.SelectedIndex = index;
        sc.Visible = true;
        return sc;
    }

    void HideTab (ShaderType type)
    {
        var sc = _shaderControls[(int)type];
        if (sc.Control == null)
        {
            sc.Visible = false;
            return;
        }

        for (int i = 0; i < _tabarea.Pages.Count; i++)
        {
            var ctrl = _tabarea.Pages[i].Content as UI.ShaderCodeEditorControl;
            if (ctrl!.ShaderType == type)
            {
                _tabarea.Pages.Remove (_tabarea.Pages[i]);
                sc.Control = null;
                sc.Visible = false;
            }
        }
    }

    void _ModelPropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof (Program.DrawMode):
            case nameof (Program.PatchVertices):
            case nameof (Program.glLineWidth):
            case nameof (Program.glPointSize):
            case nameof (Program.DepthTestingEnabled):
            case nameof (Program.DepthWritingEnabled):
            
                break;
        }
    }

    #endregion


    #region Compilation

    readonly EF.ListBox _errorList;

    void _OnShadersCompiled (object? sender, EventArgs e)
    {
        if (DataContext is Program model)
        {
            var errors = model.AllCompileErrors();
            _errorList.Items.Clear();
            if (errors.Length == 0)
            {
                _errorList.Items.Add("Compile output (no errors)");
                _errorList.TextColor = Eto.Drawing.Colors.Gray;
            }
            else
            {
                _errorList.TextColor = Eto.Drawing.Colors.Red;
                foreach (var err in errors)
                    _errorList.Items.Add(err.ToString());
            }
        }
    }

    #endregion


    #region Commands


    class SimpleCommand : EF.Command
    {
        public SimpleCommand(string text, Action action)
        {
            MenuText = text;
            Executed += (s, e) => action();
        }
    }

    class ShaderToogleCommand : EF.CheckCommand
    {
        public ShaderToogleCommand (ProgramEditorForm editor, EditorPage page, ShaderType st)
        {
            DataContext = page;
            MenuText    = st.ToString() + " Shader";
            _editor     = editor;
            _shaderType = st;
            EF.BindableExtensions.BindDataContext <bool> (this, "Checked", "Visible");
            CheckedChanged += _Toogle;
        }

        ProgramEditorForm _editor;
        ShaderType _shaderType;

        void _Toogle (object s, EventArgs e)
        {
            if (s is not EF.CheckCommand command)
                return;
            if (command.Checked)
                _editor.ShowTab(_shaderType);
            else
                _editor.HideTab(_shaderType);
        }
    }

    class SaveCommand : EF.Command
    {
        public SaveCommand (ProgramEditorForm editor) 
        {
            MenuText = "&Save";
            Executed += _OnRun;
            _editor   = editor;
        }

        ProgramEditorForm _editor;
        
        void _OnRun (object sender, EventArgs e)
        {
            var dlg = new EF.SaveFileDialog ();
            dlg.Filters.Add(new EF.FileFilter ("Text file", new string[] { "txt" }));
            if( dlg.ShowDialog (_editor) == EF.DialogResult.Ok )
            {
                _editor._program.Save(dlg.FileName);
            }
        }
    }

    class OpenMdCommand : EF.Command
    {
        public OpenMdCommand (ProgramEditorForm editor) 
        {
            MenuText = "Open Markdown";
            Executed += _OnRun;
            _editor   = editor;
        }

        ProgramEditorForm _editor;
        
        void _OnRun (object sender, EventArgs e)
        {
            var dlg = new EF.OpenFileDialog ();
            dlg.Filters.Add(new EF.FileFilter ("Markdown file", new string[] { "md" }));
            if( dlg.ShowDialog (_editor) == EF.DialogResult.Ok )
            {
                MdExporter.Read (_editor._program, dlg.FileName);
                foreach (var shader in _editor._program.Shaders)
                {
                    if (shader == null || string.IsNullOrWhiteSpace (shader.Code))
                        continue;

                    var tab = _editor.ShowTab (shader.ShaderType);
                    tab.Control!.Text = shader.Code;
                }
            }
        }
    }

    class SaveMdCommand : EF.Command
    {
        public SaveMdCommand (ProgramEditorForm editor) 
        {
            MenuText = "Save as Markdown";
            Executed += _OnRun;
            _editor   = editor;
        }

        ProgramEditorForm _editor;
        
        void _OnRun (object sender, EventArgs e)
        {
            var dlg = new EF.SaveFileDialog ();
            dlg.Filters.Add(new EF.FileFilter ("Markdown file", new string[] { "md" }));
            if( dlg.ShowDialog (_editor) == EF.DialogResult.Ok )
            {
                MdExporter.Write (_editor._program, dlg.FileName);
            }
        }
    }

    class SaveXMLAssetCommand : EF.Command
    {
        public SaveXMLAssetCommand (ProgramEditorForm editor) 
        {
            MenuText = "Save program files";
            Executed += _OnRun;
            _editor   = editor;
        }

        ProgramEditorForm _editor;
        
        void _OnRun (object sender, EventArgs e)
        {
            var dlg = new EF.SaveFileDialog ();
            dlg.Filters.Add(new EF.FileFilter ("XML file", new string[] { "xml" }));
            if( dlg.ShowDialog (_editor) == EF.DialogResult.Ok )
            {
                AssetIO.ExportAsXML (_editor._program, dlg.FileName, saveShaders: true);
            }
        }
    }

    class OpenXMLAssetCommand : EF.Command
    {
        public OpenXMLAssetCommand (ProgramEditorForm editor) 
        {
            MenuText = "Open program files";
            Executed += _OnRun;
            _editor   = editor;
        }

        ProgramEditorForm _editor;
        
        void _OnRun (object sender, EventArgs e)
        {
            var dlg = new EF.OpenFileDialog ();
            dlg.Filters.Add(new EF.FileFilter ("XML file", new string[] { "xml" }));
            if( dlg.ShowDialog (_editor) == EF.DialogResult.Ok )
            {
                _editor._program.FilePath = dlg.FileName;
                // AssetIO.ImportAsXML (_editor._program, dlg.FileName, importShaders: true);
                foreach (var shader in _editor._program.Shaders)
                {
                    if (shader == null || string.IsNullOrWhiteSpace (shader.Code))
                        continue;

                    var tab = _editor.ShowTab (shader.ShaderType);
                    tab.Control!.Text = shader.Code;
                }
            }
        }
    }

    ShaderCodeEditorControl? ActiveEditorControl ()
    {
        return _tabarea.SelectedPage.Content as UI.ShaderCodeEditorControl;
    }

    class GlslifyCommand : EF.Command
    {
        public GlslifyCommand (ProgramEditorForm editor) 
        {
            MenuText = "glslify code";
            Executed += _OnRun;
            _editor   = editor;
        }

        ProgramEditorForm _editor;
        
        void _OnRun (object sender, EventArgs e)
        {
            var shaderCtrl = _editor.ActiveEditorControl ();
            if (shaderCtrl == null || DataContext is not Program model)
                return;

            string code = model.GetCode (shaderCtrl.ShaderType);
            if (code.Contains ("#pragma glslify:"))
            {
                string processedCode = GlslifyClient.GlslifyCode (code);
                shaderCtrl.Text = processedCode;
                //model.SetCode(shaderCtrl.ShaderType, processedCode);
            }
        }
    }

    class InsertBuiltInCommand : EF.Command
    {
        public InsertBuiltInCommand (ProgramEditorForm editor, BuiltIn builtIn, bool asUniform)
        {
            MenuText    = builtIn.Name;
            ToolTip     = $"({builtIn.DataType}) {builtIn.Description}";
            Executed   += _OnRun;
            _editor     = editor;
            _builtIn    = builtIn;
            _asUniform  = asUniform;
        }
        
        ProgramEditorForm _editor;
        BuiltIn _builtIn;
        bool _asUniform;
        
        void _OnRun (object sender, EventArgs e)
        {
            var shaderCtrl = _editor.ActiveEditorControl();
            if (shaderCtrl == null)
                return;

            if (_asUniform)
            {
                string text = $"uniform {_builtIn.DataType} {_builtIn.Name};";
                shaderCtrl.InsertText(shaderCtrl.CurrentPosition, text);
            }
            else
            {
                //layout(location = 1) in vec4 vcolor;
                string code = shaderCtrl.Text;
                code = code.Replace(" ", "");
                int count = 0;
                int index = code.IndexOf("(location=", 0);
                while(index >=0) {
                    count++;
                    index = code.IndexOf ("(location=", index+10);
                }
                string text = $"layout(location = {count}) in {_builtIn.DataType} {_builtIn.Name};";
                shaderCtrl.InsertText(shaderCtrl.CurrentPosition, text);
            }
        }
    }

    class InsertGlslifyFunctionCommand : EF.Command
    {
        public InsertGlslifyFunctionCommand (ProgramEditorForm editor, GlslifyPackage package) 
        {
            MenuText  = package.Name;
            ToolTip   = $"{package.Description}\n{package.HomePage}";
            Executed += _OnRun;
            _editor   = editor;
            _package  = package;
        }

        ProgramEditorForm _editor;
        GlslifyPackage _package;
        
        void _OnRun (object sender, EventArgs e)
        {
        var shaderCtrl = _editor.ActiveEditorControl();
        if (shaderCtrl != null)
        {

            string text = _package.PragmaLine (null);
            shaderCtrl.InsertText(shaderCtrl.CurrentPosition, text);
        }
        }
    }


    #endregion
}
