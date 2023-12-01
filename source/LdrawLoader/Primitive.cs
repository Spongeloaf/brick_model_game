using System.Collections.Generic;
using Godot;

namespace Ldraw
{
    public static class Primitive
    {
        public static void AddPrimitiveToMesh(MeshManager meshManager, in Command parentCommand)
        {
            if (meshManager == null)
            {
                OmniLogger.Error("Primitive MeshManager is null");
                return;
            }

            // Do not render anchors, they're only used for logical operations
            if (ModelTree.IsCommandStringAnAnchor(parentCommand.commandString))
                return;

            List<Command> subCommands = Ldraw.Parsing.GetCommandsFromFile(in parentCommand);
            foreach (Command subCmd in subCommands)
            {
                switch (subCmd.type)
                {
                    case GameEntityType.Part:
                        AddPrimitiveToMesh(meshManager, in subCmd);
                        break;

                    case GameEntityType.Triangle:
                        meshManager.AddTriangle(in subCmd);
                        break;

                    case GameEntityType.Quad:
                        meshManager.AddQuad(in subCmd);
                        break;

                    default:
                        OmniLogger.Info("Ldraw primitievs should only contain parts, triangles, or quads as direct children");
                        break;
                }
            }
        }
    }
}
