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
        public ModelTree.ModelTypes modelType = ModelTree.ModelTypes.invalid;
        public LdrCommandType ldrCommandType = LdrCommandType.invalid;
        public LdrMetadata metadata = new LdrMetadata();
        public string commandString;
        public GameEntityType type;
        public Transform3D transform = Transform3D.Identity;
        public Vector3[] vertices;
        public int[] triangles;
        public string subfileName;
    }

    public static class Parsing
    {
        private static readonly Dictionary<string, string> m_ldrawFileIndex = new Dictionary<string, string>();
        private static Dictionary<string, string> m_fileCache = new Dictionary<string, string>();

        static Parsing()
        {
            m_ldrawFileIndex = PrepareFileIndex();
        }

        public static Dictionary<string, string> PrepareFileIndex()
        {
            Dictionary<string, string> parts = new Dictionary<string, string>();
            string[] files = Directory.GetFiles(Constants.kBasePartsPath, "*.*", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                if (file.Contains(".meta"))
                    continue;

                string fileName = file.Replace(Constants.kBasePartsPath, "");

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
            List<Command> result = new List<Command>();
            string contents = OpenFile(parentCommand.subfileName);
            return GetCommandsFromString(parentCommand, contents);
        }

        public static string OpenFile(string fileName)
        {
            if (String.IsNullOrEmpty(fileName))
                return string.Empty;
            
            if (m_fileCache.ContainsKey(fileName))
                return m_fileCache[fileName];

            string fullFilePath = GetFullPathToFile(fileName);
            if (!System.IO.File.Exists(fullFilePath))
            {
                OmniLogger.Error("File does not exist: " + fullFilePath);
                return string.Empty;
            }

            string contents = File.ReadAllText(fullFilePath);
            m_fileCache.Add(fileName, contents);
            return contents;
        }

        public static List<Command> GetCommandsFromString(in Command parentCommand, string serializedData)
        {
            ParseEmbeddedFiles(m_fileCache, parentCommand.subfileName, serializedData);
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

                    if (line.StartsWith(Constants.kEmbeddedFileStart))
                    {
                        readingEmbeddedFile = true;
                        continue;
                    }

                    if (line.StartsWith(Constants.kEmbeddedFileEnd))
                    {
                        readingEmbeddedFile = false;
                        continue;
                    }

                    if (readingEmbeddedFile)
                        continue;

                    // Warning: Parsecommand may change the metadata and not
                    // return true when it does so. (Example: BFC).
                    if (ParseCommand(line, in parentCommand.transform, ref workingMetaData, out Command cmd))
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

        public static List<Model> GetModelsFromFile(string fullFilePath)
        {
            List<Model> results = new List<Model>();
            if (!System.IO.File.Exists(fullFilePath))
            {
                OmniLogger.Error("File does not exist: " + fullFilePath);
                return results;
            }

            // Key = fileName, Value = fileContents
            Dictionary<string, string> embeddedFiles = new Dictionary<string, string>();
            string contents = string.Empty;

            try
            {
                contents = File.ReadAllText(fullFilePath); 
            }
            catch (System.Exception e)
            {
                OmniLogger.Error($"Exception '{e.Message}' raised while reading file: " + fullFilePath);
                return results;
            }

            ParseEmbeddedFiles(embeddedFiles, fullFilePath, contents);
            foreach (KeyValuePair<string, string> kvp in embeddedFiles)
            {
                CacheFileContents(m_fileCache, kvp.Key, kvp.Value);
                GetModelsFromSerializedFile(results, kvp.Key, kvp.Value);
            }

            return results;
         }

        // This function assumes there are no embedded files in the serialized file.
        private static void GetModelsFromSerializedFile(List<Model> results, string fileName, string serializedFileContents)
        {
            LdrMetadata metadata = new LdrMetadata();
            metadata.fileName = fileName;
            metadata.modelName = "unknown";
            string line;
            StringReader reader = new StringReader(serializedFileContents);
            while ((line = reader.ReadLine()) != null)
            {
                if (String.IsNullOrEmpty(line))
                    continue;

                if (line.StartsWith(Constants.kName))
                {                     
                    metadata.modelName = line.Replace(Constants.kName, "").Trim();
                    continue;
                }

                Command modelCommand = ModelTree.GetModelCommand(line);
                if (modelCommand.modelType == ModelTree.ModelTypes.invalid)
                    continue;

                modelCommand.metadata = metadata;
                modelCommand.subfileName = fileName;
                modelCommand.commandString = "1 0 0 0 0 1 0 0 0 1 0 0 0 1 " + fileName;
                results.Add(new Model(modelCommand));
            }

         }

        private static bool IsModelCommand(string line)
        {
            

            return false;
        }

        // Separates an LDraw file into a dictionary of files. The dictionary contains at least one entry,
        // the parent file. If the parent file contains embedded files, the dictionary will contain an entry
        // for each embedded file.
        private static void ParseEmbeddedFiles(Dictionary<string, string> cache, string parentFileName, string serializedFileContents)
        {
            if (cache == null)
                return;

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
                        embeddedFileName = System.IO.Path.GetFileName(parentFileName);
                        embeddedFileName += "/" + line.Replace(Constants.kEmbeddedFileStart, "").Trim();
                        embeddedFileContents = string.Empty;
                        continue;
                    }

                    if (line.StartsWith(Constants.kEmbeddedFileEnd))
                    {
                        CacheFileContents(cache, embeddedFileName, embeddedFileContents);
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

            CacheFileContents(cache, System.IO.Path.GetFileName(parentFileName), parentFileContents);
        }

        private static void CacheFileContents(Dictionary<string, string> cache, string fileName, string contents)
        {
            if (cache == null)
                return;

            if (cache.ContainsKey(fileName))
            {
                OmniLogger.Error($"File {fileName} already exists in the Ldraw file cache!");
                return;
            }

            cache.Add(fileName, contents);
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
                    case LdrCommandType.quad:
                        cmd.type = GameEntityType.Primitive;
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
            if (tokens[1] == Constants.kBFC)
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

            // Tranforms in subfiles are relative to parent transforms.
            Transform3D childTfm = GetCommandTransform(tokens);
            cmd.transform = cmd.transform * childTfm;
            cmd.subfileName = GetSubileName(tokens);
            cmd.type = ModelTree.GetGameEntityType(cmd);
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

        private static string GetFullPathToFile(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                OmniLogger.Error("File path is null or empty");
                return string.Empty;
            }

            if (path.EndsWith(Constants.kLdrExtension, StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(Constants.kMpdExtension, StringComparison.OrdinalIgnoreCase))                          
            {
                // TODO: make this locate the models folder in the project.
                return path;
            }

            if (m_ldrawFileIndex.ContainsKey(path))
                return m_ldrawFileIndex[path];

            OmniLogger.Error($"File {path} does not exist in the file index");
            return string.Empty;
        }
    }   // public static class Parsing
}
