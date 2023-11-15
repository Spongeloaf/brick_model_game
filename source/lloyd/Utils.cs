using System.IO;
using lloyd;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace lloyd
{
    public static class Utils
    {
        public static List<LDrawCommand> ReadCommandList(string serializedCommands, LDrawModel model, int mainColor)
        {
            List<LDrawCommand> commands = new List<LDrawCommand>();
            using (StringReader reader = new StringReader(serializedCommands))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Regex regex = new Regex("[ ]{2,}", RegexOptions.None);
                    line = regex.Replace(line, " ").Trim();

                    if (line == null || line == "")
                        continue;

                    LDrawCommand command = LDrawCommand.DeserializeCommand(line, mainColor);
                    if (command != null)
                        commands.Add(command);

                }
            }

            return commands;
        }
    }
}
