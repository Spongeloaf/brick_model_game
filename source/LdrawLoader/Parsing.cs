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
        public static readonly string kBFC = "BFC";
    }

    public enum VertexWinding
    {
        CCW,
        CW,
        Unknown,
    }

    public enum LdrawCommandType
    {
        Part,
        Primitive,
        Component,
        Model
    }

    public struct LdrMetadata
    {
        public LdrMetadata() { }
        public VertexWinding winding = VertexWinding.Unknown;
        public int mainColor = Constants.kMagentaColorCode;
        public string name;
    }

    public  struct Command
    {
        // This metadada can be inherited from the parent command, or read from a previous line.
        public LdrMetadata metadata;
        public string commandString;
        public LdrawCommandType type;
    }

    public static class Parsing
    {
        private static Dictionary<string, string> m_fileCache = new Dictionary<string, string>();

        public static List<Command> GetCommandsFromFile(LdrMetadata metadata, string fullFilePath)
        {
            List<Command> result = new List<Command>();
            if (!System.IO.File.Exists(fullFilePath) )
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
                if (ParseCommand(line, ref metadata, out cmd))
                    commands.Add(cmd); 
            }

            return commands;
        }

        private static bool ParseCommand(string commandString, ref LdrMetadata metadata, out Command cmd)
        {
            // TODO: Actually read the command string.
            // If it contains a meta command, update the metadata.
            // If it contains a subfile command, return a model, component, part or primitive command as necessary.
            // for the created command, we need to propogate the metadata from the parent command.
            // Figure out how to handle the BFC INVERTNEXT command.

            cmd = new Command();
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
    }
}
