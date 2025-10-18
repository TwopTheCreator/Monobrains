using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Monobrains.Core
{
    public interface IPythonBackendService
    {
        Task InitializeAsync();
        Task<string> ExecutePythonCodeAsync(string code);
        Task<AnalysisResult> AnalyzeCodeAsync(string code, string language);
        Task<ExecutionResult> ExecuteCodeInSandboxAsync(string code, string language);
        Task<List<Diagnostic>> GetLintingResultsAsync(string code, string language);
        Task<CompletionResult> GetCodeCompletionAsync(string code, int position, string language);
        Task<DefinitionResult> GetDefinitionAsync(string code, int position, string language);
        Task<List<ReferenceResult>> GetReferencesAsync(string code, int position, string language);
        Task<RefactoringResult> RefactorCodeAsync(string code, string refactoringType, string language);
        Task<FormattingResult> FormatCodeAsync(string code, string language);
        Task<DebuggingSession> StartDebugSessionAsync(string code, string language);
        Task StopDebugSessionAsync();
        Task<DebuggingStepResult> StepOverAsync();
        Task<DebuggingStepResult> StepIntoAsync();
        Task<DebuggingStepResult> StepOutAsync();
        Task AddBreakpointAsync(int lineNumber);
        Task RemoveBreakpointAsync(int lineNumber);
        Task<List<Variable>> GetVariablesAsync();
        Task<List<CallStackFrame>> GetCallStackAsync();
        Task<bool> LoadPluginAsync(string pluginPath);
        Task<bool> UnloadPluginAsync(string pluginName);
        Task<List<PluginInfo>> GetLoadedPluginsAsync();
        void Initialize();
    }

    public interface ICodeAnalysisService
    {
        Task<List<Diagnostic>> AnalyzeCodeAsync(string code, string language);
        Task<SymbolTable> GetSymbolTableAsync(string code, string language);
        Task<List<CodeIssue>> GetCodeIssuesAsync(string code, string language);
        Task<ComplexityMetrics> GetComplexityMetricsAsync(string code, string language);
        Task<List<CodeSmell>> DetectCodeSmellsAsync(string code, string language);
        Task<DependencyGraph> GetDependencyGraphAsync(string code, string language);
        Task<List<SecurityIssue>> GetSecurityIssuesAsync(string code, string language);
        Task<PerformanceAnalysis> AnalyzePerformanceAsync(string code, string language);
    }

    public interface IDebuggingService
    {
        Task<DebuggingSession> StartDebugSessionAsync(string code, string language);
        Task StopDebugSessionAsync();
        Task<DebuggingStepResult> StepOverAsync();
        Task<DebuggingStepResult> StepIntoAsync();
        Task<DebuggingStepResult> StepOutAsync();
        Task AddBreakpointAsync(int lineNumber);
        Task RemoveBreakpointAsync(int lineNumber);
        Task<List<Breakpoint>> GetBreakpointsAsync();
        Task<List<Variable>> GetVariablesAsync();
        Task<List<CallStackFrame>> GetCallStackAsync();
        Task<EvaluationResult> EvaluateExpressionAsync(string expression);
        Task<bool> IsDebuggingAsync();
        Task<DebuggingState> GetDebuggingStateAsync();
    }

    public interface IExecutionService
    {
        Task<ExecutionResult> ExecuteCodeAsync(string code, string language);
        Task StopExecutionAsync();
        Task<bool> IsExecutingAsync();
        Task<ExecutionMetrics> GetExecutionMetricsAsync();
        Task<List<ExecutionLog>> GetExecutionLogsAsync();
        Task<SandboxInfo> GetSandboxInfoAsync();
        Task<bool> CreateSandboxAsync(string language);
        Task<bool> DestroySandboxAsync();
    }

    public interface IPluginManager
    {
        Task<bool> LoadPluginAsync(string pluginPath);
        Task<bool> UnloadPluginAsync(string pluginName);
        Task<List<PluginInfo>> GetLoadedPluginsAsync();
        Task<List<PluginInfo>> GetAvailablePluginsAsync();
        Task<bool> InstallPluginAsync(string pluginName, string version);
        Task<bool> UninstallPluginAsync(string pluginName);
        Task<PluginConfiguration> GetPluginConfigurationAsync(string pluginName);
        Task<bool> SetPluginConfigurationAsync(string pluginName, PluginConfiguration config);
        void LoadPlugins();
    }

    public interface ISyntaxHighlightingService
    {
        Task<SyntaxHighlightingResult> HighlightCodeAsync(string code, string language);
        Task<List<LanguageDefinition>> GetSupportedLanguagesAsync();
        Task<LanguageDefinition> GetLanguageDefinitionAsync(string language);
        Task<bool> RegisterLanguageAsync(LanguageDefinition language);
        Task<bool> UnregisterLanguageAsync(string language);
        Task<Theme> GetCurrentThemeAsync();
        Task<bool> SetThemeAsync(Theme theme);
        Task<List<Theme>> GetAvailableThemesAsync();
    }

    public interface IFileManagerService
    {
        Task<bool> SaveFileAsync(string filePath, string content);
        Task<string> LoadFileAsync(string filePath);
        Task<bool> FileExistsAsync(string filePath);
        Task<FileInfo> GetFileInfoAsync(string filePath);
        Task<List<FileInfo>> GetDirectoryContentsAsync(string directoryPath);
        Task<bool> CreateDirectoryAsync(string directoryPath);
        Task<bool> DeleteFileAsync(string filePath);
        Task<bool> DeleteDirectoryAsync(string directoryPath);
        Task<bool> CopyFileAsync(string sourcePath, string destinationPath);
        Task<bool> MoveFileAsync(string sourcePath, string destinationPath);
        Task<List<FileInfo>> SearchFilesAsync(string directoryPath, string pattern);
        Task<FileWatcher> WatchFileAsync(string filePath, Action<FileChangeEvent> onChange);
        Task StopWatchingFileAsync(string filePath);
    }

    public interface IProjectManagerService
    {
        Task<bool> CreateProjectAsync(string projectPath, ProjectTemplate template);
        Task<bool> OpenProjectAsync(string projectPath);
        Task<bool> SaveProjectAsync();
        Task<bool> CloseProjectAsync();
        Task<ProjectInfo> GetProjectInfoAsync();
        Task<List<ProjectFile>> GetProjectFilesAsync();
        Task<bool> AddFileToProjectAsync(string filePath);
        Task<bool> RemoveFileFromProjectAsync(string filePath);
        Task<BuildResult> BuildProjectAsync();
        Task<BuildResult> CleanProjectAsync();
        Task<BuildResult> RebuildProjectAsync();
        Task<List<BuildError>> GetBuildErrorsAsync();
        Task<List<BuildWarning>> GetBuildWarningsAsync();
        Task<ProjectConfiguration> GetProjectConfigurationAsync();
        Task<bool> SetProjectConfigurationAsync(ProjectConfiguration config);
        Task<List<ProjectTemplate>> GetAvailableTemplatesAsync();
    }

    public interface ICodeCompletionService
    {
        Task<List<CodeCompletionItem>> GetCompletionsAsync(string code, int position, string language);
        Task<DefinitionResult> GetDefinitionAsync(string code, int position, string language);
        Task<List<ReferenceResult>> GetReferencesAsync(string code, int position, string language);
        Task<SymbolInfo> GetSymbolAtPositionAsync(string code, int position, string language);
        Task<List<SignatureHelp>> GetSignatureHelpAsync(string code, int position, string language);
        Task<List<HoverInfo>> GetHoverInfoAsync(string code, int position, string language);
        Task<List<CodeAction>> GetCodeActionsAsync(string code, int position, string language);
        Task<bool> RegisterCompletionProviderAsync(ICompletionProvider provider);
        Task<bool> UnregisterCompletionProviderAsync(string providerName);
    }

    public interface IRefactoringService
    {
        Task<RefactoringResult> RenameSymbolAsync(string code, int position, string newName, string language);
        Task<RefactoringResult> ExtractMethodAsync(string code, int startPosition, int endPosition, string methodName, string language);
        Task<RefactoringResult> ExtractVariableAsync(string code, int startPosition, int endPosition, string variableName, string language);
        Task<RefactoringResult> InlineVariableAsync(string code, int position, string language);
        Task<RefactoringResult> MoveToFileAsync(string code, int position, string targetFile, string language);
        Task<RefactoringResult> GenerateGetterSetterAsync(string code, int position, string language);
        Task<RefactoringResult> GenerateConstructorAsync(string code, int position, string language);
        Task<RefactoringResult> GenerateOverrideAsync(string code, int position, string language);
        Task<RefactoringResult> FormatCodeAsync(string code, string language);
        Task<RefactoringResult> OrganizeImportsAsync(string code, string language);
        Task<RefactoringResult> RemoveUnusedImportsAsync(string code, string language);
        Task<RefactoringResult> SortMembersAsync(string code, string language);
        Task<List<RefactoringSuggestion>> GetRefactoringSuggestionsAsync(string code, int position, string language);
    }

    public interface IMainWindowViewModel
    {
        System.Collections.ObjectModel.ObservableCollection<EditorTabViewModel> Tabs { get; }
        EditorTabViewModel ActiveTab { get; set; }
        string StatusText { get; set; }
        bool IsDebugging { get; set; }
        bool IsExecuting { get; set; }
        System.Collections.ObjectModel.ObservableCollection<OutputMessage> OutputMessages { get; }
        System.Collections.ObjectModel.ObservableCollection<BreakpointViewModel> Breakpoints { get; }
        System.Collections.ObjectModel.ObservableCollection<VariableViewModel> Variables { get; }
        System.Collections.ObjectModel.ObservableCollection<CallStackItem> CallStack { get; }
    }
}
