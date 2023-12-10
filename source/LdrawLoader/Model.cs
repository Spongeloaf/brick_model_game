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
        private Transform3D m_transform3D = Transform3D.Identity;

        public Model(in Command parentCommand, List<Command> commands)
        {
            // Models need the list of commands because we don't know what we're parsing until
            // we stumble upon a model anchor, then we need to treat the list that contains
            // the anchor as a model.

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

                        newModel.m_transform3D = cmd.transform;
                        m_children.Add(newModel);
                        break;

                    case CommandType.Part:
                    case CommandType.Triangle:
                    case CommandType.Quad:
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

            if (m_modelType != ModelTypes.invalid)
            {
                OmniLogger.Error("Model and component types cannot both be invalid!");
                return;
            }

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

            Vector3 childPosition = m_transform3D.Origin * m_meshManager.m_ScaleToGameCoords.Basis * m_meshManager.m_RotateToGameOrientation.Basis;
            Node3D thisComponent = new Node3D();
            parent.AddChild(thisComponent);
            thisComponent.Owner = sceneRoot;
            thisComponent.Name = m_modelName;
            thisComponent.Transform = thisComponent.Transform.TranslatedLocal(childPosition);

            MeshInstance3D meshInstance = m_meshManager.GetMeshInstance();
            thisComponent.AddChild(meshInstance);
            meshInstance.Owner = sceneRoot;

            foreach (Model component in m_children)
                component.ConnectChild(thisComponent, sceneRoot);
        }
    }
}