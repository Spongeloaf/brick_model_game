﻿using Godot;
using Ldraw;
using System.Collections.Generic;

namespace Ldraw
{
    public class Model
    {
        private readonly string m_modelName;
        private readonly List<Model> m_children = new List<Model>();
        private readonly MeshManager m_meshManager = new MeshManager();

        private Transform3D m_subfileTransform = Transform3D.Identity;
        private Transform3D m_anchorTransform = Transform3D.Identity;
        private ModelTypes m_modelType = ModelTypes.invalid;
        public Model(in Command parentCommand, List<Command> commands, in Transform3D subfileTransform)
        {
            // Models need the list of commands because we don't know what we're parsing until
            // we stumble upon a model anchor, then we need to treat the list that contains
            // the anchor as a model.

            // The offset ensures that the mesh is always orginized around the anchor position.
            // Remember that the parent command that spawned this model was the anchor, so this
            // transform is it's position within the submodel.
            m_anchorTransform = parentCommand.transform;
            m_subfileTransform = subfileTransform;
            m_meshManager.SetOffset(m_anchorTransform.Origin);

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
                        EmbeddedFile embeddedFile = new EmbeddedFile(cmd.subfileName, cmd.metadata, cmd.transform);
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

        public static Node3D CreateNode3D(Node3D parent, Node3D sceeneRoot, string name)
        {
            Node3D node = new Node3D();
            parent.AddChild(node);
            node.Owner = sceeneRoot;
            node.Name = name;
            return node;
        }

        public void CreateModel(Node3D sceneRoot)
        {
            if (sceneRoot == null)
            {
                OmniLogger.Error("Model scene root is null");
                return;
            }

            if (m_modelType == ModelTypes.invalid)
                return;

            Node3D model = CreateNode3D(sceneRoot, sceneRoot, m_modelName);

            // TODO: Make the transformer have a local position offset so the
            // model is centered around the anchor.
            Node3D transformer = CreateNode3D(model, sceneRoot, "gamespaceAdapter");
            transformer.Transform = Transforms.GetScaleAndRotateToGameCoords();

            MeshInstance3D meshInstance = m_meshManager.GetMeshInstance();
            transformer.AddChild(meshInstance);
            meshInstance.Owner = sceneRoot;

            Transform3D offset = m_anchorTransform;

            if (m_children == null || m_children.Count == 0)
                return;

            foreach (Model child in m_children)
                child.ConnectChild(meshInstance, sceneRoot, offset);
        }

        protected void ConnectChild(Node3D parent, Node3D sceneRoot, Transform3D parentAnchorPosition)
        {
            if (sceneRoot == null || parent == null)
            {
                OmniLogger.Error("Model scene root or parent is null");
                return;
            }

            Transform3D childOrigin = m_subfileTransform * m_anchorTransform;
            MeshInstance3D meshInstance = m_meshManager.GetMeshInstance();
            parent.AddChild(meshInstance);
            meshInstance.Owner = sceneRoot;
            meshInstance.Transform = m_subfileTransform;
            meshInstance.Position = childOrigin.Origin - parentAnchorPosition.Origin;

            foreach (Model component in m_children)
                component.ConnectChild(meshInstance, sceneRoot, meshInstance.Transform);
        }
    }
}