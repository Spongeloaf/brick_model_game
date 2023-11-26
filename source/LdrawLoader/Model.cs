using Godot;
using Ldraw;
using System.Collections.Generic;

namespace Ldraw
{
    public class Model
    {
        private readonly string m_modelName;
        private readonly string m_fileName;
        private readonly List<Component> m_components = new List<Component>();


        public Model(Command modelCommand)
        {
            m_modelName = modelCommand.metadata.modelName;
            m_fileName = modelCommand.metadata.fileName;

            if (modelCommand.modelType == ModelTree.ModelTypes.invalid)
            {
                OmniLogger.Error("Model type is invalid");
                return;
            }

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

        public Node3D GetModelInstance()
        {
            Node3D model = new Node3D();
            model.Name = m_modelName;

            if (m_components == null || m_components.Count == 0)
                return model;

            foreach (Component component in m_components)
            {
                Node3D componentNode = component.GetComponentInstance();
                componentNode.Owner = model;
            }
            return model;
        }
    }
}