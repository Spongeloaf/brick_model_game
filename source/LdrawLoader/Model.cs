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
        private readonly MeshManager m_meshManager = new MeshManager();
        private ModelTree.ModelTypes m_modelType = ModelTree.ModelTypes.invalid;

        public Model(in Command parentCommand, List<Command> commands)
        {
            m_modelName = parentCommand.subfileName;
            m_fileName = parentCommand.metadata.fileName;

            if (parentCommand.modelType == ModelTree.ModelTypes.invalid)
            {
                OmniLogger.Error("Model type is invalid");
                return;
            }

            foreach (Command cmd in commands)
            {
                switch (cmd.type)
                {
                    case Ldraw.CommandType.SubFile:
                        EmbeddedFile embeddedFile = new EmbeddedFile(cmd.subfileName, cmd.metadata, cmd.transform);
                        m_components.AddRange(embeddedFile.GetComponents());
                        break;

                    case CommandType.Part:
                        Primitive.AddPrimitiveToMesh(m_meshManager, in cmd);
                        break;

                    case CommandType.Model:
                        Primitive.AddPrimitiveToMesh(m_meshManager, in cmd);
                        m_modelType = cmd.modelType;
                        break;

                    default:
                        break;
                }
            }
        }

        public void ConnectModelToOwner(Node3D sceneRoot)
        {
            if (sceneRoot == null)
            {
                OmniLogger.Error("Model scene root is null");
                return;
            }

            Node3D model = new Node3D();
            sceneRoot.AddChild(model);
            model.Owner = sceneRoot;
            model.Name = m_modelName;
            if (m_modelType == ModelTree.ModelTypes.invalid)
                return;

            MeshInstance3D meshInstance = m_meshManager.GetMeshInstance();
            model.AddChild(meshInstance);
            meshInstance.Owner = sceneRoot;

            if (m_components == null || m_components.Count == 0)
                return;

            foreach (Component component in m_components)
                component.ConnectComponent(model, sceneRoot);
        }
    }
}