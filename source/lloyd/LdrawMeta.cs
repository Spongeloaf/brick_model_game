using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


namespace lloyd
{
    public enum MetaCommands
    {
        BFC,
        Unsupported,
    }

    public class LdrawMetaCommand : LDrawCommand
    {
        public bool m_BfcCertified = false;
        public bool m_invertNext = false;
        public MetaCommands m_command = MetaCommands.Unsupported;
        private readonly string kCW = "CW";
        private readonly string kCCW = "CCW";
        private readonly string kCertify = "CERTIFY";
        private readonly string kNoCertify = "NOCERTIFY";
        private readonly string kInvertNext = "INVERTNEXT";
        

        public override void Deserialize(string serialized)
        {
            string[] tokens = serialized.Split(' ');
            if (tokens.Length < 2)
            {
                GD.PrintErr($"Malformed meta command '{serialized}'");
                return;
            }

            if (tokens[1] == Constants.kBFC)
                DoBfc(tokens);
        }

        public override void PrepareMeshData(MeshManager meshMgr)
        {

        }

        private void DoBfc(string[] tokens)
        {
            // Refernce: https://www.ldraw.org/article/415.html
            m_command = MetaCommands.BFC;
            m_BfcCertified = false;

            // Winding is implied CCW if not accompanied by explicit CCW or CW token
            if (tokens.Contains(kCertify))
            {
                m_winding = VertexWinding.CCW;  
                m_BfcCertified = true;
            }

            // However, we can also assume the file is certified if it contains any BFC statement
            // that eEXCEPT for "0 BFC NOCERTIFY".

            if (tokens.Contains(kCW))
            {
                m_winding = VertexWinding.CW;
                m_BfcCertified = true;
            }

            if (tokens.Contains(kCCW))
            {
                m_winding = VertexWinding.CCW;
                m_BfcCertified = true;
            }

            // There is a potential bug here, where we don't bother to check a NOCERTIFY or CERTIFY
            // BFC command is the first BFC command in the file, as ordained by the spec. However, any
            // file exhibiting such behavior is malformed, and undefined behavior is expected in this
            // case.
            if (tokens.Contains(kNoCertify))
            {
                m_winding = VertexWinding.Unknown;
                m_BfcCertified = false;
            }

            if (tokens.Contains(kInvertNext))
                m_invertNext = true;
        }
    }
}
