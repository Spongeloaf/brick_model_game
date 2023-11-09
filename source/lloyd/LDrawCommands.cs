﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Godot;

namespace Lloyd
{

    public enum CommandType
    {
        SubFile = 1,
        Triangle = 3,
        Quad = 4
    }


    public abstract class LDrawCommand
    {
        protected int _ColorCode = -1;
        protected string _Color;
        protected LDrawModel _Parent;

        public static LDrawCommand DeserializeCommand(string line, LDrawModel parent, int parentColor)
        {
            LDrawCommand command = null;
            int type;
            var args = line.Split(' ');
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
                }
            }
            else
            {
                GD.Print("Skipped line : " + line);
            }

            if (command != null)
            {
                if (!int.TryParse(args[1], out command._ColorCode))
                {
                    command._Color = args[1];
                }

                if (command._ColorCode == Constants.kMainColorCode)
                {
                    command._ColorCode = parentColor;
                }

                command._Parent = parent;
                command.Deserialize(line);
            }

            return command;
        }

        protected Vector3[] _Verts;
        public abstract void PrepareMeshData(List<int> triangles, List<Vector3> verts);
        public abstract void Deserialize(string serialized);

    }
}
