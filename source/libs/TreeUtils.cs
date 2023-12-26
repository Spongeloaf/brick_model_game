using Godot;
using System.Collections.Generic;

namespace BrickModelGame.source.libs
{
    public static class TreeUtils
    {
        // Searches direct children of the scene root for the first node that has an IGameManager interface.
        public static IGameManager FindGameManager(Node node)
        {
            Node root = node.GetTree().Root;

            // For whatever reason, the root is not the scene root.
            // The scene root appears to be the first child of the root.
            // This may not always be true, and could bite me in the ass.
            try
            {
                Node sceneRoot = root.GetChildren()[0];

                foreach (Node child in sceneRoot.GetChildren())
                {
                    if (child is IGameManager manager)
                        return manager;
                }

                OmniLogger.Error("Could not find a game manager!");
                return null;

            }
            catch
            {
                OmniLogger.Error("Failed to find scene root inside FindGameManager()");
                return null;
            }
        }

        public static List<T> FindDirecChildren<T>(Node node) where T : class
        {
            List<T> results = new List<T>();
            foreach (Node child in node.GetChildren())
            {
                if (child is T candidate)
                    results.Add(candidate);
            }
            return results;
        }
    }
}
