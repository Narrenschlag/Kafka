using Godot.Collections;
using Cutulu;
using Godot;

[GlobalClass]
public partial class CodeHighlightAddon : SyntaxHighlighter
{
    [Export] public Dictionary<string, Color> ColorStrings;
    [Export] public Dictionary<string, Color> ColorChars;
    [Export] public Color NormalColor = Colors.White;

    public override Dictionary _GetLineSyntaxHighlighting(int line)
    {
        string Line = GetTextEdit().GetLine(line);
        Dictionary colors = new Dictionary();
        bool colored = false;

        if (Line.NotEmpty())
            for (int i = 0; i < Line.Length;)
            {
                if (highlightString(ref i)) { }
                else if (highlightChar(ref i)) { }
                else
                {
                    if (colored)
                    {
                        colors.Add(i, NormalColor);
                        colored = false;
                    }

                    i++;
                }
            }

        colors.Count.Log();
        return colors;

        bool highlightString(ref int i)
        {
            if (ColorStrings == null || ColorStrings.Count < 1) return false;

            foreach ((string key, Color color) in ColorChars)
            {
                if (Line.Length <= key.Length + i) continue;
                if (!Line.Substring(i, key.Length).Equals(key)) continue;

                colors.Add(i, color);

                i += key.Length;
                colored = true;

                return true;
            }

            return false;
        }

        bool highlightChar(ref int i)
        {
            if (ColorChars == null || ColorChars.Count < 1) return false;
            char c = Line[i];

            foreach ((string chars, Color color) in ColorChars)
            {
                if (chars.Contains(c))
                {
                    colors.Add(i, color);

                    colored = true;
                    i++;

                    return true;
                }
            }

            return false;
        }
    }
}