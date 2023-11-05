﻿using System;
using System.Collections;
using System.Collections.Generic;
using Godot;

namespace LDraw
{
    public class LDrawQuad : LDrawCommand
    {
        public override void PrepareMeshData(List<int> triangles, List<Vector3> verts)
        {
            //var nA = Vector3.Cross(v[1] - v[0], v[2] - v[0]);
            //var nB = Vector3.Cross(v[1] - v[0], v[2] - v[0]);

            Vector3[] v = _Verts;
            // Why do we need nA and nB, when they're the same value?
            Vector3 tmp = v[1] - v[0];
            Vector3 nA = tmp.Cross(v[2] - v[0]);
            Vector3 nB = tmp.Cross(v[2] - v[0]);

            int vertLen = verts.Count;
            triangles.AddRange(new[]
            {
                vertLen + 1,
                vertLen + 2,
                vertLen,
                vertLen + 1,
                vertLen + 3,
                vertLen + 2
            });

            int[] indexes = nA.Dot(nB) > 0 ? new int[] { 0, 1, 3, 2 } : new int[] { 0, 1, 2, 3 };
            for (int i = 0; i < indexes.Length; i++)
            {
                Vector3 bob = v[indexes[i]];
                verts.Add(bob);
                //verts.Add(v[indexes[i]]);
            }
        }

        public override void Deserialize(string serialized)
        {
            string[] args = serialized.Split(' ');
            float[] param = new float[12];
            for (int i = 0; i < param.Length; i++)
            {
                int argNum = i + 2;
                if (!float.TryParse(args[argNum], out param[i]))
                {
                    GD.PrintErr(
                        String.Format(
                            "Something wrong with parameters in line drawn command. ParamNum:{0}, Value:{1}",
                            argNum,
                            args[argNum]));
                }
            }

            _Verts = new Vector3[]
            {
                new Vector3(param[0], param[1], param[2]),
                new Vector3(param[3], param[4], param[5]),
                new Vector3(param[6], param[7], param[8]),
                new Vector3(param[9], param[10], param[11])
            };
        }

    }

}
