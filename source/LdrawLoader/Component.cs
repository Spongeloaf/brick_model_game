
namespace Ldraw
{
    public class Component
    {
        public enum Type
        {
            prop,
            body,
            head,
            arm,
            leg,
        }

        public Component(Command cmd)
        {
         
        }

        public readonly Type m_type;
        public  MeshManager m_meshManager;
    }
}
