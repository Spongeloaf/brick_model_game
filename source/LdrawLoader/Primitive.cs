using System.Collections.Generic;
using Godot;

namespace Ldraw
{
    public static class Primitive
    {
        public static void AddPrimitiveToMesh(MeshManager meshManager, in Command cmd)
        {
            if (meshManager == null)
            {
                Logger.Error("Primitive MeshManager is null");
                return;
            }

            List<Command> commands = Ldraw.Parsing.GetCommandsFromFile(cmd.metadata, cmd.subfileName);
            foreach (Command subCmd in commands)
            {
                switch (cmd.ldrCommandType)
                {
                    case LdrCommandType.subfile:
                        AddPrimitiveToMesh(meshManager, in subCmd);
                        break;

                    case LdrCommandType.triangle:
                        AddTriangleToMesh(meshManager, in subCmd);
                        break;

                    case LdrCommandType.quad:
                        AddQuadToMesh(meshManager, in subCmd);
                        break;

                    default:
                        Logger.Info("Ldraw models should only contain components or primitives as direct children");
                        break;
                }
            }
        }

        private static void AddTriangleToMesh(MeshManager meshMgr, in Command cmd)
        {
            Vector3[] vertices = Parsing.DeserializeTriangle(cmd.commandString);

            if (vertices.Length != 3)
            {
                GD.PrintErr("Triangle command must have 3 vertices");
                return;
            }

            if (cmd.metadata.winding == VertexWinding.CCW)
                FlipTriangleWinding(ref vertices);

            AddTriangleMeshData(meshMgr, in vertices);

            if (cmd.metadata.winding != VertexWinding.Unknown)
                return;

            // If the winding is unknown, duplicate the data and flip the winding
            FlipTriangleWinding(ref vertices);
            AddTriangleMeshData(meshMgr, in vertices);
        }

        private static void FlipTriangleWinding(ref Vector3[] verts)
        {
            Vector3 tmp = verts[0];
            verts[0] = verts[1];
            verts[1] = tmp;
        }

        private static void AddTriangleMeshData(MeshManager mgr, in Vector3[] inputVerts)
        {
            var vertLen = mgr.verts.Count;
            for (int i = 0; i < 3; i++)
            {
                mgr.triangles.Add(vertLen + i);
            }

            for (int i = 0; i < inputVerts.Length; i++)
            {
                mgr.verts.Add(inputVerts[i]);
            }
        }

        private static void AddQuadToMesh(MeshManager meshMgr, in Command cmd)
        {
            Vector3[] vertices = Parsing.DeserializeQuad(cmd.commandString);
            if (vertices.Length != 4)
            {
                GD.PrintErr("Quad command must have 4 vertices");
                return;
            }

            // Why do we need nA and nB, when they're the same value?
            // Was there someting else I was missing? Vertex winding or similar?
            //var nA = Vector3.Cross(v[1] - v[0], v[2] - v[0]);
            //var nB = Vector3.Cross(v[1] - v[0], v[2] - v[0]);

            Vector3[] v = vertices;
            Vector3 tmp = v[1] - v[0];
            Vector3 nA = tmp.Cross(v[2] - v[0]);
            Vector3 nB = tmp.Cross(v[2] - v[0]);

            int vertLen = meshMgr.verts.Count;

            // If the winding is unknown, we assume CCW on the first pass and then do another CW pass.
            if (cmd.metadata.winding == VertexWinding.CCW || cmd.metadata.winding == VertexWinding.Unknown)
            {
                meshMgr.triangles.AddRange(new[]
                {
                vertLen,
                vertLen + 1,
                vertLen + 2,
                vertLen + 2,
                vertLen + 3,
                vertLen
                });
            }
            else
            {
                meshMgr.triangles.AddRange(new[]
                {
                vertLen + 1,
                vertLen,
                vertLen + 2,
                vertLen + 2,
                vertLen,
                vertLen + 3,
                });
            }

            int[] indexes = nA.Dot(nB) < 0 ? new int[] { 0, 1, 3, 2 } : new int[] { 0, 1, 2, 3 };
            for (int i = 0; i < indexes.Length; i++)
            {
                meshMgr.verts.Add(v[indexes[i]]);
            }

            // This is dirty......
            if (cmd.metadata.winding == VertexWinding.Unknown)
            {
                // We are relying on the if statment above to ensure
                // we wind CCW on the first pass, if the winding is unknown.
                Command cmd2 = cmd;
                cmd2.metadata.winding = VertexWinding.CW;
                AddQuadToMesh(meshMgr, in cmd2);
            }
        }
    }
}
