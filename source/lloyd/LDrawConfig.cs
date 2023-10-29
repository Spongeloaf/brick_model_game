using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Godot;


namespace LDraw
{
  public partial class LDrawConfig : Resource
  {
    [Export] private string _BasePartsPath = "C:\\Program Files\\BricklinkStudio 2.0\\ldraw\\";
    [Export] private string _ModelsPath= "res://models/";
    [Export] private string _MaterialsPath = "res://assets/generated/materials/";
    [Export] private string _MeshesPath = "res://assets/generated/meshes/";
    [Export] private string _ColorConfigPath;
    [Export] private float _Scale;
    [Export] private BaseMaterial3D _DefaultOpaqueMaterial;
    [Export] private BaseMaterial3D _DefaultTransparentMaterial;

    // This entire library was stolen directly from the Unity version of this project, and ported to Godot as
    // directly as possible.
    // If any of this doesn't make sense, then go ahead and change it.
    private Dictionary<string, string> _Parts;
    private Dictionary<string, string> _Models;

    private Dictionary<int, Material> _MainColors;
    private Dictionary<string, Material> _CustomColors;
    private Dictionary<string, string> _ModelFileNames;

    private const string ConfigPath = "Assets/LDraw-Importer/Editor/Config.asset";
    public const int DefaultMaterialCode = 16;

    LDrawConfig()
    {
      _ColorConfigPath = _BasePartsPath + "LDConfig.ldr";
    }

    public System.Numerics.Matrix4x4 ScaleMatrix
    {
      get { return System.Numerics.Matrix4x4.CreateScale(new System.Numerics.Vector3(_Scale, _Scale, _Scale)); }
    }


    public Material GetColoredMaterial(int code)
    {
      return _MainColors[code];
    }


    public Material GetColoredMaterial(string colorString)
    {
      if (_CustomColors.ContainsKey(colorString))
        return _CustomColors[colorString];

      string path = _MaterialsPath + colorString + ".mat";
      if (File.Exists(path))
      {
        LoadMaterial(path, colorString);
      }
      else
      {
        BaseMaterial3D mat = _DefaultOpaqueMaterial;
        mat.ResourceName = colorString;
        mat.AlbedoColor = new Color(colorString);

        ResourceSaver.Save(mat, path);
        _CustomColors.Add(mat.ResourceName, mat);
      }

      return _CustomColors[colorString];
    }


    private void LoadMaterial(string path, string colorString)
    {
      BaseMaterial3D mat = (BaseMaterial3D)GD.Load(path);
      if (mat == null)
      {
        GD.PrintErr("Failed to load material: " + colorString);
        mat = _DefaultOpaqueMaterial;
        mat.ResourceName = colorString;
      }

      if (mat.ResourceName != colorString)
      {
        GD.PrintErr("Material name '" + mat.ResourceName + "' and color string '" + colorString + "'don't match! Using default material instead.");
        mat = _DefaultOpaqueMaterial;
        mat.ResourceName = colorString;
      }

      _CustomColors.Add(mat.ResourceName, mat);
    }


    public string[] ModelFileNames
    {
      get { return _ModelFileNames.Keys.ToArray(); }
    }


    public string GetModelByFileName(string modelFileName)
    {
      return _ModelFileNames[modelFileName];
    }


    public string GetSerializedPart(string name)
    {
      try
      {
        name = name.ToLower();

        string serialized = _Parts.ContainsKey(name) ? File.ReadAllText(_Parts[name]) : _Models[name];
        return serialized;
      }
      catch
      {
        // GODOT PORT: I don't know what the Godot equivalent of this is.
        throw new NotImplementedException();
        GD.PrintErr("http://www.ldraw.org/library/tracker/");
        //EditorUtility.DisplayDialog("Error!", "Missing part or wrong part " + name
        //                                        + "! Find it in url from debug console", "Ok", "");
        throw;
      }

    }


    public void InitParts()
    {
      PrepareModels();
      ParseColors();
      _Parts = new Dictionary<string, string>();
      string[] files = Directory.GetFiles(_BasePartsPath, "*.*", SearchOption.AllDirectories);

      foreach (string file in files)
      {
        if (!file.Contains(".meta"))
        {
          string fileName = file.Replace(_BasePartsPath, "").Split('.')[0];

          if (fileName.Contains("\\"))
            fileName = fileName.Split('\\')[1];
          if (!_Parts.ContainsKey(fileName))
            _Parts.Add(fileName, file);
        }
      }
    }


    private void ParseColors()
    {
      _MainColors = new Dictionary<int, Material>();
      using (StreamReader reader = new StreamReader(_ColorConfigPath))
      {
        string line;
        while ((line = reader.ReadLine()) != null)
        {
          Regex regex = new Regex("[ ]{2,}", RegexOptions.None);
          line = regex.Replace(line, " ").Trim();
          string[] args = line.Split(' ');
          if (args.Length <= 1 || args[1] != "!COLOUR")
            continue;
          
          string path = _MaterialsPath + args[2] + ".mat";
          if (File.Exists(path))
            _MainColors.Add(int.Parse(args[4]), (BaseMaterial3D)ResourceLoader.Load(path));
          else
            CreateColor(args, path);
        }
      }


      void CreateColor(string[] args, string path)
      {
        Color color = new Color(args[6]);
        int alphaIndex = Array.IndexOf(args, "ALPHA");
        BaseMaterial3D mat = alphaIndex > 0 ? _DefaultTransparentMaterial : _DefaultOpaqueMaterial;
        mat.ResourceName = args[2];
        mat.AlbedoColor = alphaIndex > 0 ? new Color(color.R, color.G, color.B, int.Parse(args[alphaIndex + 1]) / 256f)
            : color;

        ResourceSaver.Save(mat, path);
        _MainColors.Add(int.Parse(args[4]), mat);
      }
    }


    private void PrepareModels()
    {
      _ModelFileNames = new Dictionary<string, string>();
      string[] files = Directory.GetFiles(_ModelsPath, "*.*", SearchOption.AllDirectories);
      _Models = new Dictionary<string, string>();
      foreach (string file in files)
      {
        using (StreamReader reader = new StreamReader(file))
        {
          string line;
          string filename = String.Empty;

          bool isFirst = true;
          while ((line = reader.ReadLine()) != null)
          {
            Regex regex = new Regex("[ ]{2,}", RegexOptions.None);
            line = regex.Replace(line, " ").Trim();
            string[] args = line.Split(' ');
            if (args.Length > 1 && args[1] == "FILE")
            {

              filename = GetFileName(args, 2);
              if (isFirst)
              {
                _ModelFileNames.Add(Path.GetFileNameWithoutExtension(file), filename);
                isFirst = false;
              }

              if (_Models.ContainsKey(filename))
                filename = String.Empty;
              else
                _Models.Add(filename, String.Empty);
            }

            if (!string.IsNullOrEmpty(filename))
            {
              _Models[filename] += line + "\n";
            }
          }
        }

      }
    }


    public Mesh GetMesh(string name)
    {
      string path = Path.Combine(_MeshesPath, name + ".asset");
      return File.Exists(path) ? (Mesh)ResourceLoader.Load(path) : null;
    }


    public void SaveMesh(Mesh mesh)
    {
      string path = _MeshesPath;
      if (!Directory.Exists(path))
        Directory.CreateDirectory(path);

      path = Path.Combine(path, mesh.ResourceName + ".asset");
      ResourceSaver.Save(mesh, path);
    }


    public static string GetFileName(string[] args, int filenamePos)
    {
      string name = string.Empty;
      for (int i = filenamePos; i < args.Length; i++)
      {
        name += args[i] + ' ';
      }

      return Path.GetFileNameWithoutExtension(name).ToLower();
    }


    public static string GetExtension(string[] args, int filenamePos)
    {
      string name = string.Empty;
      for (int i = filenamePos; i < args.Length; i++)
      {
        name += args[i] + ' ';
      }

      return Path.GetExtension(name).Trim();
    }


    private static LDrawConfig _Instance;


    public static LDrawConfig Instance
    {
      get
      {
        if (_Instance == null)
        {
          _Instance = (LDrawConfig)ResourceLoader.Load(ConfigPath);
        }

        return _Instance;
      }
    }


    private void OnEnable()
    {
      InitParts();
    }
  }
}