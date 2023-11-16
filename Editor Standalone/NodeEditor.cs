using Cutulu;
using Godot;

namespace Kafka.Editor
{
    public partial class NodeEditor : WindowBase
    {
        [Export] private LineEdit Key;
        [Export] private LineEdit Name;
        [Export] private CodeEdit Content;

        [Export] private Button saveButton;
        [Export] private Button settingButton;

        private string initialKey;
        private KNode Node;

        public override void _EnterTree()
        {
            saveButton.ConnectButton(this, "saveValues");
            settingButton.ConnectButton(this, "openSettings");
        }

        public void Setup(Node parent, ref KNode node)
        {
            base.Setup(parent);

            initialKey = node.Key;
            Node = node;

            update();
        }

        public void update()
        {
            if (Node.IsNull()) return;

            Key.PlaceholderText = Node.Key;
            Key.Text = Node.Key;

            Name.Text = Node.Statement.Name;
            Content.Text = Node.Statement.Text;

            Title = $"Node Editor: {Node.Key}";
        }

        public void saveValues()
        {
            if (Node.IsNull()) return;

            Node.Statement.Text = Content.Text.Trim();
            Node.Statement.Name = Name.Text.Trim();
            Node.Key = Key.Text.Trim();

            if (!initialKey.Equals(Node.Key)) Master.removeKey(initialKey);

            Master.SaveNode(Node);
            initialKey = Node.Key;

            update();
        }

        public void openSettings()
        {
            if (Node.IsNull()) return;

            "Settings are not implemented yet.".LogError();
        }
    }
}