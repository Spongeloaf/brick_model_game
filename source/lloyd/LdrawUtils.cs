

using System;

namespace Lloyd
{
    public static class Constants
    {
        public static readonly string kBFC = "BFC";
        public static readonly int kMainColorCode = 16;
    }

    public enum VertexWinding
    {
        CCW,
        CW, 
        Unknown,
    }

    public struct BFC
    {
        public VertexWinding winding;
        public bool INVERTNEXT;
        public bool CERTIFY;
    }

    public static class Utils
    {
        
    }
}
