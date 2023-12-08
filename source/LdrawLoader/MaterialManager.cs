using Godot;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System;

namespace Ldraw
{
    using LdrColor = Int32;
    public static class MaterialManager
    {
        private static BaseMaterial3D _DefaultOpaqueMaterial;
        private static BaseMaterial3D _DefaultTransparentMaterial;
        private static string m_colorConfigPath = "";
        private static Dictionary<string, Material> m_customColors;
        private static Dictionary<int, Material> m_officialColors;
        private static string m_materialsPath = "res://assets/generated/materials/";

        public static void Configure(string colorConfigPath, BaseMaterial3D defaultOpaqueMaterial, BaseMaterial3D defaultTransMaterial)
        {
            m_colorConfigPath = colorConfigPath;
            _DefaultOpaqueMaterial = defaultOpaqueMaterial;
            _DefaultTransparentMaterial = defaultTransMaterial;
            ParseColors();
        }

        private static void LoadMaterial(string path, string colorString)
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

            m_customColors.Add(mat.ResourceName, mat);
        }

        public static Material GetColoredMaterial(LdrColor code)
        {
            Material mat;
            if (m_officialColors.TryGetValue(code, out mat))
                return mat;

            return _DefaultOpaqueMaterial;
        }

        public static Material GetColoredMaterial(string colorString)
        {
            if (m_customColors.ContainsKey(colorString))
                return m_customColors[colorString];

            string path = m_materialsPath + colorString + ".mat";
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
                m_customColors.Add(mat.ResourceName, mat);
            }

            return m_customColors[colorString];
        }

        private static void ParseColors()
        {
            m_officialColors = new Dictionary<int, Material>();
            using (StreamReader reader = new StreamReader(m_colorConfigPath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Regex regex = new Regex("[ ]{2,}", RegexOptions.None);
                    line = regex.Replace(line, " ").Trim();
                    string[] args = line.Split(' ');
                    if (args.Length <= 1 || args[1] != "!COLOUR")
                        continue;

                    string matName = $"ldr_{args[4]}_{args[2]}";
                    string path = m_materialsPath + matName + ".tres";
                    if (File.Exists(ProjectSettings.GlobalizePath(path)))
                    {
                        BaseMaterial3D mat = (BaseMaterial3D)ResourceLoader.Load(path);
                        if (mat != null)
                            m_officialColors.Add(int.Parse(args[4]), mat);
                    }
                    else
                    {
                        CreateColor(args, path, matName);
                    }
                }
            }

        }
        private static void CreateColor(string[] args, string path, string name)
        {
            Color color = new Color(args[6]);
            int alphaIndex = Array.IndexOf(args, "ALPHA");
            BaseMaterial3D mat = alphaIndex > 0 ? _DefaultTransparentMaterial : _DefaultOpaqueMaterial;
            mat.ResourceName = name;
            mat.AlbedoColor = alphaIndex > 0 ? new Color(color.R, color.G, color.B, int.Parse(args[alphaIndex + 1]) / 256f)
                : color;

            mat.ShadingMode = BaseMaterial3D.ShadingModeEnum.PerPixel;
            Error err = ResourceSaver.Save(mat, path);
            if (err != Error.Ok)
                OmniLogger.Error("Failed to save material: " + args[2] + $", error: {err}");

            m_officialColors.Add(int.Parse(args[4]), mat);
        }

    }   // class MaterialManager
}
