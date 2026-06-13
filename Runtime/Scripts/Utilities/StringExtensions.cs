using System.Text;
using UnityEngine;

namespace CrossingLears
{
    public static class StringExtensions
    {
        public static string Bold(this string input)
        {
            return $"<b>{input}</b>";
        }

        public static string Italic(this string input)
        {
            return $"<i>{input}</i>";
        }

        public static string Underline(this string input)
        {
            return $"<u>{input}</u>";
        }

        public static string Strikethrough(this string input)
        {
            return $"<s>{input}</s>";
        }

        public static string Color(this string input, Color color)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{input}</color>";
        }

        public static string Color(this string input, string color)
        {
            return $"<color={color}>{input}</color>";
        }

        public static string Size(this string input, int size)
        {
            return $"<size={size}>{input}</size>";
        }

        public static string Material(this string input, int materialIndex)
        {
            return $"<material={materialIndex}>{input}</material>";
        }

        public static string Sprite(int spriteIndex)
        {
            return $"<sprite={spriteIndex}>";
        }

        public static string Sprite(string spriteName)
        {
            return $"<sprite name=\"{spriteName}\">";
        }

        public static string FontWeight(this string input, string weight)
        {
            return $"<font-weight={weight}>{input}</font-weight>";
        }

        public static string Uppercase(this string input)
        {
            return $"<uppercase>{input}</uppercase>";
        }

        public static string Lowercase(this string input)
        {
            return $"<lowercase>{input}</lowercase>";
        }

        public static string SmallCaps(this string input)
        {
            return $"<smallcaps>{input}</smallcaps>";
        }

        public static string AllCaps(this string input)
        {
            return $"<allcaps>{input}</allcaps>";
        }

        public static string Superscript(this string input)
        {
            return $"<sup>{input}</sup>";
        }

        public static string Subscript(this string input)
        {
            return $"<sub>{input}</sub>";
        }

        public static string Mark(this string input, Color color)
        {
            return $"<mark=#{ColorUtility.ToHtmlStringRGBA(color)}>{input}</mark>";
        }

        public static string Alpha(this string input, float percentage)
        {
            byte alpha = (byte)Mathf.Clamp(Mathf.RoundToInt(percentage / 100f * 255f), 0, 255);
            return $"<alpha=#{alpha:X2}>{input}";
        }

        public static string NoParse(this string input)
        {
            return $"<noparse>{input}</noparse>";
        }

        public static string Link(this string input, string id)
        {
            return $"<link=\"{id}\">{input}</link>";
        }

        public static string Indent(this string input, int pixels)
        {
            return $"<indent={pixels}>{input}</indent>";
        }

        public static string LineIndent(this string input, int pixels)
        {
            return $"<line-indent={pixels}>{input}</line-indent>";
        }

        public static string Margin(this string input, int pixels)
        {
            return $"<margin={pixels}>{input}</margin>";
        }

        public static string MarginLeft(this string input, int pixels)
        {
            return $"<margin-left={pixels}>{input}</margin-left>";
        }

        public static string MarginRight(this string input, int pixels)
        {
            return $"<margin-right={pixels}>{input}</margin-right>";
        }

        public static string Width(this string input, int pixels)
        {
            return $"<width={pixels}>{input}</width>";
        }

        public static string Monospace(this string input, int pixels)
        {
            return $"<mspace={pixels}>{input}</mspace>";
        }

        public static string CharacterSpace(this string input, int pixels)
        {
            return $"<cspace={pixels}>{input}</cspace>";
        }

        public static string LineHeight(this string input, int pixels)
        {
            return $"<line-height={pixels}>{input}</line-height>";
        }

        public static string Position(this string input, int pixels)
        {
            return $"<pos={pixels}>{input}</pos>";
        }

        public static string VerticalOffset(this string input, int pixels)
        {
            return $"<voffset={pixels}>{input}</voffset>";
        }

        public static string Rotate(this string input, int degrees)
        {
            return $"<rotate={degrees}>{input}</rotate>";
        }

        public enum TextAlignment
        {
            Left,
            Center,
            Right,
            Justified,
            Flush,
            Geo
        }
        public static string Align(this string input, TextAlignment alignment)
        {
            return $"<align=\"{alignment}\">{input}</align>";
        }

        public static string Nicify(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            StringBuilder builder = new StringBuilder(input.Length + 8);

            int startIndex = 0;

            if (input.Length >= 2 && input[0] == 'm' && input[1] == '_')
            {
                startIndex = 2;
            }
            else if (input.Length >= 2 && input[0] == 'k' && char.IsUpper(input[1]))
            {
                startIndex = 1;
            }

            for (int i = startIndex; i < input.Length; i++)
            {
                char current = input[i];

                if (builder.Length == 0)
                {
                    builder.Append(char.ToUpperInvariant(current));
                    continue;
                }

                char previous = input[i - 1];

                if (char.IsUpper(current))
                {
                    if (char.IsLower(previous) ||
                        char.IsDigit(previous) ||
                        (char.IsUpper(previous) &&
                        i + 1 < input.Length &&
                        char.IsLower(input[i + 1])))
                    {
                        builder.Append(' ');
                    }
                }

                builder.Append(current);
            }

            return builder.ToString();
        }
    }
}