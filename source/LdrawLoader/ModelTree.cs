using Godot.Collections;
using System;
using System.Linq;

namespace Ldraw
{
    public static class ModelTree
    {
        public static ModelTypes GetModelTypeFromCommandString(string command)
        {
            if (string.IsNullOrEmpty(command))
                return ModelTypes.invalid;

            string[] tokens = command.Trim().Split();
            if (tokens.Length <= Constants.kSubFileFileName)
                return ModelTypes.invalid;

            if (Anchors.kModelAnchors.ContainsKey(tokens.Last()))
                return Anchors.kModelAnchors[tokens.Last()];

            return ModelTypes.invalid;
        }

        public static bool IsCommandStringAnAnchor(string command)
        {
            return GetModelTypeFromCommandString(command) != ModelTypes.invalid;
        }
    }
}
