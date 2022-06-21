using System.Collections.Generic;
using System.Linq;
using CosmosDbExplorer.Core.Helpers;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;

namespace CosmosDbExplorer.AvalonEdit
{
    /// <summary>
    /// Allows producing foldings from a document based on braces.
    /// </summary>
    public class BraceFoldingStrategy
    {
        private readonly FoldFinder _foldFinder;
        /// <summary>
        /// Creates a new BraceFoldingStrategy.
        /// </summary>
        public BraceFoldingStrategy()
        {
            _foldFinder = new FoldFinder(new List<Delimiter> {
                //Json Object Delimiters
                new Delimiter { Start = "{", End = "}" },
                //Json Array Delimiters
                new Delimiter { Start = "[", End = "]" }
            }, false);
        }

        public void UpdateFoldings(FoldingManager manager, TextDocument document)
        {
            var newFoldings = CreateNewFoldings(document, out int firstErrorOffset);
            manager.UpdateFoldings(newFoldings, firstErrorOffset);
        }

        /// <summary>
        /// Create <see cref="NewFolding"/>s for the specified document.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="firstErrorOffset"></param>
        private IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
        {
            var result = _foldFinder.Scan(document.Text, throwOnError: false)
                .OrderBy(o => o.Start)
                .Select(o => new NewFolding(o.Start, o.End));
            firstErrorOffset = _foldFinder.FirstErrorOffset;
            return result;
        }
    }
}
