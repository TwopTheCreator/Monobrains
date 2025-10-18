using System;
using System.Threading.Tasks;
using Monobrains.Core;

namespace Monobrains.Services
{
    public class BackendServiceManager : IPythonBackendService
    {
        private readonly IPythonBackendService _pythonBackend;
        private readonly IPythonBackendService _csharpBackend;
        private IPythonBackendService _currentBackend;
        private readonly bool _useCSharpBackup;

        public BackendServiceManager(IPythonBackendService pythonBackend, IPythonBackendService csharpBackend)
        {
            _pythonBackend = pythonBackend;
            _csharpBackend = csharpBackend;
            _useCSharpBackup = ShouldUseCSharpBackup();
            _currentBackend = _useCSharpBackup ? _csharpBackend : _pythonBackend;
        }

        private bool ShouldUseCSharpBackup()
        {
            try
            {
                var pythonProcess = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = "--version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                });

                if (pythonProcess != null && pythonProcess.WaitForExit(5000))
                {
                    return false;
                }
            }
            catch
            {
                return true;
            }

            return true;
        }

        public void Initialize()
        {
            try
            {
                _currentBackend.Initialize();
            }
            catch (Exception)
            {
                if (_currentBackend == _pythonBackend)
                {
                    _currentBackend = _csharpBackend;
                    _currentBackend.Initialize();
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task InitializeAsync()
        {
            try
            {
                await _currentBackend.InitializeAsync();
            }
            catch (Exception)
            {
                if (_currentBackend == _pythonBackend)
                {
                    _currentBackend = _csharpBackend;
                    await _currentBackend.InitializeAsync();
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<string> ExecutePythonCodeAsync(string code)
        {
            try
            {
                return await _currentBackend.ExecutePythonCodeAsync(code);
            }
            catch (Exception)
            {
                if (_currentBackend == _pythonBackend)
                {
                    _currentBackend = _csharpBackend;
                    return await _currentBackend.ExecutePythonCodeAsync(code);
                }
                throw;
            }
        }

        public async Task<AnalysisResult> AnalyzeCodeAsync(string code, string language)
        {
            try
            {
                return await _currentBackend.AnalyzeCodeAsync(code, language);
            }
            catch (Exception)
            {
                if (_currentBackend == _pythonBackend)
                {
                    _currentBackend = _csharpBackend;
                    return await _currentBackend.AnalyzeCodeAsync(code, language);
                }
                throw;
            }
        }

        public async Task<ExecutionResult> ExecuteCodeInSandboxAsync(string code, string language)
        {
            try
            {
                return await _currentBackend.ExecuteCodeInSandboxAsync(code, language);
            }
            catch (Exception)
            {
                if (_currentBackend == _pythonBackend)
                {
                    _currentBackend = _csharpBackend;
                    return await _currentBackend.ExecuteCodeInSandboxAsync(code, language);
                }
                throw;
            }
        }

        public async Task<List<Diagnostic>> GetLintingResultsAsync(string code, string language)
        {
            try
            {
                return await _currentBackend.GetLintingResultsAsync(code, language);
            }
            catch (Exception)
            {
                if (_currentBackend == _pythonBackend)
                {
                    _currentBackend = _csharpBackend;
                    return await _currentBackend.GetLintingResultsAsync(code, language);
                }
                throw;
            }
        }

        public async Task<CompletionResult> GetCodeCompletionAsync(string code, int position, string language)
        {
            try
            {
                return await _currentBackend.GetCodeCompletionAsync(code, position, language);
            }
            catch (Exception)
            {
                if (_currentBackend == _pythonBackend)
                {
                    _currentBackend = _csharpBackend;
                    return await _currentBackend.GetCodeCompletionAsync(code, position, language);
                }
                throw;
            }
        }

        public async Task<DefinitionResult> GetDefinitionAsync(string code, int position, string language)
        {
            try
            {
                return await _currentBackend.GetDefinitionAsync(code, position, language);
            }
            catch (Exception)
            {
                if (_currentBackend == _pythonBackend)
                {
                    _currentBackend = _csharpBackend;
                    return await _currentBackend.GetDefinitionAsync(code, position, language);
                }
                throw;
            }
        }

        public async Task<List<ReferenceResult>> GetReferencesAsync(string code, int position, string language)
        {
            try
            {
                return await _currentBackend.GetReferencesAsync(code, position, language);
            }
            catch (Exception)
            {
                if (_currentBackend == _pythonBackend)
                {
                    _currentBackend = _csharpBackend;
                    return await _currentBackend.GetReferencesAsync(code, position, language);
                }
                throw;
            }
        }

        public async Task<RefactoringResult> RefactorCodeAsync(string code, string refactoringType, string language)
        {
            try
            {
                return await _currentBackend.RefactorCodeAsync(code, refactoringType, language);
            }
            catch (Exception)
            {
                if (_currentBackend == _pythonBackend)
                {
                    _currentBackend = _csharpBackend;
                    return await _currentBackend.RefactorCodeAsync(code, refactoringType, language);
                }
                throw;
            }
        }

        public async Task<FormattingResult> FormatCodeAsync(string code, string language)
        {
            try
            {
                return await _currentBackend.FormatCodeAsync(code, language);
            }
            catch (Exception)
            {
                if (_currentBackend == _pythonBackend)
                {
                    _currentBackend = _csharpBackend;
                    return await _currentBackend.FormatCodeAsync(code, language);
                }
                throw;
            }
        }

        public async Task<DebuggingSession> StartDebugSessionAsync(string code, string language)
        {
            try
            {
                return await _currentBackend.StartDebugSessionAsync(code, language);
            }
            catch (Exception)
            {
                if (_currentBackend == _pythonBackend)
                {
                    _currentBackend = _csharpBackend;
                    return await _currentBackend.StartDebugSessionAsync(code, language);
                }
                throw;
            }
        }

        public async Task StopDebugSessionAsync()
        {
            try
            {
                await _currentBackend.StopDebugSessionAsync();
            }
            catch (Exception)
            {
                if (_currentBackend == _pythonBackend)
                {
                    _currentBackend = _csharpBackend;
                    await _currentBackend.StopDebugSessionAsync();
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<DebuggingStepResult> StepOverAsync()
        {
            try
            {
                return await _currentBackend.StepOverAsync();
            }
            catch (Exception)
            {
                if (_currentBackend == _pythonBackend)
                {
                    _currentBackend = _csharpBackend;
                    return await _currentBackend.StepOverAsync();
                }
                throw;
            }
        }

        public async Task<DebuggingStepResult> StepIntoAsync()
        {
            try
            {
                return await _currentBackend.StepIntoAsync();
            }
            catch (Exception)
            {
                if (_currentBackend == _pythonBackend)
                {
                    _currentBackend = _csharpBackend;
                    return await _currentBackend.StepIntoAsync();
                }
                throw;
            }
        }

        public async Task<DebuggingStepResult> StepOutAsync()
        {
            try
            {
                return await _currentBackend.StepOutAsync();
            }
            catch (Exception)
            {
                if (_currentBackend == _pythonBackend)
                {
                    _currentBackend = _csharpBackend;
                    return await _currentBackend.StepOutAsync();
                }
                throw;
            }
        }

        public async Task AddBreakpointAsync(int lineNumber)
        {
            try
            {
                await _currentBackend.AddBreakpointAsync(lineNumber);
            }
            catch (Exception)
            {
                if (_currentBackend == _pythonBackend)
                {
                    _currentBackend = _csharpBackend;
                    await _currentBackend.AddBreakpointAsync(lineNumber);
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task RemoveBreakpointAsync(int lineNumber)
        {
            try
            {
                await _currentBackend.RemoveBreakpointAsync(lineNumber);
            }
            catch (Exception)
            {
                if (_currentBackend == _pythonBackend)
                {
                    _currentBackend = _csharpBackend;
                    await _currentBackend.RemoveBreakpointAsync(lineNumber);
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<List<Variable>> GetVariablesAsync()
        {
            try
            {
                return await _currentBackend.GetVariablesAsync();
            }
            catch (Exception)
            {
                if (_currentBackend == _pythonBackend)
                {
                    _currentBackend = _csharpBackend;
                    return await _currentBackend.GetVariablesAsync();
                }
                throw;
            }
        }

        public async Task<List<CallStackFrame>> GetCallStackAsync()
        {
            try
            {
                return await _currentBackend.GetCallStackAsync();
            }
            catch (Exception)
            {
                if (_currentBackend == _pythonBackend)
                {
                    _currentBackend = _csharpBackend;
                    return await _currentBackend.GetCallStackAsync();
                }
                throw;
            }
        }

        public async Task<bool> LoadPluginAsync(string pluginPath)
        {
            try
            {
                return await _currentBackend.LoadPluginAsync(pluginPath);
            }
            catch (Exception)
            {
                if (_currentBackend == _pythonBackend)
                {
                    _currentBackend = _csharpBackend;
                    return await _currentBackend.LoadPluginAsync(pluginPath);
                }
                throw;
            }
        }

        public async Task<bool> UnloadPluginAsync(string pluginName)
        {
            try
            {
                return await _currentBackend.UnloadPluginAsync(pluginName);
            }
            catch (Exception)
            {
                if (_currentBackend == _pythonBackend)
                {
                    _currentBackend = _csharpBackend;
                    return await _currentBackend.UnloadPluginAsync(pluginName);
                }
                throw;
            }
        }

        public async Task<List<PluginInfo>> GetLoadedPluginsAsync()
        {
            try
            {
                return await _currentBackend.GetLoadedPluginsAsync();
            }
            catch (Exception)
            {
                if (_currentBackend == _pythonBackend)
                {
                    _currentBackend = _csharpBackend;
                    return await _currentBackend.GetLoadedPluginsAsync();
                }
                throw;
            }
        }

        public string GetCurrentBackendType()
        {
            return _currentBackend == _pythonBackend ? "Python" : "C#";
        }

        public bool IsUsingBackup()
        {
            return _currentBackend == _csharpBackend;
        }

        public void SwitchToPythonBackend()
        {
            _currentBackend = _pythonBackend;
        }

        public void SwitchToCSharpBackend()
        {
            _currentBackend = _csharpBackend;
        }
    }
}
