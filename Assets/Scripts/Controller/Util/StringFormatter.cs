using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using UnityEngine;
using Random = System.Random;

namespace GeoViewer.Controller.Util
{
    public class StringFormatter
    {
        private const char TagStartChar = '{';
        private const char TagEndChar = '}';
        private const char CommandEndChar = ':';
        private const char ArgumentSeparationChar = ',';

        private static readonly Random Random = new();

        //general Tags
        private static object? TryGetReplacement(ReadOnlySpan<char> tag)
        {
            var index = tag.IndexOf(CommandEndChar);
            if (index <= 0) return null;
            switch (tag[0..(index)].ToString())
            {
                case "rand":
                    var arguments = ReadVector2Int(tag[(index + 1)..]);
                    if (arguments == null) return null;
                    return Random.Next(arguments.Value.Item1, arguments.Value.Item2);

                default: return null;
            }
        }

        public static string FormatString(string input, TagReplacer replacer)
        {
            var resultBuilder = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] ! != TagStartChar)
                {
                    resultBuilder.Append(input[i]);
                    continue;
                }

                if (!ReadTag(ref i, input, out var tag))
                    throw new ArgumentException($"String {input} has invalid tags.");
                resultBuilder.Append(GetReplacement(tag, replacer));
            }

            return resultBuilder.ToString();
        }

        private static bool ReadTag(ref int index, ReadOnlySpan<char> input, out ReadOnlySpan<char> tag)
        {
            tag = ReadOnlySpan<char>.Empty;
            if (input[index] != TagStartChar) return false;
            var startIndex = index;
            for (; index < input.Length; index++)
            {
                if (input[index] == TagEndChar) break;
            }

            if (index == input.Length - 1 && input[^1] != TagEndChar) return false;

            tag = input[(startIndex + 1)..(index)];
            return true;
        }

        private static string GetReplacement(ReadOnlySpan<char> tag, TagReplacer replacer)
        {
            var res = replacer.Invoke(tag) ?? TryGetReplacement(tag);
            if (res == null)
            {
                Debug.LogWarning($"Failed to replace tag {tag.ToString()}");
                return string.Empty;
            }

            if (res is IFormattable formattable)
                return formattable.ToString(null, CultureInfo.InvariantCulture);
            return res.ToString();
        }

        public delegate object? TagReplacer(ReadOnlySpan<char> tag);

        private static (int, int)? ReadVector2Int(ReadOnlySpan<char> tag)
        {
            var index = tag.IndexOf(ArgumentSeparationChar);
            if (index <= 0) return null;

            if (!int.TryParse(tag[0..(index)], out var int1)
                || !int.TryParse(tag[(index + 1)..], out var int2))
            {
                return null;
            }

            return (int1, int2);
        }
    }
}