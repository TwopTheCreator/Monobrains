using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Monobrains.Core;
using Monobrains.Services;

namespace Monobrains.UI
{
    public class MainWindowViewModel : INotifyPropertyChanged, IMainWindowViewModel
    {
        private readonly IPythonBackendService _pythonBackend;
        private readonly ICodeAnalysisService _codeAnalysis;
        private readonly IDebuggingService _debugging;
        private readonly IExecutionService _execution;
        private readonly IPluginManager _pluginManager;
        private readonly ISyntaxHighlightingService _syntaxHighlighting;
        private readonly IFileManagerService _fileManager;
        private readonly IProjectManagerService _projectManager;
        private readonly ICodeCompletionService _codeCompletion;
        private readonly IRefactoringService _refactoring;

        private ObservableCollection<EditorTabViewModel> _tabs;
        private EditorTabViewModel _activeTab;
        private string _statusText;
        private bool _isDebugging;
        private bool _isExecuting;
        private ObservableCollection<OutputMessage> _outputMessages;
        private ObservableCollection<BreakpointViewModel> _breakpoints;
        private ObservableCollection<VariableViewModel> _variables;
        private ObservableCollection<CallStackItem> _callStack;

        public MainWindowViewModel(
            IPythonBackendService pythonBackend,
            ICodeAnalysisService codeAnalysis,
            IDebuggingService debugging,
            IExecutionService execution,
            IPluginManager pluginManager,
            ISyntaxHighlightingService syntaxHighlighting,
            IFileManagerService fileManager,
            IProjectManagerService projectManager,
            ICodeCompletionService codeCompletion,
            IRefactoringService refactoring)
        {
            _pythonBackend = pythonBackend;
            _codeAnalysis = codeAnalysis;
            _debugging = debugging;
            _execution = execution;
            _pluginManager = pluginManager;
            _syntaxHighlighting = syntaxHighlighting;
            _fileManager = fileManager;
            _projectManager = projectManager;
            _codeCompletion = codeCompletion;
            _refactoring = refactoring;

            _tabs = new ObservableCollection<EditorTabViewModel>();
            _outputMessages = new ObservableCollection<OutputMessage>();
            _breakpoints = new ObservableCollection<BreakpointViewModel>();
            _variables = new ObservableCollection<VariableViewModel>();
            _callStack = new ObservableCollection<CallStackItem>();

            InitializeCommands();
            InitializeServices();
        }

        public ObservableCollection<EditorTabViewModel> Tabs
        {
            get => _tabs;
            set => SetProperty(ref _tabs, value);
        }

        public EditorTabViewModel ActiveTab
        {
            get => _activeTab;
            set => SetProperty(ref _activeTab, value);
        }

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public bool IsDebugging
        {
            get => _isDebugging;
            set => SetProperty(ref _isDebugging, value);
        }

        public bool IsExecuting
        {
            get => _isExecuting;
            set => SetProperty(ref _isExecuting, value);
        }

        public ObservableCollection<OutputMessage> OutputMessages
        {
            get => _outputMessages;
            set => SetProperty(ref _outputMessages, value);
        }

        public ObservableCollection<BreakpointViewModel> Breakpoints
        {
            get => _breakpoints;
            set => SetProperty(ref _breakpoints, value);
        }

        public ObservableCollection<VariableViewModel> Variables
        {
            get => _variables;
            set => SetProperty(ref _variables, value);
        }

        public ObservableCollection<CallStackItem> CallStack
        {
            get => _callStack;
            set => SetProperty(ref _callStack, value);
        }

        public ICommand NewFileCommand { get; private set; }
        public ICommand OpenFileCommand { get; private set; }
        public ICommand SaveFileCommand { get; private set; }
        public ICommand SaveAsFileCommand { get; private set; }
        public ICommand CloseTabCommand { get; private set; }
        public ICommand RunCodeCommand { get; private set; }
        public ICommand DebugCodeCommand { get; private set; }
        public ICommand StopExecutionCommand { get; private set; }
        public ICommand StepOverCommand { get; private set; }
        public ICommand StepIntoCommand { get; private set; }
        public ICommand StepOutCommand { get; private set; }
        public ICommand ToggleBreakpointCommand { get; private set; }
        public ICommand FormatCodeCommand { get; private set; }
        public ICommand FindReplaceCommand { get; private set; }
        public ICommand GoToLineCommand { get; private set; }
        public ICommand BuildProjectCommand { get; private set; }
        public ICommand CleanProjectCommand { get; private set; }
        public ICommand OpenProjectCommand { get; private set; }
        public ICommand NewProjectCommand { get; private set; }
        public ICommand PluginManagerCommand { get; private set; }
        public ICommand SettingsCommand { get; private set; }

        private void InitializeCommands()
        {
            NewFileCommand = new RelayCommand(NewFile);
            OpenFileCommand = new RelayCommand(OpenFile);
            SaveFileCommand = new RelayCommand(SaveFile, () => ActiveTab != null);
            SaveAsFileCommand = new RelayCommand(SaveAsFile, () => ActiveTab != null);
            CloseTabCommand = new RelayCommand<EditorTabViewModel>(CloseTab);
            RunCodeCommand = new RelayCommand(RunCode, () => ActiveTab != null && !IsExecuting);
            DebugCodeCommand = new RelayCommand(DebugCode, () => ActiveTab != null && !IsDebugging);
            StopExecutionCommand = new RelayCommand(StopExecution, () => IsExecuting || IsDebugging);
            StepOverCommand = new RelayCommand(StepOver, () => IsDebugging);
            StepIntoCommand = new RelayCommand(StepInto, () => IsDebugging);
            StepOutCommand = new RelayCommand(StepOut, () => IsDebugging);
            ToggleBreakpointCommand = new RelayCommand<int>(ToggleBreakpoint);
            FormatCodeCommand = new RelayCommand(FormatCode, () => ActiveTab != null);
            FindReplaceCommand = new RelayCommand(FindReplace, () => ActiveTab != null);
            GoToLineCommand = new RelayCommand(GoToLine, () => ActiveTab != null);
            BuildProjectCommand = new RelayCommand(BuildProject);
            CleanProjectCommand = new RelayCommand(CleanProject);
            OpenProjectCommand = new RelayCommand(OpenProject);
            NewProjectCommand = new RelayCommand(NewProject);
            PluginManagerCommand = new RelayCommand(OpenPluginManager);
            SettingsCommand = new RelayCommand(OpenSettings);
        }

        private void InitializeServices()
        {
            _pythonBackend.Initialize();
            _pluginManager.LoadPlugins();
            StatusText = "Ready";
        }

        private void NewFile()
        {
            var tab = new EditorTabViewModel(_syntaxHighlighting, _codeAnalysis, _codeCompletion, _refactoring)
            {
                Title = "Untitled",
                FilePath = null,
                Content = ""
            };
            Tabs.Add(tab);
            ActiveTab = tab;
        }

        private void OpenFile()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "All Files (*.*)|*.*|C# Files (*.cs)|*.cs|Python Files (*.py)|*.py|JavaScript Files (*.js)|*.js|TypeScript Files (*.ts)|*.ts|HTML Files (*.html)|*.html|CSS Files (*.css)|*.css|JSON Files (*.json)|*.json|XML Files (*.xml)|*.xml|Markdown Files (*.md)|*.md"
            };

            if (dialog.ShowDialog() == true)
            {
                var content = System.IO.File.ReadAllText(dialog.FileName);
                var tab = new EditorTabViewModel(_syntaxHighlighting, _codeAnalysis, _codeCompletion, _refactoring)
                {
                    Title = System.IO.Path.GetFileName(dialog.FileName),
                    FilePath = dialog.FileName,
                    Content = content
                };
                Tabs.Add(tab);
                ActiveTab = tab;
            }
        }

        private void SaveFile()
        {
            if (ActiveTab?.FilePath != null)
            {
                System.IO.File.WriteAllText(ActiveTab.FilePath, ActiveTab.Content);
                ActiveTab.IsDirty = false;
                StatusText = $"Saved {ActiveTab.Title}";
            }
            else
            {
                SaveAsFile();
            }
        }

        private void SaveAsFile()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "All Files (*.*)|*.*|C# Files (*.cs)|*.cs|Python Files (*.py)|*.py|JavaScript Files (*.js)|*.js|TypeScript Files (*.ts)|*.ts|HTML Files (*.html)|*.html|CSS Files (*.css)|*.css|JSON Files (*.json)|*.json|XML Files (*.xml)|*.xml|Markdown Files (*.md)|*.md"
            };

            if (dialog.ShowDialog() == true)
            {
                System.IO.File.WriteAllText(dialog.FileName, ActiveTab.Content);
                ActiveTab.FilePath = dialog.FileName;
                ActiveTab.Title = System.IO.Path.GetFileName(dialog.FileName);
                ActiveTab.IsDirty = false;
                StatusText = $"Saved {ActiveTab.Title}";
            }
        }

        private void CloseTab(EditorTabViewModel tab)
        {
            if (tab.IsDirty)
            {
                var result = System.Windows.MessageBox.Show($"Save changes to {tab.Title}?", "Unsaved Changes", System.Windows.MessageBoxButton.YesNoCancel);
                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    SaveFile();
                }
                else if (result == System.Windows.MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            Tabs.Remove(tab);
            if (ActiveTab == tab)
            {
                ActiveTab = Tabs.Count > 0 ? Tabs[0] : null;
            }
        }

        private async void RunCode()
        {
            if (ActiveTab == null) return;

            IsExecuting = true;
            StatusText = "Executing...";

            try
            {
                var result = await _execution.ExecuteCodeAsync(ActiveTab.Content, GetLanguageFromFilePath(ActiveTab.FilePath));
                OutputMessages.Add(new OutputMessage { Type = MessageType.Output, Content = result.Output });
                if (!string.IsNullOrEmpty(result.Error))
                {
                    OutputMessages.Add(new OutputMessage { Type = MessageType.Error, Content = result.Error });
                }
                StatusText = "Execution completed";
            }
            catch (Exception ex)
            {
                OutputMessages.Add(new OutputMessage { Type = MessageType.Error, Content = ex.Message });
                StatusText = "Execution failed";
            }
            finally
            {
                IsExecuting = false;
            }
        }

        private async void DebugCode()
        {
            if (ActiveTab == null) return;

            IsDebugging = true;
            StatusText = "Starting debug session...";

            try
            {
                await _debugging.StartDebugSessionAsync(ActiveTab.Content, GetLanguageFromFilePath(ActiveTab.FilePath));
                StatusText = "Debug session started";
            }
            catch (Exception ex)
            {
                OutputMessages.Add(new OutputMessage { Type = MessageType.Error, Content = ex.Message });
                StatusText = "Debug session failed";
                IsDebugging = false;
            }
        }

        private void StopExecution()
        {
            _execution.StopExecution();
            _debugging.StopDebugSession();
            IsExecuting = false;
            IsDebugging = false;
            StatusText = "Execution stopped";
        }

        private async void StepOver()
        {
            await _debugging.StepOverAsync();
        }

        private async void StepInto()
        {
            await _debugging.StepIntoAsync();
        }

        private async void StepOut()
        {
            await _debugging.StepOutAsync();
        }

        private void ToggleBreakpoint(int lineNumber)
        {
            if (ActiveTab == null) return;

            var existingBreakpoint = Breakpoints.FirstOrDefault(bp => bp.LineNumber == lineNumber);
            if (existingBreakpoint != null)
            {
                Breakpoints.Remove(existingBreakpoint);
                _debugging.RemoveBreakpoint(lineNumber);
            }
            else
            {
                var breakpoint = new BreakpointViewModel { LineNumber = lineNumber, FilePath = ActiveTab.FilePath };
                Breakpoints.Add(breakpoint);
                _debugging.AddBreakpoint(lineNumber);
            }
        }

        private async void FormatCode()
        {
            if (ActiveTab == null) return;

            try
            {
                var formattedCode = await _refactoring.FormatCodeAsync(ActiveTab.Content, GetLanguageFromFilePath(ActiveTab.FilePath));
                ActiveTab.Content = formattedCode;
                StatusText = "Code formatted";
            }
            catch (Exception ex)
            {
                OutputMessages.Add(new OutputMessage { Type = MessageType.Error, Content = ex.Message });
            }
        }

        private void FindReplace()
        {
            // Implementation for find/replace dialog
        }

        private void GoToLine()
        {
            // Implementation for go to line dialog
        }

        private async void BuildProject()
        {
            StatusText = "Building project...";
            try
            {
                var result = await _projectManager.BuildProjectAsync();
                OutputMessages.Add(new OutputMessage { Type = MessageType.Output, Content = result });
                StatusText = "Build completed";
            }
            catch (Exception ex)
            {
                OutputMessages.Add(new OutputMessage { Type = MessageType.Error, Content = ex.Message });
                StatusText = "Build failed";
            }
        }

        private async void CleanProject()
        {
            StatusText = "Cleaning project...";
            try
            {
                await _projectManager.CleanProjectAsync();
                StatusText = "Project cleaned";
            }
            catch (Exception ex)
            {
                OutputMessages.Add(new OutputMessage { Type = MessageType.Error, Content = ex.Message });
                StatusText = "Clean failed";
            }
        }

        private void OpenProject()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Project Files (*.csproj;*.pyproj;*.sln)|*.csproj;*.pyproj;*.sln|All Files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                _projectManager.OpenProject(dialog.FileName);
                StatusText = $"Opened project: {System.IO.Path.GetFileName(dialog.FileName)}";
            }
        }

        private void NewProject()
        {
            // Implementation for new project dialog
        }

        private void OpenPluginManager()
        {
            // Implementation for plugin manager dialog
        }

        private void OpenSettings()
        {
            // Implementation for settings dialog
        }

        private string GetLanguageFromFilePath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return "text";
            
            var extension = System.IO.Path.GetExtension(filePath).ToLower();
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
