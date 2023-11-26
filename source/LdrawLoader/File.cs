using Godot;
using System.Collections.Generic;

namespace Ldraw
{
    public class ldrFile
    {
        private List<Model> m_models = new List<Model>();

        public ldrFile(string fullFilePath)
        {
            m_models = Parsing.GetModelsFromFile(fullFilePath);
        }

        public List<Node3D> GetModelList()
        {
            List<Node3D> modelList = new List<Node3D>();
            foreach (Model model in m_models)
            {
                Node3D modelNode = model.GetModelInstance();
                modelList.Add(modelNode);
            }
            return modelList;
        }
    }
}
