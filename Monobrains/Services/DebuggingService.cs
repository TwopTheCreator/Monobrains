using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Monobrains.Core;

namespace Monobrains.Services
{
    public class DebuggingService : IDebuggingService
    {
        private readonly IPythonBackendService _pythonBackend;
        private DebuggingSession _currentSession;
        private bool _isDebugging;

        public DebuggingService(IPythonBackendService pythonBackend)
        {
            _pythonBackend = pythonBackend;
        }

        public async Task<DebuggingSession> StartDebugSessionAsync(string code, string language)
        {
            _currentSession = await _pythonBackend.StartDebugSessionAsync(code, language);
            _isDebugging = true;
            return _currentSession;
        }

        public async Task StopDebugSessionAsync()
        {
            await _pythonBackend.StopDebugSessionAsync();
            _currentSession = null;
            _isDebugging = false;
        }

        public async Task<DebuggingStepResult> StepOverAsync()
        {
            if (!_isDebugging) return null;
            return await _pythonBackend.StepOverAsync();
        }

        public async Task<DebuggingStepResult> StepIntoAsync()
        {
            if (!_isDebugging) return null;
            return await _pythonBackend.StepIntoAsync();
        }

        public async Task<DebuggingStepResult> StepOutAsync()
        {
            if (!_isDebugging) return null;
            return await _pythonBackend.StepOutAsync();
        }

        public async Task AddBreakpointAsync(int lineNumber)
        {
            await _pythonBackend.AddBreakpointAsync(lineNumber);
        }

        public async Task RemoveBreakpointAsync(int lineNumber)
        {
            await _pythonBackend.RemoveBreakpointAsync(lineNumber);
        }

        public async Task<List<Breakpoint>> GetBreakpointsAsync()
        {
            return new List<Breakpoint>();
        }

        public async Task<List<Variable>> GetVariablesAsync()
        {
            if (!_isDebugging) return new List<Variable>();
            return await _pythonBackend.GetVariablesAsync();
        }

        public async Task<List<CallStackFrame>> GetCallStackAsync()
        {
            if (!_isDebugging) return new List<CallStackFrame>();
            return await _pythonBackend.GetCallStackAsync();
        }

        public async Task<EvaluationResult> EvaluateExpressionAsync(string expression)
        {
            return new EvaluationResult
            {
                Success = true,
                Value = expression,
                Type = "string"
            };
        }

        public async Task<bool> IsDebuggingAsync()
        {
            return _isDebugging;
        }

        public async Task<DebuggingState> GetDebuggingStateAsync()
        {
            if (!_isDebugging) return DebuggingState.Stopped;
            return _currentSession?.State ?? DebuggingState.Stopped;
        }
    }

    public class Breakpoint
    {
        public int LineNumber { get; set; }
        public string FilePath { get; set; }
        public bool IsEnabled { get; set; }
        public string Condition { get; set; }
        public int HitCount { get; set; }
    }

    public class EvaluationResult
    {
        public bool Success { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
        public string ErrorMessage { get; set; }
    }
}
