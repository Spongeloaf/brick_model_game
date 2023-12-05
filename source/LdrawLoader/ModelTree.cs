﻿using Godot.Collections;
using System;
using System.Linq;

namespace Ldraw
{
    public static class ModelTree
    {
        public enum ModelTypes
        {
            invalid,
            prop,
            minifig,
        }

        public enum ComponentTypes
        {
            invalid,
            decoration,
        }

        private static readonly Dictionary<string, ModelTypes> kModelAnchors = new Dictionary<string, ModelTypes>()
        {
            { "mwg_m_prop_anchor.dat", ModelTypes.prop },
            { "mwg_m_minifig_anchor.dat", ModelTypes.minifig },
        };

        private static readonly Dictionary<string, ComponentTypes> kComponentAnchors = new Dictionary<string, ComponentTypes>()
        {
            { "mwg_c_decoration_anchor.dat", ComponentTypes.decoration },
        };

        public static ModelTypes GetModelTypeFromCommandString(string command)
        {
            if (string.IsNullOrEmpty(command))
                return ModelTypes.invalid;

            string[] tokens = command.Trim().Split();
            if (tokens.Length <= Constants.kSubFileFileName)
                return ModelTypes.invalid;

            if (kModelAnchors.ContainsKey(tokens.Last()))
                return kModelAnchors[tokens.Last()];

            return ModelTypes.invalid;
        }

        public static bool IsCommandStringAnAnchor(string command)
        {
            return GetModelTypeFromCommandString(command) != ModelTypes.invalid;
        }

        public static ComponentTypes GetComponentTypeFromCommandString(string command)
        {
            if (string.IsNullOrEmpty(command))
                return ComponentTypes.invalid;

            string[] tokens = command.Trim().Split();
            if (tokens.Length <= Constants.kSubFileFileName)
                return ComponentTypes.invalid;

            if (kComponentAnchors.ContainsKey(tokens.Last()))
                return kComponentAnchors[tokens.Last()];

            return ComponentTypes.invalid;
        }
    }
}
