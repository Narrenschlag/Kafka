using System.Collections.Generic;
using Cutulu;
using Godot;
using static Cutulu.Modding;

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

            string path = folder + LocalPath + ending;
            LocalPath = LocalPath.Trim();

            if (path.TryFind(out var list) && list[0].TryReadJson(path, out Dictionary<string, StatementContainer> entries))
            {
                value = new KConversation
                {
                    LocalPath = LocalPath,
                    Entries = entries,
                    Ending = ending
                };

                return true;
            }

            value = null;
            return false;
        }

        public static bool TryLoad(string LocalPath, string Key, out KConversation value, bool ValidateDefaultFilesForDialogue = true, string LangFileEnding = LangFileEnding, string DefaultLangFileEnding = DefaultLangFileEnding)
        {
            value = null;

            if (LocalPath.IsEmpty()) return false;
            LocalPath = LocalPath.Trim();

            #region File Validation
            Dictionary<string, StatementContainer> entries;
            string _ending;

            if (ValidateDefaultFilesForDialogue)
            {
                if (
                    invalidate(LangFileEnding, out entries) &&                // real ending
                    invalidate(DefaultLangFileEnding, out entries)            // default
                    )
                    return false;
            }

            else
            {
                if (
                    invalidate(LangFileEnding, out entries)                   // real ending
                    )
                    return false;
            }

            string path(string ending) => Folder + LocalPath + ending;
            string metaPath(string ending) => path(ending) + ".meta";

            // Uses a .meta file to speed up the validation process
            bool invalidate(string ending, out Dictionary<string, StatementContainer> entries)
            {
                if (Modding.TryFind(metaPath(_ending = ending), out var list))
                {
                    foreach (var item in list)
                    {
                        if (item.TryReadJson(metaPath(ending), out List<string> meta))
                        {
                            if (meta.Contains(Key))
                            {
                                if (item.TryReadJson(path(ending), out entries))
                                {
                                    _ending = ending;
                                    return false;
                                }
                            }
                        }
                    }
                }

                entries = null;
                return true;
            }
            #endregion

            "Found dialogue...".Log();

            value = new KConversation
            {
                LocalPath = LocalPath,
                Entries = entries,
                Ending = _ending
            };
            return true;
        }

        public bool TryLoad(string Key, Node Node, out KNode nodeValue)
        {
            nodeValue = null;

            if (!Entries.TryGetValue(Key = Key.Trim(), out StatementContainer statement)) return false;

            nodeValue = new KNode
            {
                Statement = statement,
                LocalNode = Node,
                Key = Key
            };

            nodeValue._options = KNode.OnlyMeetingRequirements(nodeValue);
            return true;
        }

        public string json() => Entries.json();
    }
}