using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;

namespace Lloyd
{

    public enum CommandType
    {
        meta = 0,
        SubFile = 1,
        Triangle = 3,
        Quad = 4
    }


    public abstract class LDrawCommand
    {
        protected int m_colorCode = -1;
        protected string m_color;
        protected LDrawModel m_parent;

        protected Vector3[] m_vertices;
        public abstract void PrepareMeshData(List<int> triangles, List<Vector3> verts, VertexWinding winding);
        public abstract void Deserialize(string serialized);
        protected SurfaceTool m_surfaceTool = new SurfaceTool();


        public static LDrawCommand DeserializeCommand(string line, LDrawModel parent, int parentColor)
        {
            LDrawCommand command = null;
            int type;
            var args = line.Split(' ');

            if (args.Length < 2)
                return null;


            if (Int32.TryParse(args[0], out type))
            {
                var commandType = (CommandType)type;

                switch (commandType)
                {
                    case CommandType.SubFile:
                        command = new LDrawSubFile();
                        break;
                    case CommandType.Triangle:
                        command = new LDrawTriangle();
                        break;
                    case CommandType.Quad:
                        command = new LDrawQuad();
                        break;
                    case CommandType.meta:
                        command = new LdrawMetaCommand();
                        break;
                }
            }

            if (command != null)
            {
                if (!int.TryParse(args[1], out command.m_colorCode))
                {
                    command.m_color = args[1];
                }

                if (command.m_colorCode == Constants.kMainColorCode)
                {
                    command.m_colorCode = parentColor;
                }

                command.m_parent = parent;
                command.Deserialize(line);
            }

            return command;
        }
    }
}
