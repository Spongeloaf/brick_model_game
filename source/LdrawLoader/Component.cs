using System.Collections.Generic;

namespace Ldraw
{
    public class Component
    {
        public enum Type
        {
            prop,
            body,
            head,
            arm,
            leg,
        }

        public readonly List<Component> m_components;
        public readonly string m_modelName;
        public readonly string m_fileName;
        public MeshManager m_meshManager;
        public readonly Type m_type;

        public Component(Command modelCommand)
        {
            m_modelName = modelCommand.metadata.modelName;
            m_fileName = modelCommand.subfileName;

            List<Command> commands = Ldraw.Parsing.GetCommandsFromFile(modelCommand.metadata, m_fileName);
            foreach (Command cmd in commands)
            {
                switch (cmd.type)
                {
                    case Ldraw.GameEntityType.Component:
                        m_components.Add(new Component(modelCommand));
                        break;
                    case GameEntityType.Primitive:
                        CreateMesh(m_meshManager, cmd);
                        break;
                    default:
                        Logger.Info("Ldraw models should only contain components or primitives as direct children");
                        break;
                }
            }
        }

        private static void CreateMesh(MeshManager meshMgr, in Command cmd)
        {
            throw new System.NotImplementedException();
        }
    }
}
