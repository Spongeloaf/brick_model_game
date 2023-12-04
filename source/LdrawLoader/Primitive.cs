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

            List<Command> commands = Ldraw.Parsing.GetCommandsFromFile(in parentCommand);
            foreach (Command cmd in commands)
            {
                switch (cmd.type)
                {
                    case CommandType.Part:
                        AddPrimitiveToMesh(meshManager, in cmd);
                        break;

                    case CommandType.Triangle:
                        meshManager.AddTriangle(in cmd);
                        break;

                    case CommandType.Quad:
                        meshManager.AddQuad(in cmd);
                        break;

                    default:
                        OmniLogger.Info("Ldraw primitievs should only contain parts, triangles, or quads as direct children");
                        break;
                }
            }
        }
    }
}
