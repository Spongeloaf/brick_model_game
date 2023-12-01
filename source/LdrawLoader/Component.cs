using System.Collections.Generic;
using Godot;

namespace Ldraw
{
    public class Component
    {

        public readonly List<Component> m_childComponents = new List<Component>();
        public readonly string m_modelName;
        public readonly string m_fileName;
        public MeshManager m_meshManager = new MeshManager();

        public Component(Command parentCommand)
        {
            m_modelName = parentCommand.metadata.modelName;
            m_fileName = parentCommand.subfileName;

            List<Command> commands = Ldraw.Parsing.GetCommandsFromFile(parentCommand);
            foreach (Command cmd in commands)
            {
                switch (cmd.type)
                {
                    case Ldraw.GameEntityType.Component:
                        // This prevents infinite recursion because components look for other
                        // component anchors when reading files.
                        if (cmd.componentType != parentCommand.componentType)
                            m_childComponents.Add(new Component(parentCommand));
                        break;

                    case GameEntityType.Part:
                    case GameEntityType.Triangle:
                    case GameEntityType.Quad:
                        Primitive.AddPrimitiveToMesh(m_meshManager, in cmd);
                        break;

                    default:
                        OmniLogger.Info("Ldraw components should only contain components or primitives as direct children");
                        break;
                }
            }
        }

        public void ConnectComponentInstance(Node3D owner)
        {
            Node3D thisComponent = new Node3D();
            thisComponent.Name = m_modelName;
            owner.AddChild(thisComponent);
            thisComponent.Owner = owner;

            MeshInstance3D meshInstance = m_meshManager.GetMeshInstance();
            thisComponent.AddChild(meshInstance);
            meshInstance.Owner = owner;

            if (m_childComponents == null || m_childComponents.Count == 0)
                return;

            foreach (Component component in m_childComponents)
                component.ConnectComponentInstance(owner);
        }
    }
}
