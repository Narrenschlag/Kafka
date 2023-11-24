using System.Collections.Generic;
using Cutulu;
using Godot;

namespace Kafka
{
    [GlobalClass]
    public partial class KNode : Resource
    {
        public StatementContainer Statement = new StatementContainer();
        public KeyValuePair<string, string>[] _options;

        public Node LocalNode;
        public string Key;

        public string[] options()
        {
            if (_options.IsEmpty()) return null;

            string[] options = new string[_options.Length];
            for (int i = 0; i < _options.Length; i++)
                options[i] = _options[i].Value;

            return options;
        }

        public string content() => Statement.Text;
        public string name() => Statement.Name;

        public KeyValuePair<string, string> option(int i) => _options.GetClampedElement(i);
        public KeyValuePair<string, string> next => Statement.Next;

        #region Requiments reading
        public static KeyValuePair<string, string>[] OnlyMeetingRequirements(KNode node)
        {
            if (node.IsNull() || node.Statement.Options.IsEmpty()) return null;

            // Check for requirement
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();

            foreach ((string key, string val) in node.Statement.Options)
            {
                if (val.IsEmpty()) continue;
                if (val.Contains("{req:"))
                {
                    // Extracts the entries for requirements in options
                    NestedString nestedString = NestedString.Parse(val, "{req", "", ':', '}');
                    bool add = true;

                    // Find all lines
                    string[] lines = nestedString.ReadValues("{req");
                    if (lines.NotEmpty())
                        foreach (string line in lines)
                        {
                            // Return if already failed
                            if (!add) break;

                            // Incorrect format
                            if (!parse(line, ':', out string type, out string content))
                                continue;

                            // No indicators
                            if (!content.Contains('>') && !content.Contains('=') && !content.Contains('<'))
                                continue;

                            // Find two args, valueName and value
                            string[] args = content.Split(new char[] { '<', '!', '=', '>' }, System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);
                            if (args.NotEmpty() && args.Length >= 2)
                            {
                                $"({type}) {content} {args[0]}: {args[1]}".Log();

                                bool greater = content.Contains('>');
                                bool smaller = content.Contains('=');
                                bool noEqual = content.Contains("!=");
                                bool equal = !noEqual && content.Contains('=');

                                switch (type.ToLower().Trim()[0])
                                {
                                    // String
                                    case 's':
                                        if (noEqual || equal)
                                        {
                                            string _s = TempData.Get(args[0], "");
                                            if (_s.Trim().Equals(args[1].Trim()))
                                                add = equal;
                                        }

                                        break;

                                    // Integer
                                    case 'i':
                                        if (!int.TryParse(args[1], out int _vi))
                                            add = false;

                                        else
                                        {
                                            int vi = TempData.Get(args[0], 0);
                                            add = (greater && _vi > vi) ||
                                            (smaller && _vi < vi) ||
                                            (equal && _vi == vi) ||
                                            (noEqual && _vi != vi);
                                        }
                                        break;

                                    // Float
                                    case 'f':
                                        if (!float.TryParse(args[1], out float _vf))
                                            add = false;

                                        else
                                        {
                                            float vf = TempData.Get(args[0], 0f);
                                            add = (greater && _vf > vf) ||
                                            (smaller && _vf < vf) ||
                                            (equal && _vf == vf) ||
                                            (noEqual && _vf != vf);
                                        }
                                        break;

                                    // Double
                                    case 'd':
                                        if (!double.TryParse(args[1], out double _vd))
                                            add = false;

                                        else
                                        {
                                            double vd = TempData.Get(args[0], 0d);
                                            add = (greater && _vd > vd) ||
                                            (smaller && _vd < vd) ||
                                            (equal && _vd == vd) ||
                                            (noEqual && _vd != vd);
                                        }
                                        break;

                                    // Bool
                                    case 'b':
                                        if (!bool.TryParse(args[1], out bool value))
                                            add = false;

                                        else
                                        {
                                            add = TempData.Get(args[0], false) == equal;
                                        }

                                        break;

                                    default:
                                        break;
                                }
                            }
                        }

                    // Add valid options
                    if (add) list.Add(new KeyValuePair<string, string>(key, nestedString.Value));
                }

                // Add valid options
                else list.Add(new KeyValuePair<string, string>(key, val));
            }

            // Return values
            return list.IsEmpty() ? null : list.ToArray();

            bool parse(string line, char sep, out string type, out string content)
            {
                string[] args = line.Split(sep, System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);
                content = default;
                type = default;

                if (args.IsEmpty() || args.Length < 2) return false;

                content = args[1];
                type = args[0];
                return true;
            }
        }
        #endregion
    }

    #region Statement Struct
    public class StatementContainer
    {
        public string Name { get; set; }
        public string Text { get; set; }

        public KeyValuePair<string, string>[] Options { get; set; }
        public KeyValuePair<string, string> Next { get; set; }
    }
    #endregion
}