using System.Collections.Generic;
using Godot;

namespace Ldraw
{
    public static class Primitive
    {
        public static void AddPrimitiveToMesh(MeshManager meshManager, in Command command)
        {
            if (meshManager == null)
            {
                OmniLogger.Error("Primitive MeshManager is null");
                return;
            }

            List<Command> commands = Ldraw.Parsing.GetCommandsFromFile(command.metadata, command.subfileName);
            foreach (Command subCmd in commands)
            {
                switch (subCmd.ldrCommandType)
                {
                    case LdrCommandType.subfile:
                        AddPrimitiveToMesh(meshManager, in subCmd);
                        break;

                    case LdrCommandType.triangle:
                        // Use the parent transform because primitive commands do not have their own.
                        meshManager.AddTriangle(in subCmd, in command.transform);
                        break;

                    case LdrCommandType.quad:
                        // Use the parent transform because primitive commands do not have their own.
                        meshManager.AddQuad(in subCmd, command.transform);
                        break;

                    default:
                        OmniLogger.Info("Ldraw models should only contain components or primitives as direct children");
                        break;
                }
            }
        }
    }
}
