using Godot;
using System.Linq;
using BrickModelGame.source.pawns.components.turrets;

namespace Ldraw
{
    public static class ModelTree
    {
        public static ModelTypes GetModelTypeFromCommandString(string command)
        {
            if (string.IsNullOrEmpty(command))
                return ModelTypes.invalid;

            string[] tokens = command.Trim().Split();
            if (tokens.Length <= Constants.kSubFileFileName)
                return ModelTypes.invalid;

            if (Anchors.kModelAnchors.ContainsKey(tokens.Last()))
                return Anchors.kModelAnchors[tokens.Last()];

            return ModelTypes.invalid;
        }

        public static bool IsCommandStringAnAnchor(string command)
        {
            return GetModelTypeFromCommandString(command) != ModelTypes.invalid;
        }

        public static Node3D CreateModelNode(LdrawModel ldrawModel, Node3D parent, Node3D sceneRoot)
        {
            Node3D node = BuildModelScene(ldrawModel, parent, sceneRoot);
            node.Name = ldrawModel.m_modelName;
            return node;
        }

        private static Node3D BuildModelScene(LdrawModel model, Node3D parent, Node3D sceneRoot)
        {
            //                      ---- WARNING ----
            // When writing new model factory nodes, please don't forget to call 
            // SetupParents() on the new node, BEFORE you create any children!
            // 
            // The children NEED to be able to find both the scene root and their
            // parent above themselves in the tree, otherwise assigning to 
            // Node.Owner will fail.
            //
            // If you build the whole model tree on its own and want to set the
            // ownership/parentage afterward (which would be cleaner code), you'd
            // have to recursively walk all children doing so. 
            return model.m_modelType switch
            {
                ModelTypes.turret_base => CreateTurret(model, parent, sceneRoot),
                _ => new Node3D(),
            };
        }

        private static void SetupParents(Node3D child, Node3D parent, Node3D sceneRoot)
        {
            parent.AddChild(child);
            child.Owner = sceneRoot;
        }

        private static TurretBase CreateTurret(LdrawModel model, Node3D parent, Node3D sceneRoot)
        {
            TurretBase turretBase = new();
            SetupParents(turretBase, parent, sceneRoot);
            
            Gimbal gimbal = new();
            SetupParents(gimbal, turretBase, sceneRoot);

            Area3D turretBody = new();
            SetupParents(turretBody, gimbal, sceneRoot);

            MeshInstance3D mesh = model.GetMeshManager().GetMeshInstance();
            mesh.Transform *= Transforms.GetScaleAndRotateToGameCoords();
            SetupParents(mesh, turretBody, sceneRoot);

            CollisionShape3D collider = GetMeshBBox(mesh);
            SetupParents(collider, turretBody, sceneRoot);
            return turretBase;
        }

        private static CollisionShape3D GetMeshBBox(MeshInstance3D mesh)
        {
            Aabb meshAabb = mesh.GetAabb();
            BoxShape3D shape = new();
            shape.Size = meshAabb.Size * Transforms.kLdrToGodotScale;
            CollisionShape3D collider = new();
            collider.Shape = shape;
            collider.Position = meshAabb.Position * Transforms.GetScaleAndRotateToGameCoords();
            return collider;

            // Not sure exactly how to fix this. Very likely will involve using the anchor
            // point to offset the collider.
        }
    }
}
