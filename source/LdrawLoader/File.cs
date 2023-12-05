using Godot;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Ldraw
{
    public class UserModelFile
    {
        private string m_fullFilePath;
        private string m_fileContents;
        private Command m_loadModelsCommand = new Command();
        private List<EmbeddedFile> m_embeddedFiles = new List<EmbeddedFile>();

        public UserModelFile(string fullFilePath)
        {
            m_fullFilePath = fullFilePath;
            m_fileContents = FileCache.OpenFile(m_fullFilePath);
            if (string.IsNullOrEmpty(m_fileContents))
                return; 

            m_loadModelsCommand.subfileName = m_fullFilePath;
            m_loadModelsCommand.metadata.fileName = m_fullFilePath;
            List<Command> commands = Ldraw.Parsing.GetCommandsFromFile(m_loadModelsCommand);
            foreach (Command cmd in commands)
            {
                switch (cmd.type)
                {
                    case Ldraw.CommandType.SubFile:
                        m_embeddedFiles.Add(new EmbeddedFile(cmd.subfileName, cmd.metadata, Transform3D.Identity));
                        break;

                    default:
                        break;
                }
            }
        }

        public Node3D GetModels()
        {
            Node3D scene = new Node3D();
            foreach (EmbeddedFile efile in m_embeddedFiles)
            {
                efile.ConnectModelTreeToOwner(scene);
            }

            scene.Name = System.IO.Path.GetFileName(m_fullFilePath);
            return scene;
        }

    }   // class UserModelFile

    public class EmbeddedFile
    {
        private string m_fullFilePath;
        private string m_fileContents;
        private Command m_loadModelsCommand = new Command();
        private List<Model> m_models = new List<Model>();
        private List<Component> m_components = new List<Component>();

        public EmbeddedFile(string fullFilePath, LdrMetadata metadata, Transform3D tfm)
        {
            TODO: // figure out  what to do with the TFM.

            m_fullFilePath = fullFilePath;
            m_fileContents = FileCache.OpenFile(m_fullFilePath);
            if (string.IsNullOrEmpty(m_fileContents))
                return;

            m_loadModelsCommand.subfileName = m_fullFilePath;
            m_loadModelsCommand.metadata = metadata;
            m_loadModelsCommand.transform = tfm;
            List<Command> commands = Ldraw.Parsing.GetCommandsFromFile(m_loadModelsCommand);
            foreach (Command cmd in commands)
            {
                switch (cmd.type)
                {
                    case Ldraw.CommandType.Model:
                        // Models need the list of commands because we don't know what we're parsing until
                        // we stumble upon a model anchor, then we need to treat the list that contains
                        // the anchor as a model.
                        m_models.Add(new Model(cmd, commands));
                        break;

                    case CommandType.Component:
                        m_components.Add(new Component(cmd, commands));
                        break;

                    default:
                        OmniLogger.Info("Ldraw models should only contain components as direct children, not primitives or parts.");
                        break;
                }
            }
        }

        public List<Component> GetComponents()
        {
            return m_components;
        }

        public void ConnectModelTreeToOwner(Node3D sceneRoot)
        {
            if (sceneRoot == null)
            {
                OmniLogger.Error("Model scene root is null");
                return;
            }

            foreach (Model model in m_models)
                model.ConnectModelToOwner(sceneRoot);
        }
    }   // class UserModelFile
}
