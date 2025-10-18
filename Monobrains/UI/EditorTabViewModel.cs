using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Monobrains.Core;
using Monobrains.Services;

namespace Monobrains.UI
{
    public class EditorTabViewModel : INotifyPropertyChanged
    {
        private readonly ISyntaxHighlightingService _syntaxHighlighting;
        private readonly ICodeAnalysisService _codeAnalysis;
        private readonly ICodeCompletionService _codeCompletion;
        private readonly IRefactoringService _refactoring;

        private string _title;
        private string _filePath;
        private string _content;
        private bool _isDirty;
        private int _caretPosition;
        private int _selectionStart;
        private int _selectionLength;
        private ObservableCollection<Diagnostic> _diagnostics;
        private ObservableCollection<CodeCompletionItem> _completionItems;
        private bool _showCompletion;
        private int _completionStart;
        private int _completionLength;

        public EditorTabViewModel(
            ISyntaxHighlightingService syntaxHighlighting,
            ICodeAnalysisService codeAnalysis,
            ICodeCompletionService codeCompletion,
            IRefactoringService refactoring)
        {
            _syntaxHighlighting = syntaxHighlighting;
            _codeAnalysis = codeAnalysis;
            _codeCompletion = codeCompletion;
            _refactoring = refactoring;

            _diagnostics = new ObservableCollection<Diagnostic>();
            _completionItems = new ObservableCollection<CodeCompletionItem>();

            InitializeCommands();
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        public string Content
        {
            get => _content;
            set
            {
                if (SetProperty(ref _content, value))
                {
                    IsDirty = true;
                    AnalyzeCode();
                }
            }
        }

        public bool IsDirty
        {
            get => _isDirty;
            set => SetProperty(ref _isDirty, value);
        }

        public int CaretPosition
        {
            get => _caretPosition;
            set => SetProperty(ref _caretPosition, value);
        }

        public int SelectionStart
        {
            get => _selectionStart;
            set => SetProperty(ref _selectionStart, value);
        }

        public int SelectionLength
        {
            get => _selectionLength;
            set => SetProperty(ref _selectionLength, value);
        }

        public ObservableCollection<Diagnostic> Diagnostics
        {
            get => _diagnostics;
            set => SetProperty(ref _diagnostics, value);
        }

        public ObservableCollection<CodeCompletionItem> CompletionItems
        {
            get => _completionItems;
            set => SetProperty(ref _completionItems, value);
        }

        public bool ShowCompletion
        {
            get => _showCompletion;
            set => SetProperty(ref _showCompletion, value);
        }

        public int CompletionStart
        {
            get => _completionStart;
            set => SetProperty(ref _completionStart, value);
        }

        public int CompletionLength
        {
            get => _completionLength;
            set => SetProperty(ref _completionLength, value);
        }

        public ICommand TriggerCompletionCommand { get; private set; }
        public ICommand InsertCompletionCommand { get; private set; }
        public ICommand GoToDefinitionCommand { get; private set; }
        public ICommand FindReferencesCommand { get; private set; }
        public ICommand RenameSymbolCommand { get; private set; }
        public ICommand FormatSelectionCommand { get; private set; }
        public ICommand CommentSelectionCommand { get; private set; }
        public ICommand UncommentSelectionCommand { get; private set; }
        public ICommand DuplicateLineCommand { get; private set; }
        public ICommand DeleteLineCommand { get; private set; }
        public ICommand MoveLineUpCommand { get; private set; }
        public ICommand MoveLineDownCommand { get; private set; }

        private void InitializeCommands()
        {
            TriggerCompletionCommand = new RelayCommand(TriggerCompletion);
            InsertCompletionCommand = new RelayCommand<CodeCompletionItem>(InsertCompletion);
            GoToDefinitionCommand = new RelayCommand(GoToDefinition);
            FindReferencesCommand = new RelayCommand(FindReferences);
            RenameSymbolCommand = new RelayCommand(RenameSymbol);
            FormatSelectionCommand = new RelayCommand(FormatSelection);
            CommentSelectionCommand = new RelayCommand(CommentSelection);
            UncommentSelectionCommand = new RelayCommand(UncommentSelection);
            DuplicateLineCommand = new RelayCommand(DuplicateLine);
            DeleteLineCommand = new RelayCommand(DeleteLine);
            MoveLineUpCommand = new RelayCommand(MoveLineUp);
            MoveLineDownCommand = new RelayCommand(MoveLineDown);
        }

        private async void AnalyzeCode()
        {
            if (string.IsNullOrEmpty(Content)) return;

            try
            {
                var diagnostics = await _codeAnalysis.AnalyzeCodeAsync(Content, GetLanguageFromFilePath());
                Diagnostics.Clear();
                foreach (var diagnostic in diagnostics)
                {
                    Diagnostics.Add(diagnostic);
                }
            }
            catch (Exception ex)
            {
                // Handle analysis error
            }
        }

        private async void TriggerCompletion()
        {
            if (string.IsNullOrEmpty(Content)) return;

            try
            {
                var completions = await _codeCompletion.GetCompletionsAsync(Content, CaretPosition, GetLanguageFromFilePath());
                CompletionItems.Clear();
                foreach (var completion in completions)
                {
                    CompletionItems.Add(completion);
                }
                ShowCompletion = CompletionItems.Count > 0;
            }
            catch (Exception ex)
            {
                // Handle completion error
            }
        }

        private void InsertCompletion(CodeCompletionItem item)
        {
            if (item == null) return;

            var beforeCaret = Content.Substring(0, CaretPosition);
            var afterCaret = Content.Substring(CaretPosition);

            var insertText = item.InsertText ?? item.Text;
            Content = beforeCaret + insertText + afterCaret;
            CaretPosition = CaretPosition + insertText.Length;
            ShowCompletion = false;
        }

        private async void GoToDefinition()
        {
            if (string.IsNullOrEmpty(Content)) return;

            try
            {
                var definition = await _codeCompletion.GetDefinitionAsync(Content, CaretPosition, GetLanguageFromFilePath());
                if (definition != null)
                {
                    CaretPosition = definition.Position;
                }
            }
            catch (Exception ex)
            {
                // Handle error
            }
        }

        private async void FindReferences()
        {
            if (string.IsNullOrEmpty(Content)) return;

            try
            {
                var references = await _codeCompletion.GetReferencesAsync(Content, CaretPosition, GetLanguageFromFilePath());
                // Show references in a panel
            }
            catch (Exception ex)
            {
                // Handle error
            }
        }

        private async void RenameSymbol()
        {
            if (string.IsNullOrEmpty(Content)) return;

            try
            {
                var symbol = await _codeCompletion.GetSymbolAtPositionAsync(Content, CaretPosition, GetLanguageFromFilePath());
                if (symbol != null)
                {
                    // Show rename dialog
                }
            }
            catch (Exception ex)
            {
                // Handle error
            }
        }

        private async void FormatSelection()
        {
            if (string.IsNullOrEmpty(Content)) return;

            try
            {
                var start = SelectionStart;
                var length = SelectionLength;
                var selectedText = Content.Substring(start, length);
                var formattedText = await _refactoring.FormatCodeAsync(selectedText, GetLanguageFromFilePath());
                
                Content = Content.Substring(0, start) + formattedText + Content.Substring(start + length);
                SelectionStart = start;
                SelectionLength = formattedText.Length;
            }
            catch (Exception ex)
            {
                // Handle error
            }
        }

        private void CommentSelection()
        {
            if (string.IsNullOrEmpty(Content)) return;

            var language = GetLanguageFromFilePath();
            var commentPrefix = GetCommentPrefix(language);
            
            var lines = Content.Split('\n');
            var startLine = GetLineFromPosition(SelectionStart);
            var endLine = GetLineFromPosition(SelectionStart + SelectionLength);

            for (int i = startLine; i <= endLine; i++)
            {
                if (i < lines.Length)
                {
                    lines[i] = commentPrefix + lines[i];
                }
            }

            Content = string.Join("\n", lines);
        }

        private void UncommentSelection()
        {
            if (string.IsNullOrEmpty(Content)) return;

            var language = GetLanguageFromFilePath();
            var commentPrefix = GetCommentPrefix(language);
            
            var lines = Content.Split('\n');
            var startLine = GetLineFromPosition(SelectionStart);
            var endLine = GetLineFromPosition(SelectionStart + SelectionLength);

            for (int i = startLine; i <= endLine; i++)
            {
                if (i < lines.Length && lines[i].TrimStart().StartsWith(commentPrefix))
                {
                    lines[i] = lines[i].Replace(commentPrefix, "", 1);
                }
            }

            Content = string.Join("\n", lines);
        }

        private void DuplicateLine()
        {
            if (string.IsNullOrEmpty(Content)) return;

            var lineNumber = GetLineFromPosition(CaretPosition);
            var lines = Content.Split('\n');
            
            if (lineNumber < lines.Length)
            {
                var newLines = new string[lines.Length + 1];
                Array.Copy(lines, 0, newLines, 0, lineNumber + 1);
                newLines[lineNumber + 1] = lines[lineNumber];
                Array.Copy(lines, lineNumber + 1, newLines, lineNumber + 2, lines.Length - lineNumber - 1);
                
                Content = string.Join("\n", newLines);
                CaretPosition = GetPositionFromLine(lineNumber + 1);
            }
        }

        private void DeleteLine()
        {
            if (string.IsNullOrEmpty(Content)) return;

            var lineNumber = GetLineFromPosition(CaretPosition);
            var lines = Content.Split('\n');
            
            if (lineNumber < lines.Length)
            {
                var newLines = new string[lines.Length - 1];
                Array.Copy(lines, 0, newLines, 0, lineNumber);
                Array.Copy(lines, lineNumber + 1, newLines, lineNumber, lines.Length - lineNumber - 1);
                
                Content = string.Join("\n", newLines);
                CaretPosition = Math.Min(CaretPosition, Content.Length);
            }
        }

        private void MoveLineUp()
        {
            if (string.IsNullOrEmpty(Content)) return;

            var lineNumber = GetLineFromPosition(CaretPosition);
            var lines = Content.Split('\n');
            
            if (lineNumber > 0 && lineNumber < lines.Length)
            {
                var temp = lines[lineNumber - 1];
                lines[lineNumber - 1] = lines[lineNumber];
                lines[lineNumber] = temp;
                
                Content = string.Join("\n", lines);
                CaretPosition = GetPositionFromLine(lineNumber - 1);
            }
        }

        private void MoveLineDown()
        {
            if (string.IsNullOrEmpty(Content)) return;

            var lineNumber = GetLineFromPosition(CaretPosition);
            var lines = Content.Split('\n');
            
            if (lineNumber < lines.Length - 1)
            {
                var temp = lines[lineNumber];
                lines[lineNumber] = lines[lineNumber + 1];
                lines[lineNumber + 1] = temp;
                
                Content = string.Join("\n", lines);
                CaretPosition = GetPositionFromLine(lineNumber + 1);
            }
        }

        private string GetLanguageFromFilePath()
        {
            if (string.IsNullOrEmpty(FilePath)) return "text";
            
            var extension = System.IO.Path.GetExtension(FilePath).ToLower();
            return extension switch
            {
                ".cs" => "csharp",
                ".py" => "python",
                ".js" => "javascript",
                ".ts" => "typescript",
                ".html" => "html",
                ".css" => "css",
                ".json" => "json",
                ".xml" => "xml",
                ".md" => "markdown",
                _ => "text"
            };
        }

        private string GetCommentPrefix(string language)
        {
            return language switch
            {
                "csharp" or "javascript" or "typescript" or "css" => "//",
                "python" => "#",
                "html" or "xml" => "<!--",
                _ => "//"
            };
        }

        private int GetLineFromPosition(int position)
        {
            if (string.IsNullOrEmpty(Content)) return 0;
            
            var lines = Content.Substring(0, Math.Min(position, Content.Length)).Split('\n');
            return lines.Length - 1;
        }

        private int GetPositionFromLine(int lineNumber)
        {
            if (string.IsNullOrEmpty(Content)) return 0;
            
            var lines = Content.Split('\n');
            int position = 0;
            
            for (int i = 0; i < Math.Min(lineNumber, lines.Length); i++)
            {
                position += lines[i].Length + 1; // +1 for newline
            }
            
            return Math.Min(position, Content.Length);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
