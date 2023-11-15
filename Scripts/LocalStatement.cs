using System.Collections.Generic;
using Cutulu;
using Godot;

namespace Kafka
{
    [GlobalClass]
    public partial class LocalStatement : Resource
    {
        public StatementContainer Statement;
        public string[] optionStrings;

        public Node LocalNode;
        public string Key;

        public string[] options() => optionStrings;
        public string content() => Statement.Text;
        public string name() => Statement.Name;

        public KeyValuePair<string, string> option(int i) => Statement.Options.GetClampedElement(i);
        public KeyValuePair<string, string> next => Statement.Next;

    }

    #region Statement Struct
    public struct StatementContainer
    {
        public string Name { get; set; }
        public string Text { get; set; }

        public KeyValuePair<string, string>[] Options { get; set; }
        public KeyValuePair<string, string> Next { get; set; }
    }
    #endregion
}