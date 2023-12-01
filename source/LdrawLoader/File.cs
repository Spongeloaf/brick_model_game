using Godot;
using System.Collections.Generic;

namespace Ldraw
{
    public class LdrFile
    {
        private string m_fullFilePath;
        private string m_fileHandle;
        private string m_fileContents;
        private Command m_loadModelsCommand = new Command();
        private List<Model> m_models = new List<Model>();
        private List<Component> m_components = new List<Component>();

        public LdrFile(string fullFilePath)
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
                    case Ldraw.GameEntityType.Model:
                        m_models.Add(new Model(cmd));
                        break;

                    case GameEntityType.Component:
                        m_components.Add(new Component(cmd));
                        break;

                    case Ldraw.GameEntityType.File:
                        LdrFile file = new LdrFile(cmd.subfileName);
                        m_models.AddRange(file.GetModelList());
                        break;

                    default:
                        OmniLogger.Info("Ldraw models should only contain components as direct children, not primitives or parts.");
                        break;
                }
            }
        }

        private bool LoadModelFile()
        {          
            
            return true;
        }

        public List<Node3D> GetNode3DList()
        {
            List<Node3D> modelList = new List<Node3D>();
            foreach (Model model in m_models)
            {
                Node3D modelNode = model.GetModelInstance();
                modelList.Add(modelNode);
            }
            return modelList;
        }

        public List<Model> GetModelList()
        {
            return m_models;
        }

        public List<Component> GetComponentList()
        {
            return m_components;
        }
    }
}
