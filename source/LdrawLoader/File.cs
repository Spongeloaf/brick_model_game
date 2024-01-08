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
        private List<LdrawModel> m_embeddedModels = new List<LdrawModel>();

        public UserModelFile(string fullFilePath)
        {
            m_fullFilePath = fullFilePath;
            m_fileContents = FileCache.OpenFile(m_fullFilePath);
            if (string.IsNullOrEmpty(m_fileContents))
                return;

            m_loadModelsCommand.subfileName = m_fullFilePath;
            m_loadModelsCommand.metadata.fileName = m_fullFilePath;
            List<Command> commands = Ldraw.Parsing.GetCommandsFromFile(m_loadModelsCommand);

            // We return after the first subfile because the .mpd file format assumes that the first
            // FILE command is the "main" file, and all other embedded files should be children of that file.
            // This implies that embedded files not instanced in the "main" model are orphaned, and never
            // loaded.

            // We also abort after loading the first model because in the .ldr format, there should only be
            // one "main" model, and any children of that model should be embedded subfiles or other .ldr files.

            // This is really the primary difference between .ldr and .mpd files: .ldr files store everything in
            // the main file, whereas .mpd files store every submodel in their own embedded files.
            foreach (Command cmd in commands)
            {
                switch (cmd.type)
                {
                    case Ldraw.CommandType.Subfile:
                        var file = new EmbeddedFile(cmd.subfileName, cmd.metadata, Transform3D.Identity);
                        m_embeddedModels.Add(file.GetModel());
                        return;

                    case Ldraw.CommandType.Model:
                        m_embeddedModels.Add(new LdrawModel(commands, Transform3D.Identity));
                        return;

                    default:
                        break;
                }
            }
        }

        public Node3D GetModels()
        {
            Node3D scene = new Node3D();
            foreach (LdrawModel model in m_embeddedModels)
            {
                model.CreateModel(scene);
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
        private LdrawModel _mLdrawModel = null;

        public EmbeddedFile(string fullFilePath, LdrMetadata metadata, in Transform3D subfileOffset)
        {
            m_fullFilePath = fullFilePath;
            m_fileContents = FileCache.OpenFile(m_fullFilePath);
            if (string.IsNullOrEmpty(m_fileContents))
                return;

            m_loadModelsCommand.subfileName = m_fullFilePath;
            m_loadModelsCommand.metadata = metadata;
            List<Command> commands = Ldraw.Parsing.GetCommandsFromFile(m_loadModelsCommand);
            _mLdrawModel = new LdrawModel(commands, subfileOffset);
        }

        public LdrawModel GetModel()
        {
            return _mLdrawModel;
        }

        public void ConnectModelTreeToOwner(Node3D sceneRoot)
        {
            if (sceneRoot == null)
            {
                OmniLogger.Error("Model scene root is null");
                return;
            }

            if (_mLdrawModel == null)
                return;

            _mLdrawModel.CreateModel(sceneRoot);
        }
    }   // class EmbeddedFile
}
