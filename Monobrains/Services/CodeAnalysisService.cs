using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Monobrains.Core;

namespace Monobrains.Services
{
    public class CodeAnalysisService : ICodeAnalysisService
    {
        private readonly IPythonBackendService _pythonBackend;

        public CodeAnalysisService(IPythonBackendService pythonBackend)
        {
            _pythonBackend = pythonBackend;
        }

        public async Task<List<Diagnostic>> AnalyzeCodeAsync(string code, string language)
        {
            var result = await _pythonBackend.AnalyzeCodeAsync(code, language);
            return result.Diagnostics;
        }

        public async Task<SymbolTable> GetSymbolTableAsync(string code, string language)
        {
            var result = await _pythonBackend.AnalyzeCodeAsync(code, language);
            return result.SymbolTable;
        }

        public async Task<List<CodeIssue>> GetCodeIssuesAsync(string code, string language)
        {
            var diagnostics = await AnalyzeCodeAsync(code, language);
            var issues = new List<CodeIssue>();
            
            foreach (var diagnostic in diagnostics)
            {
                issues.Add(new CodeIssue
                {
                    Message = diagnostic.Message,
                    Severity = diagnostic.Severity,
                    LineNumber = diagnostic.LineNumber,
                    ColumnNumber = diagnostic.ColumnNumber,
                    FilePath = diagnostic.FilePath,
                    Code = diagnostic.Code,
                    Description = diagnostic.Description
                });
            }
            
            return issues;
        }

        public async Task<ComplexityMetrics> GetComplexityMetricsAsync(string code, string language)
        {
            var result = await _pythonBackend.AnalyzeCodeAsync(code, language);
            return result.Complexity;
        }

        public async Task<List<CodeSmell>> DetectCodeSmellsAsync(string code, string language)
        {
            var result = await _pythonBackend.AnalyzeCodeAsync(code, language);
            return result.CodeSmells;
        }

        public async Task<DependencyGraph> GetDependencyGraphAsync(string code, string language)
        {
            return new DependencyGraph
            {
                Nodes = new List<DependencyNode>(),
                Edges = new List<DependencyEdge>()
            };
        }

        public async Task<List<SecurityIssue>> GetSecurityIssuesAsync(string code, string language)
        {
            var issues = new List<SecurityIssue>();
            
            if (language.ToLower() == "python")
            {
                if (code.Contains("eval("))
                {
                    issues.Add(new SecurityIssue
                    {
                        Type = "Code Injection",
                        Severity = SecuritySeverity.High,
                        LineNumber = GetLineNumber(code, "eval("),
                        Description = "Use of eval() can lead to code injection vulnerabilities",
                        Recommendation = "Use safer alternatives like ast.literal_eval() or json.loads()"
                    });
                }
                
                if (code.Contains("exec("))
                {
                    issues.Add(new SecurityIssue
                    {
                        Type = "Code Injection",
                        Severity = SecuritySeverity.High,
                        LineNumber = GetLineNumber(code, "exec("),
                        Description = "Use of exec() can lead to code injection vulnerabilities",
                        Recommendation = "Avoid using exec() with user input"
                    });
                }
            }
            
            return issues;
        }

        public async Task<PerformanceAnalysis> AnalyzePerformanceAsync(string code, string language)
        {
            return new PerformanceAnalysis
            {
                ComplexityScore = CalculateComplexityScore(code),
                PerformanceIssues = new List<PerformanceIssue>(),
                Recommendations = new List<string>()
            };
        }

        private int GetLineNumber(string code, string searchText)
        {
            var lines = code.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(searchText))
                {
                    return i + 1;
                }
            }
            return 0;
        }

        private int CalculateComplexityScore(string code)
        {
            var lines = code.Split('\n');
            int complexity = 0;
            
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith("if ") || trimmedLine.StartsWith("elif ") ||
                    trimmedLine.StartsWith("for ") || trimmedLine.StartsWith("while ") ||
                    trimmedLine.Contains(" and ") || trimmedLine.Contains(" or "))
                {
                    complexity++;
                }
            }
            
            return complexity;
        }
    }

    public class CodeIssue
    {
        public string Message { get; set; }
        public DiagnosticSeverity Severity { get; set; }
        public int LineNumber { get; set; }
        public int ColumnNumber { get; set; }
        public string FilePath { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
    }

    public class DependencyGraph
    {
        public List<DependencyNode> Nodes { get; set; }
        public List<DependencyEdge> Edges { get; set; }
    }

    public class DependencyNode
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string FilePath { get; set; }
    }

    public class DependencyEdge
    {
        public string SourceId { get; set; }
        public string TargetId { get; set; }
        public string Relationship { get; set; }
    }

    public class SecurityIssue
    {
        public string Type { get; set; }
        public SecuritySeverity Severity { get; set; }
        public int LineNumber { get; set; }
        public string Description { get; set; }
        public string Recommendation { get; set; }
    }

    public enum SecuritySeverity
    {
        Low,
        Medium,
        High,
        Critical
    }

    public class PerformanceAnalysis
    {
        public int ComplexityScore { get; set; }
        public List<PerformanceIssue> PerformanceIssues { get; set; }
        public List<string> Recommendations { get; set; }
    }

    public class PerformanceIssue
    {
        public string Type { get; set; }
        public int LineNumber { get; set; }
        public string Description { get; set; }
        public string Recommendation { get; set; }
    }
}
