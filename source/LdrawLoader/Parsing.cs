using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ldraw
{
    internal static class ModelTree
    {
        internal enum ModelTypes
        {
            invalid,
            prop,
        }

        internal enum ComponentTypes
        {
            invalid,
            body,
        }
    }

    public static class Constants
    {
        public static readonly int kMainColorCode = 16;
        public static readonly int kMagentaColorCode = 5;
        public static readonly string kLdrExtension = "ldr";
        public static readonly string kMpdExtension = "mpd";
        public static readonly string kDatExtension = "dat";
    }

    public enum VertexWinding
    {
        CCW,
        CW,
        Unknown,
    }

    public enum GameEntityType
    {
        Invalid,
        Primitive,
        Component,
        Model
    }

    public enum LdrCommandType
    {
        invalid = -1,
        meta = 0,
        line = 2,
        subfile = 1,
        triangle = 3,
        quad = 4,
        optionalLine = 5,
    }

    // Should I convert this to a class?
    // No. Being a struct means it's never null, and we can force
    // consumers to create a copy or not using the ref keyword.
    // If we used a class, we'd have to manually do a deep copy.
    public struct LdrMetadata
    {
        public LdrMetadata() { }
        public VertexWinding winding = VertexWinding.Unknown;
        public int mainColor = Constants.kMagentaColorCode;
        public string modelName;
        public string fileName;
        public bool invertNext = false;
    }

    public struct Command
    {
        public Command() {}

        // This metadada can be inherited from the parent command, or read from a previous line.
        public LdrMetadata metadata = new LdrMetadata();
        public string commandString;
        public GameEntityType type;
        public Transform3D transform = Transform3D.Identity;
        public Vector3[] vertices;
        public int[] triangles;
        public string subfileName;
        public LdrCommandType ldrCommandType;
    }

    public static class Parsing
    {
        public static string kBasePartsPath = "C:\\dev\\ldraw\\";

        private static readonly string kFILE = "FILE";
        private static readonly string kBFC = "BFC";
        private static readonly string kCW = "CW";
        private static readonly string kCCW = "CCW";
        private static readonly string kCertify = "CERTIFY";
        private static readonly string kNoCertify = "NOCERTIFY";
        private static readonly string kInvertNext = "INVERTNEXT";

        // Token array indices for subfile commands
        private static readonly int kSubFileColour = 1;
        private static readonly int kSubFileX = 2;
        private static readonly int kSubFileY = 3;
        private static readonly int kSubFileZ = 4;
        private static readonly int kSubFileA = 5;
        private static readonly int kSubFileB = 6;
        private static readonly int kSubFileC = 7;
        private static readonly int kSubFileD = 8;
        private static readonly int kSubFileE = 9;
        private static readonly int kSubFileF = 10;
        private static readonly int kSubFileG = 11;
        private static readonly int kSubFileH = 12;
        private static readonly int kSubFileI = 13;
        private static readonly int kSubFileFileName = 14;

        private static readonly Dictionary<string, string> m_ldrawFileIndex = new Dictionary<string, string>();
        private static Dictionary<string, string> m_fileCache = new Dictionary<string, string>();

        static Parsing()
        {
            m_ldrawFileIndex = PrepareFileIndex();
        }

        public static Dictionary<string, string> PrepareFileIndex()
        {
            Dictionary<string, string> parts = new Dictionary<string, string>();
            string[] files = Directory.GetFiles(kBasePartsPath, "*.*", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                if (file.Contains(".meta"))
                    continue;

                string fileName = file.Replace(kBasePartsPath, "");

                // According to the LDRAW spec, https://www.ldraw.org/article/398.html:
                // Filename is the file name of the part including the folder (e.g. s/, 48/)
                // if it is not directly in the parts or p folders. 
                if (fileName.StartsWith("parts\\"))
                    fileName = fileName.Replace("parts\\", "");

                if (fileName.StartsWith("p\\"))
                    fileName = fileName.Replace("p\\", "");

                fileName = fileName.Trim();
                if (!parts.ContainsKey(fileName))
                    parts.Add(fileName, file);
                else
                    GD.PrintErr("Duplicate part: " + fileName);
            }

            return parts;
        }

        public static List<Command> GetCommandsFromFile(in Command parentCommand)
        {
            string fullFilePath = GetFullPathToFile(parentCommand.subfileName);
            List<Command> result = new List<Command>();
            if (!System.IO.File.Exists(fullFilePath))
            {
                OmniLogger.Error("File does not exist: " + fullFilePath);
                return result;
            }

            if (m_fileCache.ContainsKey(parentCommand.subfileName))
            {
                return GetCommandsFromString(parentCommand, m_fileCache[parentCommand.subfileName]);
            }

            try
            {
                string contents = File.ReadAllText(fullFilePath);
                m_fileCache.Add(parentCommand.subfileName, contents);
                return GetCommandsFromString(parentCommand, contents);
            }
            catch (System.Exception e)
            {
                OmniLogger.Error($"Exception '{e.Message}' raised while reading file: " + fullFilePath);
                return result;
            }
        }

        public static List<Command> GetCommandsFromString(in Command parentCommand, string serializedData)
        {
            List<Command> commands = new List<Command>();
            StringReader reader = new StringReader(serializedData);
            LdrMetadata workingMetaData = parentCommand.metadata;
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                Regex regex = new Regex("[ ]{2,}", RegexOptions.None);
                line = regex.Replace(line, " ").Trim();

                if (String.IsNullOrEmpty(line))
                    continue;

                // Warning: Parsecommand may change the metadata and not
                // return true when it does so. (Example: BFC).
                if (ParseCommand(line, in parentCommand.transform, ref workingMetaData, out Command cmd))
                    commands.Add(cmd);
            }

            return commands;
        }

        private static bool ParseCommand(string commandString, in Transform3D parentTransform, ref LdrMetadata metadata, out Command cmd)
        {
            commandString = commandString.Replace("\t", " ");
            commandString = commandString.Trim();
            string[] tokens = commandString.Split();
            cmd = new Command();
            cmd.metadata = metadata;
            cmd.transform = parentTransform;
            int type;
            if (tokens.Length < 2)
                return false;

            if (Int32.TryParse(tokens[0], out type))
            {
                cmd.ldrCommandType = GetCommandType(type);
                switch (cmd.ldrCommandType)
                {
                    case LdrCommandType.meta:
                        return ParseMetaCommand(tokens, ref metadata);

                    case LdrCommandType.subfile:
                        return ParseSubfileCommand(tokens, ref metadata, ref cmd);

                    case LdrCommandType.triangle:

                        cmd.type = GameEntityType.Primitive;
                        cmd.metadata = metadata;
                        cmd.commandString = commandString;
                        break;

                    case LdrCommandType.quad:
                        cmd.type = GameEntityType.Primitive;
                        cmd.metadata = metadata;
                        cmd.commandString = commandString;
                        break;

                    case LdrCommandType.line:
                    case LdrCommandType.optionalLine:
                    default:
                        return false;
                }
            }

            return true;
        }

        private static bool ParseMetaCommand(string[] tokens, ref LdrMetadata metadata)
        {
            if (tokens[1] == kBFC)
            {
                ParseBfcCommand(tokens, ref metadata);
                return false;
            }

            return false;
        }

        private static void ParseBfcCommand(string[] tokens, ref LdrMetadata metadata)
        {
            // Reference: https://www.ldraw.org/article/415.html
            
            // Following the advic here: https://forums.ldraw.org/thread-23274.html
            // I have purposefully chosen to disable all BFC. If we need the performance
            // later on, we can do a deep dive on implementing it properly.
            metadata.winding = VertexWinding.Unknown;
            return;


            //// Winding is implied CCW if not accompanied by explicit CCW or CW token
            //if (tokens.Contains(kCertify))
            //{
            //    metadata.winding = VertexWinding.CCW;
            //}

            //// However, we can also assume the file is certified if it contains any BFC statement
            //// that eEXCEPT for "0 BFC NOCERTIFY".

            //if (tokens.Contains(kCW))
            //{
            //    metadata.winding = VertexWinding.CW;
            //}

            //if (tokens.Contains(kCCW))
            //{
            //    metadata.winding = VertexWinding.CCW;
            //}

            //// There is a potential bug here, where we don't bother to check a NOCERTIFY or CERTIFY
            //// BFC command is the first BFC command in the file, as ordained by the spec. However, any
            //// file exhibiting such behavior is malformed, and undefined behavior is expected in this
            //// case.
            ////
            //// I'm currently imagining future me, reading this with a sigh, and then fixing it.
            //// Sorry future me.
            //if (tokens.Contains(kNoCertify))
            //{
            //    metadata.winding = VertexWinding.Unknown;
            //}

            //if (tokens.Contains(kInvertNext))
            //    metadata.invertNext = true;
        }

        private static bool ParseSubfileCommand(string[] tokens, ref LdrMetadata metadata, ref Command cmd)
        {
            if (tokens.Length < 15)
            {
                OmniLogger.Error("Subfile command has too few tokens");
                return false;
            }

            tokens[kSubFileFileName] = tokens[kSubFileFileName].Trim();

            // Tranforms in subfiles are relative to parent transforms.
            Transform3D childTfm = GetCommandTransform(tokens);
            cmd.transform = cmd.transform * childTfm;
            cmd.type = GetGameEntityType(tokens[kSubFileFileName]);
            cmd.subfileName = tokens[kSubFileFileName]; 
            return true;
        }

        private static Transform3D GetCommandTransform(string[] tokens)
        {
            // TODO: Do I need to check for BFC INVERTNEXT here?
            float[] param = new float[12];
            for (int i = 0; i < param.Length; i++)
            {
                int argNum = i + 2;
                if (!Single.TryParse(tokens[argNum], out param[i]))
                    OmniLogger.Error("Failed to parse transform from subfile, with value: " + tokens[argNum]);
            }

            System.Numerics.Matrix4x4 mat4x4 = new System.Numerics.Matrix4x4(
                param[3], param[6], param[9], 0,
                param[4], param[7], param[10], 0,
                param[5], param[8], param[11], 0,
                param[0], param[1], param[2], 1);

            return Transforms.MakeGodotTransformFrom4x4(mat4x4);
        }

        private static GameEntityType GetGameEntityType(string subfileName)
        {
            if (subfileName.EndsWith(Constants.kDatExtension, StringComparison.OrdinalIgnoreCase))
                return GameEntityType.Primitive;

            if (!subfileName.EndsWith(Constants.kLdrExtension, StringComparison.OrdinalIgnoreCase) &&
                !subfileName.EndsWith(Constants.kMpdExtension, StringComparison.OrdinalIgnoreCase))
                return GameEntityType.Invalid;
           
            subfileName = Path.GetFileNameWithoutExtension(subfileName);
            if (!IsAModelOrComponentName(subfileName))
                return GameEntityType.Invalid;

            string workinSubfileName = subfileName.Replace("[", "").Replace("]", "").Trim();
            string[] tokens = workinSubfileName.Split(' ');
            if (tokens.Length != 2)
            {
                // This is a shity hack. Fix this so we can have spaces in model names please!
                OmniLogger.Error($"Invalid model or component name: {subfileName}, can't have more than one space!");
                return GameEntityType.Invalid;
            }

            try
            {
                Enum.TryParse(tokens[0], out GameEntityType value);
                return value;
            }
            catch
            {
                OmniLogger.Error($"Invalid model or component type: {tokens[0]}");
                return GameEntityType.Invalid;
            }
        }

        private static bool IsAModelOrComponentName(string subfileName)
        {
            if (subfileName.StartsWith("[") && subfileName.EndsWith("]"))
                return true;

            return false;
        }

        private static LdrCommandType GetCommandType(int value)
        {
            if (value < 0 || value > 5)
                return LdrCommandType.invalid;
            
            return (LdrCommandType)value;
        }

        public static Vector3[] DeserializeQuad(string serialized)
        {
            string[] args = serialized.Split(' ');
            float[] param = new float[12];
            for (int i = 0; i < param.Length; i++)
            {
                int argNum = i + 2;
                if (!float.TryParse(args[argNum], out param[i]))
                {
                    GD.PrintErr(
                        String.Format(
                            "Something wrong with parameters in line drawn command. ParamNum:{0}, Value:{1}",
                            argNum,
                            args[argNum]));
                    return new Vector3[0];
                }
            }

            return new Vector3[]
            {
                new Vector3(param[0], param[1], param[2]),
                new Vector3(param[3], param[4], param[5]),
                new Vector3(param[6], param[7], param[8]),
                new Vector3(param[9], param[10], param[11])
            };
        }

        public static Vector3[] DeserializeTriangle(string serialized)
        {
            var args = serialized.Split(' ');
            float[] param = new float[9];
            for (int i = 0; i < param.Length; i++)
            {
                int argNum = i + 2;
                if (!Single.TryParse(args[argNum], out param[i]))
                {
                    GD.PrintErr(
                        String.Format(
                            "Something wrong with parameters in line drawn command. ParamNum:{0}, Value:{1}",
                            argNum,
                            args[argNum]));
                    return new Vector3[0];
                }
            }

            return new Vector3[]
            {
                new Vector3(param[0], param[1], param[2]),
                new Vector3(param[3], param[4], param[5]),
                new Vector3(param[6], param[7], param[8])
            };
        }

        private static string GetFullPathToFile(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                OmniLogger.Error("File path is null or empty");
                return string.Empty;
            }

            if (m_ldrawFileIndex.ContainsKey(path))
                return m_ldrawFileIndex[path];

            OmniLogger.Error($"File {path} does not exist in the file index");
            return string.Empty;
        }
    }   // public static class Parsing
}
