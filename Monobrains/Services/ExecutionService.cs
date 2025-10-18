using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Monobrains.Core;

namespace Monobrains.Services
{
    public class ExecutionService : IExecutionService
    {
        private readonly IPythonBackendService _pythonBackend;
        private bool _isExecuting;
        private ExecutionMetrics _currentMetrics;
        private List<ExecutionLog> _executionLogs;

        public ExecutionService(IPythonBackendService pythonBackend)
        {
            _pythonBackend = pythonBackend;
            _executionLogs = new List<ExecutionLog>();
        }

        public async Task<ExecutionResult> ExecuteCodeAsync(string code, string language)
        {
            _isExecuting = true;
            var startTime = DateTime.Now;

            try
            {
                var result = await _pythonBackend.ExecuteCodeInSandboxAsync(code, language);
                
                _executionLogs.Add(new ExecutionLog
                {
                    Timestamp = DateTime.Now,
                    Language = language,
                    Code = code,
                    Output = result.Output,
                    Error = result.Error,
                    ExitCode = result.ExitCode,
                    ExecutionTime = result.ExecutionTime
                });

                _currentMetrics = new ExecutionMetrics
                {
                    TotalExecutions = _executionLogs.Count,
                    SuccessfulExecutions = result.ExitCode == 0 ? 1 : 0,
                    FailedExecutions = result.ExitCode != 0 ? 1 : 0,
                    AverageExecutionTime = result.ExecutionTime.TotalMilliseconds,
                    LastExecutionTime = DateTime.Now
                };

                return result;
            }
            finally
            {
                _isExecuting = false;
            }
        }

        public async Task StopExecutionAsync()
        {
            _isExecuting = false;
            await Task.CompletedTask;
        }

        public async Task<bool> IsExecutingAsync()
        {
            return _isExecuting;
        }

        public async Task<ExecutionMetrics> GetExecutionMetricsAsync()
        {
            return _currentMetrics ?? new ExecutionMetrics();
        }

        public async Task<List<ExecutionLog>> GetExecutionLogsAsync()
        {
            return _executionLogs;
        }

        public async Task<SandboxInfo> GetSandboxInfoAsync()
        {
            return new SandboxInfo
            {
                IsActive = _isExecuting,
                Language = "multi",
                MaxExecutionTime = TimeSpan.FromSeconds(30),
                MemoryLimit = 1024 * 1024 * 100, // 100MB
                CpuLimit = 1.0
            };
        }

        public async Task<bool> CreateSandboxAsync(string language)
        {
            return true;
        }

        public async Task<bool> DestroySandboxAsync()
        {
            return true;
        }
    }

    public class ExecutionMetrics
    {
        public int TotalExecutions { get; set; }
        public int SuccessfulExecutions { get; set; }
        public int FailedExecutions { get; set; }
        public double AverageExecutionTime { get; set; }
        public DateTime LastExecutionTime { get; set; }
    }

    public class ExecutionLog
    {
        public DateTime Timestamp { get; set; }
        public string Language { get; set; }
        public string Code { get; set; }
        public string Output { get; set; }
        public string Error { get; set; }
        public int ExitCode { get; set; }
        public TimeSpan ExecutionTime { get; set; }
    }

    public class SandboxInfo
    {
        public bool IsActive { get; set; }
        public string Language { get; set; }
        public TimeSpan MaxExecutionTime { get; set; }
        public long MemoryLimit { get; set; }
        public double CpuLimit { get; set; }
    }
}
