using System.Collections.Generic;
using Godot;

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

        public readonly List<Component> m_childComponents = new List<Component>();
        public readonly string m_modelName;
        public readonly string m_fileName;
        public MeshManager m_meshManager = new MeshManager();
        public readonly Type m_type;

        public Component(Command modelCommand)
        {
            m_modelName = modelCommand.metadata.modelName;
            m_fileName = modelCommand.subfileName;

            List<Command> commands = Ldraw.Parsing.GetCommandsFromFile(modelCommand);
            foreach (Command cmd in commands)
            {
                switch (cmd.type)
                {
                    case Ldraw.GameEntityType.Component:
                        m_childComponents.Add(new Component(modelCommand));
                        break;
                    case GameEntityType.Primitive:
                        Primitive.AddPrimitiveToMesh(m_meshManager, in cmd);
                        m_meshManager.BuildMesh();
                        break;
                    default:
                        OmniLogger.Info("Ldraw models should only contain components or primitives as direct children");
                        break;
                }
            }
        }

        public Node3D GetComponentInstance()
        {
            Node3D component = new Node3D();
            component.Name = m_modelName;
            component.AddChild(m_meshManager.GetMeshInstance());

            if (m_childComponents == null || m_childComponents.Count == 0)
                return component;

            foreach (Component child in m_childComponents)
            {
                Node3D componentNode = child.GetComponentInstance();
                componentNode.Owner = component;
            }
            return component;
        }
    }
}
