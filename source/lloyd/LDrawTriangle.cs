using System;
using System.Collections.Generic;
using Godot;

namespace Lloyd
{
	public class LDrawTriangle : LDrawCommand
	{
        public override void PrepareMeshData(List<int> triangles, List<Vector3> verts, VertexWinding winding)
        {
			if (m_vertices.Length != 3)
			{
				GD.PrintErr("Triangle command must have 3 vertices");
				return;
			}

			if (winding == VertexWinding.CCW)
			{
				Vector3 tmp = m_vertices[0];
				m_vertices[0] = m_vertices[1];
				m_vertices[1] = tmp;
			}
            
			var vertLen = verts.Count;
			for (int i = 0; i < 3; i++)
			{
				triangles.Add(vertLen + i);
			}

			for (int i = 0; i < m_vertices.Length; i++)
			{
				verts.Add(m_vertices[i]);
			}

		}

		public override void Deserialize(string serialized)
		{
			var args = serialized.Split(' ');
			float[] param = new float[9];
			for (int i = 0; i < param.Length; i++)
			{
				int argNum = i + 2;
				if (!Single.TryParse(args[argNum], out param[i]))
				{
					GD.PrintErr(
						String.Format(
							"Something wrong with parameters in line drawn command. ParamNum:{0}, Value:{1}",
							argNum,
							args[argNum]));
				}
			}

			m_vertices = new Vector3[]
			{
				new Vector3(param[0], param[1], param[2]),
				new Vector3(param[3], param[4], param[5]),
				new Vector3(param[6], param[7], param[8])
			};
		}

	}
	
}
