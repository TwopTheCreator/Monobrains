using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Monobrains.Core;

namespace Monobrains.Services
{
    public class RefactoringService : IRefactoringService
    {
        private readonly IPythonBackendService _pythonBackend;

        public RefactoringService(IPythonBackendService pythonBackend)
        {
            _pythonBackend = pythonBackend;
        }

        public async Task<RefactoringResult> RenameSymbolAsync(string code, int position, string newName, string language)
        {
            try
            {
                return await _pythonBackend.RefactorCodeAsync(code, "rename", language);
            }
            catch (Exception ex)
            {
                return new RefactoringResult
                {
                    Success = false,
                    NewCode = code,
                    Edits = new List<TextEdit>(),
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<RefactoringResult> ExtractMethodAsync(string code, int startPosition, int endPosition, string methodName, string language)
        {
            try
            {
                return await _pythonBackend.RefactorCodeAsync(code, "extract_method", language);
            }
            catch (Exception ex)
            {
                return new RefactoringResult
                {
                    Success = false,
                    NewCode = code,
                    Edits = new List<TextEdit>(),
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<RefactoringResult> ExtractVariableAsync(string code, int startPosition, int endPosition, string variableName, string language)
        {
            try
            {
                return await _pythonBackend.RefactorCodeAsync(code, "extract_variable", language);
            }
            catch (Exception ex)
            {
                return new RefactoringResult
                {
                    Success = false,
                    NewCode = code,
                    Edits = new List<TextEdit>(),
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<RefactoringResult> InlineVariableAsync(string code, int position, string language)
        {
            try
            {
                return await _pythonBackend.RefactorCodeAsync(code, "inline_variable", language);
            }
            catch (Exception ex)
            {
                return new RefactoringResult
                {
                    Success = false,
                    NewCode = code,
                    Edits = new List<TextEdit>(),
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<RefactoringResult> MoveToFileAsync(string code, int position, string targetFile, string language)
        {
            try
            {
                return await _pythonBackend.RefactorCodeAsync(code, "move_to_file", language);
            }
            catch (Exception ex)
            {
                return new RefactoringResult
                {
                    Success = false,
                    NewCode = code,
                    Edits = new List<TextEdit>(),
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<RefactoringResult> GenerateGetterSetterAsync(string code, int position, string language)
        {
            try
            {
                return await _pythonBackend.RefactorCodeAsync(code, "generate_getter_setter", language);
            }
            catch (Exception ex)
            {
                return new RefactoringResult
                {
                    Success = false,
                    NewCode = code,
                    Edits = new List<TextEdit>(),
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<RefactoringResult> GenerateConstructorAsync(string code, int position, string language)
        {
            try
            {
                return await _pythonBackend.RefactorCodeAsync(code, "generate_constructor", language);
            }
            catch (Exception ex)
            {
                return new RefactoringResult
                {
                    Success = false,
                    NewCode = code,
                    Edits = new List<TextEdit>(),
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<RefactoringResult> GenerateOverrideAsync(string code, int position, string language)
        {
            try
            {
                return await _pythonBackend.RefactorCodeAsync(code, "generate_override", language);
            }
            catch (Exception ex)
            {
                return new RefactoringResult
                {
                    Success = false,
                    NewCode = code,
                    Edits = new List<TextEdit>(),
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<RefactoringResult> FormatCodeAsync(string code, string language)
        {
            try
            {
                var result = await _pythonBackend.FormatCodeAsync(code, language);
                return new RefactoringResult
                {
                    Success = result.Success,
                    NewCode = result.FormattedCode,
                    Edits = new List<TextEdit>(),
                    ErrorMessage = result.ErrorMessage
                };
            }
            catch (Exception ex)
            {
                return new RefactoringResult
                {
                    Success = false,
                    NewCode = code,
                    Edits = new List<TextEdit>(),
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<RefactoringResult> OrganizeImportsAsync(string code, string language)
        {
            try
            {
                return await _pythonBackend.RefactorCodeAsync(code, "organize_imports", language);
            }
            catch (Exception ex)
            {
                return new RefactoringResult
                {
                    Success = false,
                    NewCode = code,
                    Edits = new List<TextEdit>(),
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<RefactoringResult> RemoveUnusedImportsAsync(string code, string language)
        {
            try
            {
                return await _pythonBackend.RefactorCodeAsync(code, "remove_unused_imports", language);
            }
            catch (Exception ex)
            {
                return new RefactoringResult
                {
                    Success = false,
                    NewCode = code,
                    Edits = new List<TextEdit>(),
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<RefactoringResult> SortMembersAsync(string code, string language)
        {
            try
            {
                return await _pythonBackend.RefactorCodeAsync(code, "sort_members", language);
            }
            catch (Exception ex)
            {
                return new RefactoringResult
                {
                    Success = false,
                    NewCode = code,
                    Edits = new List<TextEdit>(),
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<List<RefactoringSuggestion>> GetRefactoringSuggestionsAsync(string code, int position, string language)
        {
            var suggestions = new List<RefactoringSuggestion>();

            if (language.ToLower() == "csharp")
            {
                suggestions.Add(new RefactoringSuggestion
                {
                    Title = "Extract Method",
                    Description = "Extract the selected code into a new method",
                    Kind = RefactoringKind.ExtractMethod
                });

                suggestions.Add(new RefactoringSuggestion
                {
                    Title = "Extract Variable",
                    Description = "Extract the selected expression into a new variable",
                    Kind = RefactoringKind.ExtractVariable
                });

                suggestions.Add(new RefactoringSuggestion
                {
                    Title = "Rename Symbol",
                    Description = "Rename the symbol at the current position",
                    Kind = RefactoringKind.RenameSymbol
                });
            }
            else if (language.ToLower() == "python")
            {
                suggestions.Add(new RefactoringSuggestion
                {
                    Title = "Extract Function",
                    Description = "Extract the selected code into a new function",
                    Kind = RefactoringKind.ExtractMethod
                });

                suggestions.Add(new RefactoringSuggestion
                {
                    Title = "Extract Variable",
                    Description = "Extract the selected expression into a new variable",
                    Kind = RefactoringKind.ExtractVariable
                });
            }

            return suggestions;
        }
    }

    public class RefactoringSuggestion
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public RefactoringKind Kind { get; set; }
    }

    public enum RefactoringKind
    {
        RenameSymbol,
        ExtractMethod,
        ExtractVariable,
        InlineVariable,
        MoveToFile,
        GenerateGetterSetter,
        GenerateConstructor,
        GenerateOverride,
        FormatCode,
        OrganizeImports,
        RemoveUnusedImports,
        SortMembers
    }
}
