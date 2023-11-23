using Godot;
using Ldraw;
using System.Collections.Generic;

namespace Ldraw
{
    public static class ModelManager
    {
        public static List<Model> LoadModelsFromFile(string fullFilePath)
        {
            // We can just make default data here, because we'll be overwriting it anyway inside GetCommandsFromFile().
            LdrMetadata metadata = new LdrMetadata();
            Command cmd = new Command();
            cmd.metadata = metadata;
            List<Command> commands = Parsing.GetCommandsFromFile(cmd);
            if (commands == null || commands.Count == 0)
                return null;

            List<Model> nodes = new List<Model>();
            foreach (Command command in commands)
            {
                if (command.type == GameEntityType.Model)
                    nodes.Add(new Model(command));
            }

            return nodes;
        }
    }

    public class Model
    {
        public readonly string m_modelName;
        public readonly string m_fileName;
        public readonly List<Component> m_components;


        public Model(Command modelCommand)
        {
            m_modelName = modelCommand.metadata.modelName;
            m_fileName = modelCommand.subfileName;

            List<Command> commands = Ldraw.Parsing.GetCommandsFromFile(modelCommand);
            foreach (Command cmd in commands)
            {
                switch (cmd.type)
                {
                    case Ldraw.GameEntityType.Component:
                        m_components.Add(new Component(modelCommand));
                        break;
                    default:
                        OmniLogger.Info("Ldraw models should only contain components as direct children, not primitives or parts.");
                        break;
                }
            }
        }
    }
}