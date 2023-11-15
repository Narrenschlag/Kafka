using Cutulu;
using Godot;

namespace Kafka.Editor
{
    public partial class NodeEditor : Window
    {
        [Export] private LineEdit Key;
        [Export] private LineEdit Name;
        [Export] private CodeEdit Content;

        [Export] private Button saveButton;
        [Export] private Button settingButton;

        private KNode Node;

        public override void _EnterTree()
        {
            saveButton.ConnectButton(this, "saveValues");
            settingButton.ConnectButton(this, "openSettings");

            this.Connect("close_requested", this, "close");
        }

        public void Setup(ref KNode node)
        {
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

            Master.SaveNode(Node);
            update();
        }

        public void openSettings()
        {
            if (Node.IsNull()) return;

            "Settings are not implemented yet.".LogError();
        }

        public void close()
        {
            this.Destroy();
        }
    }
}