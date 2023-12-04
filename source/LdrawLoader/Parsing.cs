using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Ldraw
{
    public static class Constants
    {
        public static readonly int kMainColorCode = 16;
        public static readonly int kMagentaColorCode = 5;
        public static readonly string kLdrExtension = "ldr";
        public static readonly string kMpdExtension = "mpd";
        public static readonly string kDatExtension = "dat";

        public static string kBasePartsPath = "C:\\dev\\ldraw\\";

        public static readonly string kEmbeddedFileStart = "0 FILE";
        public static readonly string kEmbeddedFileEnd = "0 NOFILE";
        public static readonly string kBFC = "BFC";
        public static readonly string kCW = "CW";
        public static readonly string kCCW = "CCW";
        public static readonly string kCertify = "CERTIFY";
        public static readonly string kNoCertify = "NOCERTIFY";
        public static readonly string kInvertNext = "INVERTNEXT";
        public static readonly string kName = "0 Name:";


        // Token array indices for subfile commands
        public static readonly int kSubFileColour = 1;
        public static readonly int kSubFileX = 2;
        public static readonly int kSubFileY = 3;
        public static readonly int kSubFileZ = 4;
        public static readonly int kSubFileA = 5;
        public static readonly int kSubFileB = 6;
        public static readonly int kSubFileC = 7;
        public static readonly int kSubFileD = 8;
        public static readonly int kSubFileE = 9;
        public static readonly int kSubFileF = 10;
        public static readonly int kSubFileG = 11;
        public static readonly int kSubFileH = 12;
        public static readonly int kSubFileI = 13;
        public static readonly int kSubFileFileName = 14;
    }

    public enum VertexWinding
    {
        CCW,
        CW,
        Unknown,
    }

    public enum CommandType
    {
        Invalid,
        Model,
        Component,
        Part,
        Triangle,
        Quad,
        SubFile,
    }

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
        public Command() { }

        // This metadada can be inherited from the parent command, or read from a previous line.
        public LdrMetadata metadata = new LdrMetadata();
        public ModelTree.ModelTypes modelType = ModelTree.ModelTypes.invalid;
        public ModelTree.ComponentTypes componentType = ModelTree.ComponentTypes.invalid;
        public string commandString;
        public CommandType type;
        public Transform3D transform = Transform3D.Identity;
        public Vector3[] vertices;
        public int[] triangles;
        public string subfileName;
    }

    public static class Parsing
    {
        public static bool IsLdrOrMpdFile(string fileName)
        {
            if (fileName.EndsWith(Constants.kLdrExtension, StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith(Constants.kMpdExtension, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        public static List<Command> GetCommandsFromFile(in Command parentCommand)
        {
            try
            {
                List<Command> result = new List<Command>();
                string contents = FileCache.OpenFile(parentCommand.subfileName);
                return ParseSerializedData(parentCommand, contents);
            }
            catch (System.Exception e)
            {
                OmniLogger.Error($"Exception '{e.Message}' raised while parsing file: {parentCommand.subfileName}");
                return new List<Command>();
            }
        }

        private static List<Command> ParseSerializedData(in Command parentCommand, string serializedData)
        {
            ParseEmbeddedFiles(parentCommand.subfileName, serializedData);
            LdrMetadata workingMetaData = parentCommand.metadata;
            List<Command> commands = new List<Command>();
            StringReader reader = new StringReader(serializedData);
            string line = "";
            bool readingEmbeddedFile = false;

            // God I hate these control statement roller coasters.
            try
            {
                while ((line = reader.ReadLine()) != null)
                {
                    if (String.IsNullOrEmpty(line))
                        continue;

                    line = line.Trim().Trim('\t');

                    // I think removes dubliate spaces, but I'm not sure.
                    // Drop it into a regex sandbox some time and see.
                    Regex regex = new Regex("[ ]{2,}", RegexOptions.None);
                    line = regex.Replace(line, " ").Trim();

                    Command cmd = new Command();
                    if (line.StartsWith(Constants.kEmbeddedFileStart))
                    {
                        readingEmbeddedFile = true;
                        cmd.type = CommandType.SubFile;
                        cmd.subfileName = GetEmbeddedFileNameFromCommandString(parentCommand.subfileName, line);
                        commands.Add(cmd);
                        continue;
                    }

                    if (line.StartsWith(Constants.kEmbeddedFileEnd))
                    {
                        readingEmbeddedFile = false;
                        continue;
                    }

                    if (readingEmbeddedFile)
                    {
                        continue;
                    }

                    // Warning: Parsecommand may change the metadata and not
                    // return true when it does so. (Example: BFC).
                    if (ParseCommand(line, in parentCommand.transform, ref workingMetaData, out cmd))
                        commands.Add(cmd);
                }
            }
            catch (System.Exception e)
            {
                OmniLogger.Error($"Exception '{e.Message}' raised while parsing file: {parentCommand.subfileName}. Line: {line}");
            }

            return commands;
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

        // Separates an LDraw file into a dictionary of files. The dictionary contains at least one entry,
        // the parent file. If the parent file contains embedded files, the dictionary will contain an entry
        // for each embedded file.
        private static void ParseEmbeddedFiles(string parentFileName, string serializedFileContents)
        {
            StringReader reader = new StringReader(serializedFileContents);
            string embeddedFileContents = string.Empty;
            string embeddedFileName = string.Empty;
            string parentFileContents = string.Empty;

            // God I hate these control statement roller coasters.
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                try
                {
                    if (String.IsNullOrEmpty(line))
                        continue;

                    line = line.Trim().Trim('\t');

                    // I think removes duplicate spaces, but I'm not sure.
                    // Drop it into a regex sandbox some time and see.
                    Regex regex = new Regex("[ ]{2,}", RegexOptions.None);
                    line = regex.Replace(line, " ").Trim();

                    if (line.StartsWith(Constants.kEmbeddedFileStart))
                    {
                        embeddedFileName = GetEmbeddedFileNameFromCommandString(parentFileName, line);
                        embeddedFileContents = string.Empty;
                        continue;
                    }

                    if (line.StartsWith(Constants.kEmbeddedFileEnd))
                    {
                        FileCache.CacheFileContents(embeddedFileName, embeddedFileContents);
                        embeddedFileName = string.Empty;
                        embeddedFileContents = string.Empty;
                        continue;
                    }

                    if (String.IsNullOrEmpty(embeddedFileName))
                    {
                        // add to parent file entry
                        parentFileContents += line + System.Environment.NewLine;
                    }
                    else
                    {
                        embeddedFileContents += line + System.Environment.NewLine;
                    }
                }
                catch (System.Exception e)
                {
                    OmniLogger.Error($"Exception '{e.Message}' while parsing embedded file: {parentFileName}. Line: {line}");
                }
            }   // while

            FileCache.CacheFileContents(System.IO.Path.GetFileName(parentFileName), parentFileContents);
        }

        private static string GetEmbeddedFileNameFromCommandString(string parentFileName, string line)
        {
            string embeddedFileName = System.IO.Path.GetFileName(parentFileName);
            embeddedFileName += "/" + line.Replace(Constants.kEmbeddedFileStart, "").Trim();
            return embeddedFileName;
        }

        private static bool ParseCommand(string commandString, in Transform3D parentTransform, ref LdrMetadata metadata, out Command cmd)
        {
            commandString = commandString.Replace("\t", " ");
            commandString = commandString.Trim();
            string[] tokens = commandString.Split();
            cmd = new Command();
            cmd.metadata = metadata;
            cmd.transform = parentTransform;
            cmd.commandString = commandString;
            if (tokens.Length < 2)
                return false;


            LdrCommandType type = GetCommandType(int.Parse(tokens[0]));
            switch (type)
            {
                case LdrCommandType.meta:
                    return ParseMetaCommand(tokens, ref metadata, ref cmd);

                case LdrCommandType.subfile:
                    return ParseSubfileCommand(tokens, ref metadata, ref cmd);

                case LdrCommandType.triangle:
                    cmd.type = CommandType.Triangle;
                    break;
                case LdrCommandType.quad:
                    cmd.type = CommandType.Quad;
                    break;

                case LdrCommandType.line:
                case LdrCommandType.optionalLine:
                default:
                    return false;
            }

            return true;
        }

        private static bool ParseMetaCommand(string[] tokens, ref LdrMetadata metadata, ref Command cmd)
        {
            if (tokens[1] == Constants.kBFC)
            {
                ParseBfcCommand(tokens, ref metadata);
                return false;
            }

            if (tokens[1] == Constants.kEmbeddedFileStart)
            {
                cmd.type = CommandType.SubFile;
                cmd.subfileName = GetEmbeddedFileNameFromCommandString(metadata.fileName, cmd.commandString);
                return true;
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

            // Tranforms in subfiles are relative to parent transforms.
            Transform3D childTfm = GetCommandTransform(tokens);
            cmd.transform = cmd.transform * childTfm;
            cmd.subfileName = GetSubileName(tokens);

            if (IsLdrOrMpdFile(cmd.subfileName))
            {
                cmd.subfileName = GetEmbeddedFileNameFromCommandString(metadata.fileName, cmd.subfileName);
                cmd.type = CommandType.SubFile;
                return true;
            }

            ModelTree.ModelTypes modelType = ModelTree.GetModelTypeFromCommandString(cmd.commandString);
            if (modelType != ModelTree.ModelTypes.invalid)
            {
                cmd.modelType = modelType;
                cmd.type = CommandType.Model;
                cmd.subfileName = metadata.fileName;
                return true;
            }

            ModelTree.ComponentTypes componentType = ModelTree.GetComponentTypeFromCommandString(cmd.commandString);
            if (componentType != ModelTree.ComponentTypes.invalid)
            {
                cmd.componentType = componentType;
                cmd.type = CommandType.Component;
                cmd.subfileName = metadata.fileName;
                return true;
            }

            cmd.type = CommandType.Part;
            return true;
        }

        private static string GetSubileName(string[] tokens)
        {
            if (tokens.Length <= Constants.kSubFileFileName)
            {
                OmniLogger.Error("Subfile command has too few tokens");
                return string.Empty;
            }

            if (tokens.Length == Constants.kSubFileFileName + 1)
            {
                return tokens[Constants.kSubFileFileName];
            }

            string result = "";
            for (int i = Constants.kSubFileFileName; i < tokens.Length; i++)
            {
                result += " " + tokens[i];
            }
            return result.Trim();
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

        private static LdrCommandType GetCommandType(int value)
        {
            if (value < 0 || value > 5)
                return LdrCommandType.invalid;

            return (LdrCommandType)value;
        }
    }   // public static class Parsing
}
