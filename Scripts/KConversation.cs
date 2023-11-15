using System.Collections.Generic;
using Cutulu;
using Godot;

namespace Kafka
{
    [GlobalClass]
    public partial class KConversation : Resource
    {
        public const string Folder = "Dialogue/";

        private const string LangFileEnding = ".txt";
        private const string DefaultLangFileEnding = ".txt";

        public Dictionary<string, StatementContainer> Entries;
        public string LocalPath;
        public string Ending;

        public static bool TryLoad(string folder, string LocalPath, string ending, out KConversation value)
        {
            value = null;

            if (LocalPath.IsEmpty()) return false;
            LocalPath = LocalPath.Trim();

            if (Modding.TryLoadModJson(LocalPath + ending, out Dictionary<string, StatementContainer> entries, folder))
            {
                value = new KConversation();
                value.LocalPath = LocalPath;
                value.Entries = entries;
                value.Ending = ending;

                return true;
            }

            value = null;
            return false;
        }

            public static bool TryLoad(string LocalPath, string Key, out KConversation value, bool ValidateDefaultFilesForDialogue = true, string LangFileEnding = KConversation.LangFileEnding, string DefaultLangFileEnding = KConversation.DefaultLangFileEnding)
        {
            string _ending = LangFileEnding;
            value = null;

            if (LocalPath.IsEmpty()) return false;
            LocalPath = LocalPath.Trim();

            #region File Validation
            Dictionary<string, StatementContainer> entries = null;
            if (ValidateDefaultFilesForDialogue)
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

            string metaPath(string ending) => Folder + LocalPath + ending + ".meta";
            string path(string ending) => Folder + LocalPath + ending;

            // Uses a .meta file to speed up the validation process
            bool invalidate(string ending, bool mod)
            {
                _ending = ending;

                if (mod)
                {
                    if (!Modding.TryLoadModJson(metaPath(ending), out List<string> meta)) return true;
                    if (!meta.Contains(Key)) return true;

                    return !Modding.TryLoadModJson(path(ending), out entries);
                }

                else
                {
                    if (!Modding.TryLoadAssetJson(metaPath(ending), out List<string> meta)) return true;
                    if (!meta.Contains(Key)) return true;

                    return !Modding.TryLoadAssetJson(path(ending), out entries);
                }
            }
            #endregion

            value = new KConversation();
            value.LocalPath = LocalPath;
            value.Entries = entries;
            value.Ending = _ending;
            return true;
        }

        public bool TryLoad(string Key, Node Node, out KNode value)
        {
            value = null;

            if (!Entries.TryGetValue(Key = Key.Trim(), out StatementContainer statement)) return false;

            value = new KNode();
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