using System.Linq;
using Cutulu;
using Godot;

namespace Kafka.Editor
{
    public partial class Master : Node
    {
        private const string FileFolder = "Files/";

        [Export] private PackedScene _Window;

        private static PackedScene Window;
        private static Node Root;

        public static KConversation Conversation;
        private static NodeEditor NodeEditorInstance;
        private bool mouseDown;

        public override void _EnterTree()
        {
            Window = _Window;
            Root = this;
        }

        public override void _Ready()
        {
            LoadOrCreateFile("test", ".txt");
            SaveFile();

            KNode node = new KNode();
            node.Key = "key_01";
            node.Statement.Name = "John Doe";
            node.Statement.Text = "Lorem Ipsum";

            EditNode(ref node);
        }

        public override void _Process(double delta)
        {
            if (!"ui_accept".Down(ref mouseDown)) return;

            "Click".Log();
        }

        public static void SaveFile() => WriteFile();
        public static void LoadOrCreateFile(string local, string ending)
        {
            if (!TryReadFile(local, ending, out Conversation))
            {
                Conversation = new KConversation();

                Conversation.Entries = new System.Collections.Generic.Dictionary<string, StatementContainer>();
                Conversation.LocalPath = local;
                Conversation.Ending = ending;
            }
        }

        public static void EditNode(ref KNode node)
        {
            if (node.IsNull() || Window.IsNull()) return;

            if (NodeEditorInstance.NotNull()) NodeEditorInstance.Destroy();

            NodeEditorInstance = Window.Instantiate<NodeEditor>(Root);
            NodeEditorInstance.Setup(ref node);
        }

        public static void SaveNode(KNode node)
        {
            if (Conversation.Entries.ContainsKey(node.Key))
                Conversation.Entries[node.Key] = node.Statement;
            else Conversation.Entries.Add(node.Key, node.Statement);

            SaveFile();
        }

        public static void removeKey(string key)
        {
            if(Conversation.Entries.ContainsKey(key))
                Conversation.Entries.Remove(key);
        }

        private static void WriteFile()
        {
            if (Conversation.IsNull()) return;

            string path = Conversation.LocalPath.LocalToGlobal(IO.USER_PATH + FileFolder) + Conversation.Ending;
            path.WriteText(Conversation.json());

            string meta = Conversation.Entries.Keys.json();
            $"{path}.meta".WriteText(meta);

            $"Saved file and meta.".Log();
        }

        private static bool TryReadFile(string local, string ending, out KConversation value)
            => KConversation.TryLoad(FileFolder, local, ending, out value);
    }
}