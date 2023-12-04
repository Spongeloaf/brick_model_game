using System.Collections.Generic;
using Godot;

namespace Ldraw
{
    public class Component
    {

        public readonly string m_modelName;
        public readonly string m_fileName;
        public readonly List<Component> m_childComponents = new List<Component>();
        public Transform3D m_transform3D = new Transform3D();
        public MeshManager m_meshManager = new MeshManager();

        public Component(Command parentCommand)
        {
            m_modelName = parentCommand.metadata.modelName;
            m_fileName = parentCommand.subfileName;
            m_transform3D = parentCommand.transform;

            List<Command> commands = Ldraw.Parsing.GetCommandsFromFile(parentCommand);
            foreach (Command cmd in commands)
            {
                switch (cmd.type)
                {
                    case Ldraw.CommandType.Component:
                        // This prevents infinite recursion because components look for other
                        // component anchors when reading files.
                        if (cmd.componentType != parentCommand.componentType)
                            m_childComponents.Add(new Component(cmd));
                        break;

                    case CommandType.Part:
                    case CommandType.Triangle:
                    case CommandType.Quad:
                        Primitive.AddPrimitiveToMesh(m_meshManager, in cmd);
                        break;

                    default:
                        OmniLogger.Info("Ldraw components should only contain components or primitives as direct children");
                        break;
                }
            }
        }

        public void ConnectComponent(Node3D parent, Node3D sceneRoot)
        {
            if (sceneRoot == null || parent == null)
            {
                OmniLogger.Error("Model scene root or parent is null");
                return;
            }

            Node3D thisComponent = new Node3D();
            parent.AddChild(thisComponent);
            thisComponent.Owner = sceneRoot;
            thisComponent.Name = m_modelName;
            thisComponent.Transform = m_transform3D;

            MeshInstance3D meshInstance = m_meshManager.GetMeshInstance();
            thisComponent.AddChild(meshInstance);
            meshInstance.Owner = sceneRoot;

            foreach (Component component in m_childComponents)
                component.ConnectComponent(thisComponent, sceneRoot);
        }
    }
}
