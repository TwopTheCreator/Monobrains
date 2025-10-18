using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using Monobrains.Core;

namespace Monobrains.Services
{
    public class PythonBackendService : IPythonBackendService
    {
        private Process _pythonProcess;
        private bool _isInitialized;
        private readonly string _pythonExecutable;
        private readonly string _backendScriptPath;

        public PythonBackendService()
        {
            _pythonExecutable = FindPythonExecutable();
            _backendScriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backend", "monobrains_backend.py");
        }

        public void Initialize()
        {
            if (!_isInitialized)
            {
                StartPythonBackend();
                _isInitialized = true;
            }
        }

        public async Task InitializeAsync()
        {
            Initialize();
            await Task.CompletedTask;
        }

        private string FindPythonExecutable()
        {
            var possiblePaths = new[]
            {
                "python",
                "python3",
                "py",
                @"C:\Python39\python.exe",
                @"C:\Python310\python.exe",
                @"C:\Python311\python.exe",
                @"C:\Python312\python.exe",
                @"/usr/bin/python3",
                @"/usr/local/bin/python3"
            };

            foreach (var path in possiblePaths)
            {
                try
                {
                    var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = path,
                        Arguments = "--version",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    });

                    if (process != null && process.WaitForExit(5000))
                    {
                        return path;
                    }
                }
                catch
                {
                    continue;
                }
            }

            throw new InvalidOperationException("Python executable not found. Please install Python 3.9 or later.");
        }

        private void StartPythonBackend()
        {
            if (_pythonProcess != null && !_pythonProcess.HasExited)
            {
                return;
            }

            _pythonProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _pythonExecutable,
                    Arguments = $"\"{_backendScriptPath}\"",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            _pythonProcess.Start();
        }

        private async Task<T> SendCommandAsync<T>(object command)
        {
            if (_pythonProcess == null || _pythonProcess.HasExited)
            {
                StartPythonBackend();
            }

            var commandJson = JsonSerializer.Serialize(command);
            await _pythonProcess.StandardInput.WriteLineAsync(commandJson);
            await _pythonProcess.StandardInput.FlushAsync();

            var response = await _pythonProcess.StandardOutput.ReadLineAsync();
            return JsonSerializer.Deserialize<T>(response);
        }

        public async Task<string> ExecutePythonCodeAsync(string code)
        {
            var command = new { action = "execute_python", code = code };
            var result = await SendCommandAsync<ExecutionResult>(command);
            return result.Output;
        }

        public async Task<AnalysisResult> AnalyzeCodeAsync(string code, string language)
        {
            var command = new { action = "analyze_code", code = code, language = language };
            return await SendCommandAsync<AnalysisResult>(command);
        }

        public async Task<ExecutionResult> ExecuteCodeInSandboxAsync(string code, string language)
        {
            var command = new { action = "execute_sandbox", code = code, language = language };
            return await SendCommandAsync<ExecutionResult>(command);
        }

        public async Task<List<Diagnostic>> GetLintingResultsAsync(string code, string language)
        {
            var command = new { action = "lint_code", code = code, language = language };
            var result = await SendCommandAsync<LintingResult>(command);
            return result.Diagnostics;
        }

        public async Task<CompletionResult> GetCodeCompletionAsync(string code, int position, string language)
        {
            var command = new { action = "get_completion", code = code, position = position, language = language };
            return await SendCommandAsync<CompletionResult>(command);
        }

        public async Task<DefinitionResult> GetDefinitionAsync(string code, int position, string language)
        {
            var command = new { action = "get_definition", code = code, position = position, language = language };
            return await SendCommandAsync<DefinitionResult>(command);
        }

        public async Task<List<ReferenceResult>> GetReferencesAsync(string code, int position, string language)
        {
            var command = new { action = "get_references", code = code, position = position, language = language };
            var result = await SendCommandAsync<ReferencesResult>(command);
            return result.References;
        }

        public async Task<RefactoringResult> RefactorCodeAsync(string code, string refactoringType, string language)
        {
            var command = new { action = "refactor_code", code = code, refactoring_type = refactoringType, language = language };
            return await SendCommandAsync<RefactoringResult>(command);
        }

        public async Task<FormattingResult> FormatCodeAsync(string code, string language)
        {
            var command = new { action = "format_code", code = code, language = language };
            return await SendCommandAsync<FormattingResult>(command);
        }

        public async Task<DebuggingSession> StartDebugSessionAsync(string code, string language)
        {
            var command = new { action = "start_debug", code = code, language = language };
            return await SendCommandAsync<DebuggingSession>(command);
        }

        public async Task StopDebugSessionAsync()
        {
            var command = new { action = "stop_debug" };
            await SendCommandAsync<object>(command);
        }

        public async Task<DebuggingStepResult> StepOverAsync()
        {
            var command = new { action = "step_over" };
            return await SendCommandAsync<DebuggingStepResult>(command);
        }

        public async Task<DebuggingStepResult> StepIntoAsync()
        {
            var command = new { action = "step_into" };
            return await SendCommandAsync<DebuggingStepResult>(command);
        }

        public async Task<DebuggingStepResult> StepOutAsync()
        {
            var command = new { action = "step_out" };
            return await SendCommandAsync<DebuggingStepResult>(command);
        }

        public async Task AddBreakpointAsync(int lineNumber)
        {
            var command = new { action = "add_breakpoint", line = lineNumber };
            await SendCommandAsync<object>(command);
        }

        public async Task RemoveBreakpointAsync(int lineNumber)
        {
            var command = new { action = "remove_breakpoint", line = lineNumber };
            await SendCommandAsync<object>(command);
        }

        public async Task<List<Variable>> GetVariablesAsync()
        {
            var command = new { action = "get_variables" };
            var result = await SendCommandAsync<VariablesResult>(command);
            return result.Variables;
        }

        public async Task<List<CallStackFrame>> GetCallStackAsync()
        {
            var command = new { action = "get_call_stack" };
            var result = await SendCommandAsync<CallStackResult>(command);
            return result.Frames;
        }

        public async Task<bool> LoadPluginAsync(string pluginPath)
        {
            var command = new { action = "load_plugin", path = pluginPath };
            var result = await SendCommandAsync<PluginResult>(command);
            return result.Success;
        }

        public async Task<bool> UnloadPluginAsync(string pluginName)
        {
            var command = new { action = "unload_plugin", name = pluginName };
            var result = await SendCommandAsync<PluginResult>(command);
            return result.Success;
        }

        public async Task<List<PluginInfo>> GetLoadedPluginsAsync()
        {
            var command = new { action = "get_loaded_plugins" };
            var result = await SendCommandAsync<PluginsResult>(command);
            return result.Plugins;
        }

        public void Dispose()
        {
            _pythonProcess?.Kill();
            _pythonProcess?.Dispose();
        }
    }

    public class ExecutionResult
    {
        public string Output { get; set; }
        public string Error { get; set; }
        public int ExitCode { get; set; }
        public TimeSpan ExecutionTime { get; set; }
    }

    public class AnalysisResult
    {
        public List<Diagnostic> Diagnostics { get; set; }
        public SymbolTable SymbolTable { get; set; }
        public ComplexityMetrics Complexity { get; set; }
        public List<CodeSmell> CodeSmells { get; set; }
    }

    public class LintingResult
    {
        public List<Diagnostic> Diagnostics { get; set; }
    }

    public class CompletionResult
    {
        public List<CodeCompletionItem> Items { get; set; }
        public int StartPosition { get; set; }
        public int EndPosition { get; set; }
    }

    public class DefinitionResult
    {
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
        public int ColumnNumber { get; set; }
        public int Position { get; set; }
    }

    public class ReferencesResult
    {
        public List<ReferenceResult> References { get; set; }
    }

    public class ReferenceResult
    {
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
        public int ColumnNumber { get; set; }
        public ReferenceKind Kind { get; set; }
    }

    public enum ReferenceKind
    {
        Read,
        Write,
        Declaration,
        Definition
    }

    public class RefactoringResult
    {
        public bool Success { get; set; }
        public string NewCode { get; set; }
        public List<TextEdit> Edits { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class TextEdit
    {
        public int StartPosition { get; set; }
        public int EndPosition { get; set; }
        public string NewText { get; set; }
    }

    public class FormattingResult
    {
        public bool Success { get; set; }
        public string FormattedCode { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class DebuggingSession
    {
        public string SessionId { get; set; }
        public bool IsActive { get; set; }
        public DebuggingState State { get; set; }
    }

    public class DebuggingStepResult
    {
        public bool Success { get; set; }
        public int CurrentLine { get; set; }
        public string CurrentFile { get; set; }
        public List<Variable> Variables { get; set; }
        public List<CallStackFrame> CallStack { get; set; }
    }

    public class Variable
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
        public bool IsExpanded { get; set; }
        public List<Variable> Children { get; set; }
    }

    public class CallStackFrame
    {
        public string MethodName { get; set; }
        public string FileName { get; set; }
        public int LineNumber { get; set; }
        public string ModuleName { get; set; }
        public List<Variable> LocalVariables { get; set; }
    }

    public class PluginResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class PluginsResult
    {
        public List<PluginInfo> Plugins { get; set; }
    }

    public class PluginInfo
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public List<string> Dependencies { get; set; }
        public bool IsLoaded { get; set; }
    }

    public class VariablesResult
    {
        public List<Variable> Variables { get; set; }
    }

    public class CallStackResult
    {
        public List<CallStackFrame> Frames { get; set; }
    }

    public enum DebuggingState
    {
        Stopped,
        Running,
        Paused,
        Stepping
    }

    public class SymbolTable
    {
        public List<Symbol> Symbols { get; set; }
    }

    public class Symbol
    {
        public string Name { get; set; }
        public SymbolKind Kind { get; set; }
        public int LineNumber { get; set; }
        public int ColumnNumber { get; set; }
        public string Type { get; set; }
        public string Documentation { get; set; }
    }

    public enum SymbolKind
    {
        Variable,
        Function,
        Class,
        Method,
        Property,
        Field,
        Namespace,
        Module,
        Interface,
        Enum,
        Struct
    }

    public class ComplexityMetrics
    {
        public int CyclomaticComplexity { get; set; }
        public int CognitiveComplexity { get; set; }
        public int LinesOfCode { get; set; }
        public int CommentLines { get; set; }
        public double CommentDensity { get; set; }
    }

    public class CodeSmell
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int LineNumber { get; set; }
        public CodeSmellSeverity Severity { get; set; }
        public string Suggestion { get; set; }
    }

    public enum CodeSmellSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }
}
