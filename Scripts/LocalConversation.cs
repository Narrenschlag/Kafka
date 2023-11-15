using System.Collections.Generic;
using Cutulu;
using Godot;

namespace Kafka
{
    [GlobalClass]
    public partial class LocalConversation : Resource
    {
        public const string Folder = "Dialogue/";

        private const string LangFileEnding = ".txt";
        private const string DefaultLangFileEnding = ".txt";

        public Dictionary<string, StatementContainer> Entries;
        public string Path;

        public static bool TryLoad(string Path, string Key, out LocalConversation value)
        {
            value = null;

            if (Path.IsEmpty()) return false;
            Path = Path.Trim();

            #region File Validation
            Dictionary<string, StatementContainer> entries = null;
            if (false) //ValidateDefaultFilesForDialoguegue
            {
                if (
                    invalidate(LangFileEnding, true) &&              // mod: real ending
                    invalidate(DefaultLangFileEnding, true) &&       // mod: default
                    invalidate(LangFileEnding, false) &&             // res: real ending
                    invalidate(DefaultLangFileEnding, false)         // res: default
                    )
                    return false;
            }

            else
            {
                if (
                    invalidate(LangFileEnding, true) &&              // mod
                    invalidate(LangFileEnding, false)                // res
                    )
                    return false;
            }

            string path(string ending) => Folder + Path + ending;
            bool invalidate(string ending, bool mod)
                => !(mod ? Modding.TryLoadModJson(path(ending), out entries) : Modding.TryLoadAssetJson(path(ending), out entries)) || entries.IsEmpty() || !entries.ContainsKey(Key);
            #endregion

            value = new LocalConversation();
            value.Entries = entries;
            value.Path = Path;
            return true;
        }

        public bool TryLoad(string Key, Node Node, out LocalStatement value)
        {
            value = null;

            if (!Entries.TryGetValue(Key = Key.Trim(), out StatementContainer statement)) return false;

            value = new LocalStatement();
            value.Statement = statement;
            value.LocalNode = Node;
            value.Key = Key;

            if (statement.Options.IsEmpty()) value.optionStrings = null;
            else
            {
                value.optionStrings = new string[statement.Options.Length];
                for (int i = 0; i < statement.Options.Length; i++)
                    value.optionStrings[i] = statement.Options[i].Value;
            }

            return true;
        }

        public string json() => Entries.json();
    }
}