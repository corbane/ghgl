using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace RhGL;


class GlslifyPackage
{
    internal GlslifyPackage(Dictionary<string,string> d)
    {
        Name        = d["name"];
        Author      = d["author"];
        Description = d["description"];
        HomePage    = d["homepage"];
    }

    public string Name { get; }
    public string Author { get; }
    public string Description { get; }
    public string HomePage { get; }

    public bool IsStackGl
        => Name.Equals ("matcap", StringComparison.OrdinalIgnoreCase)
        || Name.StartsWith ("glsl-", StringComparison.OrdinalIgnoreCase);

    public string PragmaLine (string? functionName)
    {
        if(string.IsNullOrWhiteSpace(functionName))
        {
            functionName = Name;
            if (functionName.StartsWith("glsl-"))
                functionName = functionName.Substring("glsl-".Length);
            functionName = functionName.Replace('-', '_');
        }
        string text = $"#pragma glslify: {functionName} = require('{Name}')";
        return text;
    }
}


static class GlslifyClient
{
    static System.Net.Http.HttpClient? _cient;

    static Task <List <GlslifyPackage>>? _retriever;
    
    static string PackageServerUrl => "https://ghgl-glslify.herokuapp.com"; // "http://localhost:8080";


    static bool _is_initialized;
    public static bool IsInitialized => _is_initialized;

    public static void Initialize ()
    {
        if (_is_initialized)
            return;
        _is_initialized = true;

        if (null == _cient)
            _cient = new System.Net.Http.HttpClient();

        if (_retriever != null )
            return;
        
        _retriever = Task.Run (async () =>
        {
            List <GlslifyPackage> packages = new ();
            try
            {
                var jsonSerializer = new System.Web.Script.Serialization.JavaScriptSerializer ();
                var msg = await _cient.GetAsync (PackageServerUrl + "/packages");
                {
                    string json = await msg.Content.ReadAsStringAsync ();
                    var    pkgs = jsonSerializer.Deserialize <Dictionary <string, string>[]> (json);
                    foreach (var dict in pkgs) packages.Add (new GlslifyPackage (dict));
                }
            }
            catch (Exception) {
                // throw away
            }
            return packages;
        });
    }


    static List<GlslifyPackage>? _availablePackages;

    public static GlslifyPackage[] AvailablePackages
    {
        get
        {
            if( _availablePackages == null )
            {
                _availablePackages = _retriever.Result;
                _retriever = null;
                if (_availablePackages != null) {
                    _availablePackages.Sort((a, b) =>
                    {
                        return string.CompareOrdinal(a.Name.ToLowerInvariant(), b.Name.ToLowerInvariant());
                    });
                }
            }
            return _availablePackages.ToArray();
        }
    }


    public static string GlslifyCode (string code)
    {
        try
        {
            if (null == _cient)
                _cient = new System.Net.Http.HttpClient();

            var    values        = new Dictionary <string, string> { { "code", code } };
            string json          = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(values);
            var    content       = new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json");
            var    response      = _cient.PostAsync(PackageServerUrl + "/process", content);
            string processedCode = response.Result.Content.ReadAsStringAsync().Result;
            return processedCode;
        }
        catch (Exception)
        {
        }
        return "";
    }


}

