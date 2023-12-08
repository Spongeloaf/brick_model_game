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
        private Model m_model = null;

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
                    case Ldraw.CommandType.Subfile:
                        m_embeddedFiles.Add(new EmbeddedFile(cmd.subfileName, cmd.metadata));
                        break;

                    case Ldraw.CommandType.Model:
                        // A file could have a model that is not in an embedded subfile.
                        m_model = new Model(cmd, commands);
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

            if (m_model != null)
                m_model.ConnectModelToOwner(scene);

            scene.Name = System.IO.Path.GetFileName(m_fullFilePath);
            return scene;
        }

    }   // class UserModelFile

    public class EmbeddedFile
    {
        private string m_fullFilePath;
        private string m_fileContents;
        private Command m_loadModelsCommand = new Command();
        private Model m_model = null;

        public EmbeddedFile(string fullFilePath, LdrMetadata metadata)
        {
            m_fullFilePath = fullFilePath;
            m_fileContents = FileCache.OpenFile(m_fullFilePath);
            if (string.IsNullOrEmpty(m_fileContents))
                return;

            m_loadModelsCommand.subfileName = m_fullFilePath;
            m_loadModelsCommand.metadata = metadata;

            List<Command> commands = Ldraw.Parsing.GetCommandsFromFile(m_loadModelsCommand);
            foreach (Command cmd in commands)
            {
                switch (cmd.type)
                {
                    case Ldraw.CommandType.Model:
                        // Models need the list of commands because we don't know what we're parsing until
                        // we stumble upon a model anchor, then we need to treat the list that contains
                        // the anchor as a model.
                        m_model =new Model(cmd, commands);
                        return;

                    default:
                        break;
                }
            }
        }

        public Model GetModel()
        {
            return m_model;
        }

        public void ConnectModelTreeToOwner(Node3D sceneRoot)
        {
            if (sceneRoot == null)
            {
                OmniLogger.Error("Model scene root is null");
                return;
            }

            if (m_model == null)
                return;

            m_model.ConnectModelToOwner(sceneRoot);
        }
    }   // class EmbeddedFile
}
