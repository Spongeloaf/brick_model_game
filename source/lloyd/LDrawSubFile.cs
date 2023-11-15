﻿using System;
using System.Collections.Generic;
using Godot;

namespace lloyd
{
	
	public class LDrawSubFile : LDrawCommand
	{
		private string _Name;
		private string _Extension;
		private System.Numerics.Matrix4x4 _Matrix;
		private LDrawModel _Model;
		
		public void GetModelNode(Node3D parent, List<Node> createdNodes, bool inverted)
		{
			_Model.m_inverted = inverted;
            _Model.CreateMeshGameObject(_Matrix, GetMaterial(), parent, createdNodes);
		}

		public void GetPart()
		{

		}

        public override void PrepareMeshData(MeshManager meshMgr)
        {
			 
		}

		public override void Deserialize(string serialized)
		{
            string[] args = serialized.Split(' ');
			float[] param = new float[12];

			_Name = args[14];
			_Name = _Name.Trim();
			_Extension = LDrawConfig.GetExtension(args, 14);
			for (int i = 0; i < param.Length; i++)
			{
				int argNum = i + 2;
				if (!Single.TryParse(args[argNum], out param[i]))
				{
					GD.PrintErr(
						String.Format(
							"Something wrong with parameters in {0} sub-file reference command. ParamNum:{1}, Value:{2}",
							_Name,
							argNum,
							args[argNum]));
				}
			}

			_Model = LDrawModel.Create(_Name, LDrawConfig.Instance.GetSerializedPart(_Name), m_colorCode);
            System.Numerics.Matrix4x4 mat = System.Numerics.Matrix4x4.CreateTranslation(4, 5, 6);
			
			// Packing the martix with transform in the right column
			//_Matrix = new System.Numerics.Matrix4x4(
			//	param[3], param[6], param[9], param[0],
			//	param[4], param[7], param[10], param[1],
			//	param[5], param[8], param[11], param[2],
			//	0, 0, 0,  1
			//);

			// Packing the martix with transform in the bottom row.
            _Matrix = new System.Numerics.Matrix4x4(
                param[3], param[6], param[9], 0,
                param[4], param[7], param[10], 0,
                param[5], param[8], param[11], 0,
                param[0], param[1], param[2], 1
            );
        }

		private Material GetMaterial()
		{
			if(_Extension == ".ldr") 
				return  null;
			
			if (m_colorCode > 0) 
				return LDrawConfig.Instance.GetColoredMaterial(m_colorCode);
			
			if (m_color != null) 
				return LDrawConfig.Instance.GetColoredMaterial(m_color);
			
			return LDrawConfig.Instance.GetColoredMaterial(0);
		}
		
	}

}