using Kafka.Editor;
using Godot;
using Kafka;

namespace Kafka.Editor
{
    public partial class SettingsEditor : WindowBase
    {
        [Export] private PackedScene Prefab;
        [Export] private Node Root;

        private KNode node;

        public virtual void Load(ref KNode node)
        {
            this.node = node;

            update();
        }

        public virtual void update()
        {

        }

        public virtual void save()
        {
            Master.SaveNode(node);
        }
    }
}