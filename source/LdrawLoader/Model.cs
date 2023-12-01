using Godot;
using Ldraw;
using System.Collections.Generic;

namespace Ldraw
{
    public class Model
    {
        private readonly string m_modelName;
        private readonly string m_fileName;
        private readonly List<Component> m_components = new List<Component>();
        private readonly MeshManager m_meshManager = new MeshManager();
        private ModelTree.ModelTypes m_modelType = ModelTree.ModelTypes.invalid;

        public Model(Command modelCommand)
        {
            m_modelName = modelCommand.metadata.modelName;
            m_fileName = modelCommand.metadata.fileName;

            if (modelCommand.modelType == ModelTree.ModelTypes.invalid)
            {
                OmniLogger.Error("Model type is invalid");
                return;
            }
            
            List<Command> commands = Ldraw.Parsing.GetCommandsFromFile(modelCommand);
            foreach (Command cmd in commands)
            {
                switch (cmd.type)
                {
                    case Ldraw.GameEntityType.Component:
                        m_components.Add(new Component(modelCommand));
                        break;

                    case GameEntityType.Part:
                        Primitive.AddPrimitiveToMesh(m_meshManager, in cmd);
                        break;

                    case GameEntityType.Model:
                        Primitive.AddPrimitiveToMesh(m_meshManager, in cmd);
                        m_modelType = cmd.modelType;
                        break;

                    case GameEntityType.File:
                        LdrFile file = new LdrFile(cmd.subfileName);
                        m_components.AddRange(file.GetComponentList());
                        break;
                    default:
                        break;
                }
            }
        }

        public Node3D GetModelInstance()
        {
            Node3D model = new Node3D();
            model.Name = m_modelName;
            if (m_modelType == ModelTree.ModelTypes.invalid)
                return model;

            MeshInstance3D meshInstance = m_meshManager.GetMeshInstance();
            model.AddChild(meshInstance);
            meshInstance.Owner = model;

            if (m_components == null || m_components.Count == 0)
                return model;

            foreach (Component component in m_components)
                component.ConnectComponentInstance(model);

            return model;
        }
    }
}