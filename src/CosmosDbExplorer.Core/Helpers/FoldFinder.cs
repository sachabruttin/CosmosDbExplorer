using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CosmosDbExplorer.Core.Helpers
{
    /// <summary>
    /// Based on Daniel Gimendez's answer for "Code folding in RichTextBox". Altered to fit our use case
    /// Source: https://stackoverflow.com/questions/18156451/code-folding-in-richtextbox
    /// </summary>
    public class FoldFinder
    {
        private readonly IList<Delimiter> _delimiters;
        private readonly Regex _scanner;

        public FoldFinder(IList<Delimiter> delimiters, bool foldableRoot = true)
        {
            FoldableRoot = foldableRoot;
            _delimiters = delimiters;
            _scanner = RegexifyDelimiters(delimiters);
        }

        public int FirstErrorOffset { get; private set; } = -1;

        public bool FoldableRoot { get; set; }

        public IList<FoldMatch> Scan(string code, int start = 0, int end = -1, bool throwOnError = true)
        {
            FirstErrorOffset = -1;

            var positions = new List<FoldMatch>();
            var stack = new Stack<FoldStackItem>();

            int regexGroupIndex;
            bool isStartToken;
            Delimiter matchedDelimiter;
            var currentItem = default(FoldStackItem);

            foreach (Match match in _scanner.Matches(code, start))
            {
                if (!FoldableRoot && match.Index == 0)
                {
                    continue;
                }

                // the pattern for every group is that 0 corresponds to SectionDelimter, 1 corresponds to Start
                // and 2 corresponds to End.
                regexGroupIndex = match.Groups.Cast<Group>().Select((g, i) => new { g.Success, Index = i })
                    .Where(r => r.Success && r.Index > 0).First().Index;

                matchedDelimiter = _delimiters[(regexGroupIndex - 1) / 3];
                isStartToken = match.Groups[regexGroupIndex + 1].Success;

                if (isStartToken)
                {
                    currentItem = new FoldStackItem()
                    {
                        Delimter = matchedDelimiter,
                        Position = new FoldMatch() { Start = match.Index }
                    };
                    stack.Push(currentItem);
                }
                else
                {
                    if (stack.Count == 0)
                    {
                        if (FirstErrorOffset == -1)
                        {
                            FirstErrorOffset = match.Index;
                        }

                        if (throwOnError)
                        {
                            throw new Exception(string.Format("Unexpected end of string at {0}", match.Index));
                        }

                        break;
                    }

                    currentItem = stack.Pop();
                    if (currentItem.Delimter == matchedDelimiter)
                    {
                        currentItem.Position.End = match.Index + match.Length;
                        //Only add folding if it spans more than 2 lines
                        if (HasAtLeastNLines(code[currentItem.Position.Start..currentItem.Position.End], 2))
                        {
                            positions.Add(currentItem.Position);
                        }

                        // if searching for an end, and we've passed it, and the stack is empty then quit.
                        if (end > -1 && currentItem.Position.End >= end && stack.Count == 0)
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (FirstErrorOffset == -1)
                        {
                            FirstErrorOffset = match.Index;
                        }

                        if (throwOnError)
                        {
                            throw new Exception(string.Format("Invalid Ending Token at {0}", match.Index));
                        }

                        break;
                    }
                }
            }

            if (stack.Count > 0 && throwOnError)
            {
                if (FirstErrorOffset == -1)
                {
                    FirstErrorOffset = code.Length;
                }

                throw new Exception("Not enough closing symbols.");
            }

            return positions;
        }

        private static bool HasAtLeastNLines(string search, int n = 1)
        {
            int count = 0, index = 0;
            if (n <= 1)
            {
                return (search?.Length ?? 0) > 0;
            }

            while ((index = search.IndexOf("\r\n", index, StringComparison.Ordinal)) != -1)
            {
                index += 2;
                count++;
                if (count + 1 >= n)
                {
                    return true;
                }
            }

            return false;
        }

        private static Regex RegexifyDelimiters(IList<Delimiter> delimiters)
        {
            return new Regex(
                string.Join("|", delimiters.Select(d =>
                    string.Format("((\\{0})|(\\{1}))", d.Start, d.End))), RegexOptions.Compiled | RegexOptions.Multiline);
        }

        private struct FoldStackItem
        {
            public FoldMatch Position;
            public Delimiter Delimter;
        }



    }

    public struct FoldMatch
    {
        public int Start;
        public int End;
    }

    public class Delimiter
    {
        public Delimiter(string start, string end)
        {
            Start = start;
            End = end;
        }

        public string Start { get; private set; }
        public string End { get; private set; }
    }

}
