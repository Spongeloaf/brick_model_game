using System;
using System.Collections.Generic;
using System.IO;


namespace Ldraw
{
    public static class FileCache
    {
        static FileCache()
        {
            m_ldrawFileIndex = PrepareFileIndex();
        }

        // m_ldrawFileIndex is a list of part and primitive files in the LDraw library
        private static readonly Dictionary<string, string> m_ldrawFileIndex = new Dictionary<string, string>();
        private static Dictionary<string, string> m_fileCache = new Dictionary<string, string>();

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

            try
            {
                string contents = File.ReadAllText(fullFilePath);
                m_fileCache.Add(fileName, contents);
                return contents;
            }
            catch (System.Exception e)
            {
                OmniLogger.Error("Exception while reading " + fullFilePath + ": " + e.Message);
                return string.Empty;
            }
        }

        private static string GetFullPathToFile(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                OmniLogger.Error("File path is null or empty");
                return string.Empty;
            }

            // These must be user files, so we use the full path
            if (Parsing.IsLdrOrMpdFile(path))
                return path;

            if (m_ldrawFileIndex.ContainsKey(path))
                return m_ldrawFileIndex[path];

            OmniLogger.Error($"File {path} does not exist in the file index");
            return string.Empty;
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
                    OmniLogger.Error("Ldraw file index error: Duplicate part " + fileName);
            }

            return parts;
        }

        public static void CacheFileContents(string fileName, string contents)
        {
            if (m_fileCache.ContainsKey(fileName))
            {
                OmniLogger.Error($"File {fileName} already exists in the Ldraw file cache!");
                return;
            }

            m_fileCache.Add(fileName, contents);
        }
    }
}
