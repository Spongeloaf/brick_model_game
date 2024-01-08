using Godot;
using Ldraw;
using System.Collections.Generic;

namespace Ldraw
{
    public class Model
    {
        private readonly string m_modelName;
        private readonly List<Model> m_children = new List<Model>();
        protected readonly MeshManager m_meshManager = new MeshManager();

        private Transform3D m_subfileTransform = Transform3D.Identity;
        private Transform3D m_anchorTransform = Transform3D.Identity;
        private ModelTypes m_modelType = ModelTypes.invalid;
        
        
        public Model(List<Command> commands, in Transform3D subfileTransform)
        {
            // Models need the list of commands because we don't know what we're parsing until
            // we stumble upon a model anchor, then we need to treat the list that contains
            // the anchor as a model.

            m_subfileTransform = subfileTransform;
            foreach (Command cmd in commands)
            {
                switch (cmd.type)
                {
                    case CommandType.Model:
                        m_anchorTransform = cmd.transform;
                        m_modelName = cmd.subfileName;
                        m_modelType = cmd.modelType;

                        // The offset ensures that the mesh is always organized around the anchor position.
                        // Remember that the parent command that spawned this model was the anchor, so this
                        // transform is its position within the submodel.
                        m_meshManager.SetOffset(m_anchorTransform.Origin);
                        break;

                    case CommandType.Subfile:
                        EmbeddedFile embeddedFile = new EmbeddedFile(cmd.subfileName, cmd.metadata, cmd.transform);
                        Model newModel = embeddedFile.GetModel();
                        if (newModel != null)
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
            MeshInstance3D meshInstance = m_meshManager.GetMeshInstance();
            model.AddChild(meshInstance);
            meshInstance.Owner = sceneRoot;
            meshInstance.Transform *= Transforms.GetScaleAndRotateToGameCoords();
            Transform3D offset = m_anchorTransform;

            if (m_children == null || m_children.Count == 0)
                return;

            foreach (Model child in m_children)
                child.ConnectChild(model, sceneRoot, offset);
        }

        protected void ConnectChild(Node3D parent, Node3D sceneRoot, Transform3D parentAnchorPosition)
        {
            if (sceneRoot == null || parent == null)
            {
                OmniLogger.Error("Model scene root or parent is null");
                return;
            }

            // The transform stuff here is a bit fucky, so bear with me.
            //
            // We always want the models origin point (and by extension, the center
            // of rotation) to be where the user puts the anchor.
            //
            // Basically, the LDRAW models all have their own origin point that is not
            // related in any way whatsoever to our anchor point. The user could make 
            // a model with the anchor at 0,0,0, or 9001,420,69, but if the parts are
            // in the same positions relative to the anchor, it should look exactly the
            // same in both cases.
            //
            // We use a simple Node3D to hold the mesh like a container, and build the
            // model tree out of those containers. The mesh objects are children of the
            // containers, and their transform converts them from LDraw coords to Godot
            // coords.
            //
            // To achieve this, we need to compose a position for the container node made
            // by taking the anchor point in submodel space, mutated by the subfile
            // transform.
            // 
            // Please be very careful when messing with the transforms. A slight change
            // could break everything badly. But I could also break only complex cases,
            // and you may not notice on simple models.
            MeshInstance3D meshInstance = m_meshManager.GetMeshInstance();
            meshInstance.Transform *= Transforms.GetScaleAndRotateToGameCoords();
            
            Node3D container = CreateNode3D(parent, sceneRoot, "container");
            container.AddChild(meshInstance);
            meshInstance.Owner = sceneRoot;
            
            Transform3D offset = m_subfileTransform * m_anchorTransform;
            Vector3 containerOrigin = offset.Origin - parentAnchorPosition.Origin;
            container.Transform = m_subfileTransform;
            container.Position = containerOrigin * Transforms.GetScaleAndRotateToGameCoords();
            container.Transform = Transforms.GetCorrectedRotation(container.Transform);

            foreach (Model component in m_children)
                component.ConnectChild(container, sceneRoot, m_anchorTransform);
        }
    }
}