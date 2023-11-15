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
        // This metadada can be inherited from the parent command, or read from a previous line.
        public LdrMetadata metadata;
        public string commandString;
        public GameEntityType type;
        public System.Numerics.Matrix4x4 transform;
        public Vector3[] vertices;
        public int[] triangles;
        public string subfileName;
    }

    public static class Parsing
    {
        internal enum LdrCommandType
        {
            invalid = -1,
            meta = 0,
            line = 2,
            subfile = 1,
            triangle = 3,
            quad = 4,
            optionalLine = 5,
        }

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

        private static Dictionary<string, string> m_fileCache = new Dictionary<string, string>();

        public static List<Command> GetCommandsFromFile(LdrMetadata metadata, string fullFilePath)
        {
            List<Command> result = new List<Command>();
            if (!System.IO.File.Exists(fullFilePath))
            {
                Logger.Error("File does not exist: " + fullFilePath);
                return result;
            }

            if (m_fileCache.ContainsKey(fullFilePath))
            {
                return GetCommandsFromString(metadata, m_fileCache[fullFilePath]);
            }

            try
            {
                string contents = File.ReadAllText(fullFilePath);
                m_fileCache.Add(fullFilePath, contents);
                return GetCommandsFromString(metadata, contents);
            }
            catch (System.Exception e)
            {
                Logger.Error($"Exception '{e.Message}' raised while reading file: " + fullFilePath);
                return result;
            }
        }

        public static List<Command> GetCommandsFromString(LdrMetadata metadata, string serializedData)
        {
            List<Command> commands = new List<Command>();
            StringReader reader = new StringReader(serializedData);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                Regex regex = new Regex("[ ]{2,}", RegexOptions.None);
                line = regex.Replace(line, " ").Trim();

                if (String.IsNullOrEmpty(line))
                    continue;

                Command cmd;

                // Warning: Parsecommand may change the metadata and not
                // return true when it does so. (Example: BFC).
                if (ParseCommand(line, ref metadata, out cmd))
                    commands.Add(cmd);
            }

            return commands;
        }

        private static bool ParseCommand(string commandString, ref LdrMetadata metadata, out Command cmd)
        {
            commandString = commandString.Replace("\t", " ");
            commandString = commandString.Trim();
            string[] tokens = commandString.Split();
            cmd = new Command();
            int type;
            if (tokens.Length < 2)
                return false;

            if (Int32.TryParse(tokens[0], out type))
            {
                LdrCommandType commandType = GetCommandType(type);
                switch (commandType)
                {
                    case LdrCommandType.meta:
                        return ParseMetaCommand(tokens, ref metadata);

                    case LdrCommandType.subfile:
                        cmd.type = GameEntityType.Component;
                        cmd.metadata = metadata;
                        cmd.commandString = commandString;
                        break;

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
                return true;
            }

            return false;
        }

        private static void ParseBfcCommand(string[] tokens, ref LdrMetadata metadata)
        {
            // Reference: https://www.ldraw.org/article/415.html

            // Winding is implied CCW if not accompanied by explicit CCW or CW token
            if (tokens.Contains(kCertify))
            {
                metadata.winding = VertexWinding.CCW;
            }

            // However, we can also assume the file is certified if it contains any BFC statement
            // that eEXCEPT for "0 BFC NOCERTIFY".

            if (tokens.Contains(kCW))
            {
                metadata.winding = VertexWinding.CW;
            }

            if (tokens.Contains(kCCW))
            {
                metadata.winding = VertexWinding.CCW;
            }

            // There is a potential bug here, where we don't bother to check a NOCERTIFY or CERTIFY
            // BFC command is the first BFC command in the file, as ordained by the spec. However, any
            // file exhibiting such behavior is malformed, and undefined behavior is expected in this
            // case.
            //
            // I'm currently imagining future me, reading this with a sigh, and then fixing it.
            // Sorry future me.
            if (tokens.Contains(kNoCertify))
            {
                metadata.winding = VertexWinding.Unknown;
            }

            if (tokens.Contains(kInvertNext))
                metadata.invertNext = true;
        }

        private static bool ParseSubfileCommand(string[] tokens, ref LdrMetadata metadata, ref Command cmd)
        {
            if (tokens.Length < 15)
                return false;

            tokens[kSubFileFileName] = tokens[kSubFileFileName].Trim();
            cmd.subfileName = tokens[kSubFileFileName];
            cmd.type = GetGameEntityType(tokens[kSubFileFileName]);
            
            // TODO: parse the transform. DDO we need to worry about BFC right now?

            return true;
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
                Logger.Error($"Invalid model or component name: {subfileName}, can't have more than one space!");
                return GameEntityType.Invalid;
            }

            try
            {
                Enum.TryParse(tokens[0], out GameEntityType value);
                return value;
            }
            catch
            {
                Logger.Error($"Invalid model or component type: {tokens[0]}");
                return GameEntityType.Invalid;
            }
        }

        private static bool IsAModelOrComponentName(string subfileName)
        {
            if (subfileName.StartsWith("[") && subfileName.EndsWith("]"))
                return true;

            return false;
        }


        public static string GetFileContents(string relativePath)
        {

            throw new System.NotImplementedException();
        }

        // Returns the name of a sufile refernce from a subfile command. Appends the extension .virtual
        // if the subfile is a virtual file. (i.e. the filename does not have an extension)
        public static string GetSubFileName(string commandString)
        {
            // examples:

            // 1 16 0 0 0 1 0 0 0 1 0 0 0 1 1-8sphe.dat
            // should return 1-8sphe.dat

            // 1 7 0 0 0 1 0 0 0 1 0 0 0 1 quadplane.ldr
            // should return quadplane.ldr
            throw new System.NotImplementedException();
        }

        public static string GetModelName(string commandString)
        {
            // basically just subfile name without extension
            throw new System.NotImplementedException();
        }

        private static LdrCommandType GetCommandType(int value)
        {
            // This function is gross. Maybe someday I'll make a template out of it or something cleaner.
            LdrCommandType[] arr = new[] { LdrCommandType.invalid, LdrCommandType.optionalLine };
            if (Array.IndexOf(arr, value) < 0)
            {
                return LdrCommandType.invalid;
            }

            return (LdrCommandType)value;
        }
    }
}
