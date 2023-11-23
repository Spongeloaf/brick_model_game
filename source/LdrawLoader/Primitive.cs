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

            if ( parentCommand.subfileName == "stud2a.dat")
            {
                int i = 0;
            }

            List<Command> subCommands = Ldraw.Parsing.GetCommandsFromFile(in parentCommand);
            foreach (Command subCmd in subCommands)
            {
                switch (subCmd.ldrCommandType)
                {
                    case LdrCommandType.subfile:
                        AddPrimitiveToMesh(meshManager, in subCmd);
                        break;

                    case LdrCommandType.triangle:
                        // Use the parent transform because primitive commands do not have their own.
                        meshManager.AddTriangle(in subCmd);
                        break;

                    case LdrCommandType.quad:
                        // Use the parent transform because primitive commands do not have their own.
                        meshManager.AddQuad(in subCmd);
                        break;

                    default:
                        OmniLogger.Info("Ldraw models should only contain components or primitives as direct children");
                        break;
                }
            }
        }
    }
}
