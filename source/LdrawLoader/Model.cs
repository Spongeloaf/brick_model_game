using Godot;
using Ldraw;
using System.Collections.Generic;

namespace Ldraw
{
    public class Model
    {
        private readonly string m_modelName;
        private readonly List<Model> m_children = new List<Model>();
        private readonly MeshManager m_meshManager = new MeshManager();

        private ModelTypes m_modelType = ModelTypes.invalid;
        public Model(in Command parentCommand, List<Command> commands)
        {
            // Models need the list of commands because we don't know what we're parsing until
            // we stumble upon a model anchor, then we need to treat the list that contains
            // the anchor as a model.

            // The offset ensures that the mesh is always orginized around the anchor position.
            // Remember that the parent command that spawned this model was the anchor, so this
            // transform is it's position within the submodel.
            m_meshManager.SetOffset(parentCommand.transform.Origin);
            m_modelName = parentCommand.subfileName;
            m_modelType = parentCommand.modelType;

            if (parentCommand.modelType == ModelTypes.invalid)
            {
                OmniLogger.Error("Model type is invalid");
                return;
            }

            foreach (Command cmd in commands)
            {
                switch (cmd.type)
                {
                    case CommandType.Subfile:
                        EmbeddedFile embeddedFile = new EmbeddedFile(cmd.subfileName, cmd.metadata);
                        Model newModel = embeddedFile.GetModel();
                        if (newModel == null)
                            continue;

                        m_children.Add(newModel);
                        break;

                    case CommandType.Triangle:
                        m_meshManager.AddTriangle(in cmd);
                        break;

                    case CommandType.Quad:
                        m_meshManager.AddQuad(in cmd);
                        break;

                    case CommandType.Part:
                        Primitive.AddPrimitiveToMesh(m_meshManager, in cmd);
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

            if (m_modelType == ModelTypes.invalid)
                return;

            Node3D model = new Node3D();
            sceneRoot.AddChild(model);
            model.Owner = sceneRoot;
            model.Name = m_modelName;
            if (m_modelType == ModelTypes.invalid)
                return;

            MeshInstance3D meshInstance = m_meshManager.GetMeshInstance();
            model.AddChild(meshInstance);
            meshInstance.Owner = sceneRoot;

            if (m_children == null || m_children.Count == 0)
                return;

            foreach (Model child in m_children)
                child.ConnectChild(model, sceneRoot);
        }

        protected void ConnectChild(Node3D parent, Node3D sceneRoot)
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

            MeshInstance3D meshInstance = m_meshManager.GetMeshInstance();
            thisComponent.AddChild(meshInstance);
            meshInstance.Owner = sceneRoot;

            foreach (Model component in m_children)
                component.ConnectChild(thisComponent, sceneRoot);
        }
    }
}