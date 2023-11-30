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
        };

        public static Command GetModelCommand(string command)
        {
            Command cmd = new Command();
            if (string.IsNullOrEmpty(command))
                return cmd;

            cmd.modelType = GetModelType(command);
            return cmd;
        }

        private static ModelTypes GetModelType(string command)
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

        public static GameEntityType GetGameEntityType(Command cmd)
        {
                // Next step:
                // Nothing loads because the models have no component anchors.
                // 1. Do props needa body anchor?
                // 2. Should the model anchor somehow serve as a compenent anchor?
                // 3. Or should the model just get a primitive directlly, as though it were a component?

            if (kModelAnchors.ContainsKey(cmd.subfileName))
                return GameEntityType.Model;

            if (kModelAnchors.ContainsKey(cmd.subfileName))
                return GameEntityType.Component;

            return GameEntityType.Invalid;
        }
    }
}
