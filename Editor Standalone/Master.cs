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

            KNode node;
            if (Conversation.Entries.IsEmpty() || !Conversation.TryLoad(Conversation.Entries.Keys.First(), null, out node) || node.IsNull())
                node = new KNode()
                {
                    Key = "key_01",
                    Statement = new StatementContainer()
                    {
                        Name = "John Doe",
                        Text = "Lorem Ipsum"
                    }
                };

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
            if (NodeEditorInstance.NotNull()) NodeEditorInstance.Destroy();
            if (node.IsNull() || Window.IsNull()) return;

            NodeEditorInstance = Window.Instantiate<NodeEditor>(Root);
            NodeEditorInstance.Setup(Root, ref node);
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

            string path = FileFolder + Conversation.LocalPath + Conversation.Ending;
            Modding.Mods.WriteJson(path, Conversation);

            Modding.Mods.WriteJson(path + ".meta", Conversation.Entries.Keys);
            $"Saved file and meta.".Log();
        }

        private static bool TryReadFile(string local, string ending, out KConversation value)
            => KConversation.TryLoad(FileFolder, local, ending, out value);
    }
}