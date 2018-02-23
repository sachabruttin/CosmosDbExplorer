using ICSharpCode.AvalonEdit;

namespace DocumentDbExplorer.Infrastructure.AvalonEdit
{
    public static class AvalonCommands
    {
        public static readonly RelayCommand<TextEditor> CommentCommand = new RelayCommand<TextEditor>(OnCommentCommand);
        public static readonly RelayCommand<TextEditor> UnCommentCommand = new RelayCommand<TextEditor>(OnUnCommentCommand);

        private static void OnCommentCommand(TextEditor textEditor)
        {
            var document = textEditor.Document;
            var start = document.GetLineByOffset(textEditor.SelectionStart);
            var end = document.GetLineByOffset(textEditor.SelectionStart + textEditor.SelectionLength);

            var prefix = GetCommentPrefix(textEditor);

            using (document.RunUpdate())
            {
                var startIndex = start.LineNumber;
                var endIndex = end.LineNumber;
                for (var i = startIndex; i <= endIndex; i++)
                {
                    var line = document.GetLineByNumber(i);
                    document.Insert(line.Offset, prefix);
                }
            }
        }

        private static void OnUnCommentCommand(TextEditor textEditor)
        {
            var document = textEditor.Document;
            var start = document.GetLineByOffset(textEditor.SelectionStart);
            var end = document.GetLineByOffset(textEditor.SelectionStart + textEditor.SelectionLength);

            var prefix = GetCommentPrefix(textEditor);

            using (document.RunUpdate())
            {
                var startIndex = start.LineNumber;
                var endIndex = end.LineNumber;
                for (var i = startIndex; i <= endIndex; i++)
                {
                    var line = document.GetLineByNumber(i);

                    if (document.GetText(line.Offset, prefix.Length) == prefix)
                    {
                        document.Remove(line.Offset, prefix.Length);
                    }
                }
            }
        }

        private static string GetCommentPrefix(TextEditor textEditor)
        {
            switch (textEditor.SyntaxHighlighting.Name)
            {
                case "DocumentDb":
                    return "--";
                case "Json":
                default:
                    return "//";
            }
        }
    }
}
