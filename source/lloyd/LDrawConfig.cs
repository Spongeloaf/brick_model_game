﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Godot;


namespace Lloyd
{
    public partial class LDrawConfig : Resource
    {
        [Export] private string _BasePartsPath = "C:\\dev\\ldraw\\";
        [Export] private string _ModelsPath = "res://models/imports/";
        [Export] private string _MaterialsPath = "res://assets/generated/materials/";
        [Export] private string _MeshesPath = "res://assets/generated/meshes/";
        [Export] private string _ScenesPath = " res://assets/generated/scenes/";
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


        public LDrawConfig()
        {
            _ColorConfigPath = _BasePartsPath + "LDConfig.ldr";
            _DefaultOpaqueMaterial = ResourceLoader.Load<BaseMaterial3D>("res://assets/materials/importDefaults/DefaultOpaque.tres");
            _DefaultTransparentMaterial = ResourceLoader.Load<BaseMaterial3D>("res://assets/materials/importDefaults/DefaultTransparent.tres");
            InitParts();
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

        public void SetMainColor(int code)
        {
            if (code != Constants.kMainColorCode)
                _MainColors[Constants.kMainColorCode] = _MainColors[code];
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
            if (_ModelFileNames.ContainsKey(modelFileName))
                return _ModelFileNames[modelFileName];

            return null;
        }


        public string GetSerializedPart(string name)
        {
            if (name.Contains("3062bs01"))
            {
                int i = 0;
            }

            try
            {
                name = name.ToLower();

                string serialized = _Parts.ContainsKey(name) ? File.ReadAllText(_Parts[name]) : _Models[name];
                return serialized;
            }
            catch
            {
                // GODOT PORT: I don't know what the Godot equivalent of this is.
                GD.PrintErr("http://www.ldraw.org/library/tracker/");
                //EditorUtility.DisplayDialog("Error!", "Missing part or wrong part " + name
                //                                        + "! Find it in url from debug console", "Ok", "");
                return "";
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
                if (file.Contains("s\\3062bs01"))
                {
                    int i = 0;
                }

                if (file.Contains(".meta"))
                    continue;

                string fileName = file.Replace(_BasePartsPath, "");

                // According to the LDRAW spec, https://www.ldraw.org/article/398.html:
                // Filename is the file name of the part including the folder (e.g. s/, 48/)
                // if it is not directly in the parts or p folders. 
                if (fileName.StartsWith("parts\\"))
                    fileName = fileName.Replace("parts\\", "");

                if (fileName.StartsWith("p\\"))
                    fileName = fileName.Replace("p\\", "");

                fileName = fileName.Trim();
                if (!_Parts.ContainsKey(fileName))
                    _Parts.Add(fileName, file);
                else
                    GD.PrintErr("Duplicate part: " + fileName);
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

                    string path = _MaterialsPath + args[2] + ".tres";
                    if (File.Exists(ProjectSettings.GlobalizePath(path)))
                    {
                        BaseMaterial3D mat = (BaseMaterial3D)ResourceLoader.Load(path);
                        if (mat != null)
                            _MainColors.Add(int.Parse(args[4]), mat);
                    }
                    else
                    {
                        CreateColor(args, path);
                    }
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

                mat.ShadingMode = BaseMaterial3D.ShadingModeEnum.PerPixel; 
                Error err = ResourceSaver.Save(mat, path);
                if (err != Error.Ok)
                    Debug.WriteLine("Failed to save material: " + args[2] + $", error: {err}");

                _MainColors.Add(int.Parse(args[4]), mat);
            }
        }


        private void PrepareModels()
        {
            _ModelFileNames = new Dictionary<string, string>();
            string[] files = Directory.GetFiles(ProjectSettings.GlobalizePath(_ModelsPath), "*.*", SearchOption.AllDirectories);
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
                            filename = filename.Trim();
                            if (isFirst)
                            {
                                _ModelFileNames.Add(Path.GetFileNameWithoutExtension(file), filename);
                                isFirst = false;
                            }

                            filename = filename.Trim();
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


        public static string GetFileName(string[] args, int filenamePos, bool includeExtension = false)
        {
            string name = string.Empty;
            for (int i = filenamePos; i < args.Length; i++)
            {
                name += args[i] + ' ';
            }

            if (includeExtension)
                return Path.GetFileName(name).ToLower();
            else
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
                    _Instance = new LDrawConfig();
                }

                return _Instance;
            }
        }


        private void OnEnable()
        {
            InitParts();
        }

        public Error SaveNodeAsScene(Node3D node)
        {
            if (node == null)
                return Error.InvalidData;

            foreach (Node child in node.GetChildren())
            {
                child.Owner = node;
            }

            PackedScene scene = new PackedScene();
            scene.Pack(node);

            //string path = "";
            //if (OS.HasFeature("editor"))
            //{

            //  // Running from an editor binary.
            //  // `path` will contain the absolute path to `hello.txt` located in the project root.
            //  path = ProjectSettings.GlobalizePath(_ScenesPath + node.Name + ".tscn");
            //}
            //else
            //{
            //  // Running from an exported project.
            //  // `path` will contain the absolute path to `hello.txt` next to the executable.
            //  // This is *not* identical to using `ProjectSettings.globalize_path()` with
            //  //a `res://` path, but is close enough in spirit.
            //  path = OS.GetExecutablePath().GetBaseDir().PathJoin(node.Name + ".tscn");
            //}

            string path = "C:\\dev\\brick_model_game\\assets\\generated\\models\\" + node.Name + ".tscn";
            return ResourceSaver.Save(scene, path);
        }
    }
}