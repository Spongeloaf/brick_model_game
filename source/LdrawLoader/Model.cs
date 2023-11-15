﻿using Godot;
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
            Command[] commands = Parsing.GetCommandsFromFile(metadata, fullFilePath);
            if (commands == null || commands.Length == 0)
                return null;

            List<Model> nodes = new List<Model>();
            foreach (Command command in commands)
            {
                if (command.type == LdrawCommandType.Model)
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


        public Model(Ldraw.Command modelCommand)
        {
            m_modelName = modelCommand.metadata.name;
            m_fileName = Parsing.GetSubFileName(modelCommand.commandString);

            Command[] commands = Ldraw.Parsing.GetCommandsFromFile(modelCommand.metadata, m_fileName);
            foreach (Command cmd in commands)
            {
                switch (cmd.type)
                {
                    case Ldraw.LdrawCommandType.Component:
                        m_components.Add(new Component(modelCommand));
                        break;
                    default:
                        Logger.Info("Ldraw models should only contain components as direct children, not primitives or parts.");
                        break;
                }
            }
        }
    }
}