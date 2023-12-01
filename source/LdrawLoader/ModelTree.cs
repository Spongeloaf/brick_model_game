using Godot.Collections;
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
            body,
        }

        private static readonly Dictionary<string, ModelTypes> kModelAnchors = new Dictionary<string, ModelTypes>()
        {
            { "prop_anchor.dat", ModelTypes.prop },
            { "minifig_anchor.dat", ModelTypes.minifig },
        };

        private static readonly Dictionary<string, ComponentTypes> kComponentAnchors = new Dictionary<string, ComponentTypes>()
        {
            { "body_anchor.dat", ComponentTypes.body },
            { "component_anchor.dat", ComponentTypes.body }, // This is a fake component just for testing
        };

        public static Command GetModelCommand(string command)
        {
            Command cmd = new Command();
            if (string.IsNullOrEmpty(command))
                return cmd;

            cmd.modelType = GetModelTypeFromCommandString(command);
            if (cmd.modelType == ModelTypes.invalid)
            {
                cmd.type = GameEntityType.Invalid;
                return cmd;
            }

            cmd.type = GameEntityType.Model;
            return cmd;
        }

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
