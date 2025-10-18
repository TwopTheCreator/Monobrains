using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Monobrains.Core;

namespace Monobrains.Services
{
    public class SyntaxHighlightingService : ISyntaxHighlightingService
    {
        private readonly Dictionary<string, LanguageDefinition> _languages;
        private Theme _currentTheme;

        public SyntaxHighlightingService()
        {
            _languages = new Dictionary<string, LanguageDefinition>();
            _currentTheme = new Theme { Name = "Dark", IsDark = true };
            InitializeDefaultLanguages();
        }

        private void InitializeDefaultLanguages()
        {
            RegisterLanguage(new LanguageDefinition
            {
                Name = "csharp",
                DisplayName = "C#",
                FileExtensions = new List<string> { ".cs" },
                Keywords = new List<string> { "public", "private", "protected", "internal", "static", "readonly", "const", "virtual", "override", "abstract", "sealed", "class", "interface", "struct", "enum", "namespace", "using", "if", "else", "for", "foreach", "while", "do", "switch", "case", "default", "break", "continue", "return", "throw", "try", "catch", "finally", "var", "new", "this", "base", "null", "true", "false" },
                CommentPrefix = "//",
                MultiLineCommentStart = "/*",
                MultiLineCommentEnd = "*/",
                StringDelimiters = new List<string> { "\"", "'" }
            });

            RegisterLanguage(new LanguageDefinition
            {
                Name = "python",
                DisplayName = "Python",
                FileExtensions = new List<string> { ".py" },
                Keywords = new List<string> { "def", "class", "if", "else", "elif", "for", "while", "try", "except", "finally", "with", "import", "from", "return", "yield", "lambda", "and", "or", "not", "in", "is", "True", "False", "None" },
                CommentPrefix = "#",
                StringDelimiters = new List<string> { "\"", "'", "\"\"\"", "'''" }
            });

            RegisterLanguage(new LanguageDefinition
            {
                Name = "javascript",
                DisplayName = "JavaScript",
                FileExtensions = new List<string> { ".js" },
                Keywords = new List<string> { "var", "let", "const", "function", "class", "if", "else", "for", "while", "do", "switch", "case", "default", "break", "continue", "return", "throw", "try", "catch", "finally", "new", "this", "null", "undefined", "true", "false", "typeof", "instanceof", "in", "of" },
                CommentPrefix = "//",
                MultiLineCommentStart = "/*",
                MultiLineCommentEnd = "*/",
                StringDelimiters = new List<string> { "\"", "'", "`" }
            });

            RegisterLanguage(new LanguageDefinition
            {
                Name = "typescript",
                DisplayName = "TypeScript",
                FileExtensions = new List<string> { ".ts" },
                Keywords = new List<string> { "var", "let", "const", "function", "class", "interface", "type", "enum", "namespace", "module", "import", "export", "if", "else", "for", "while", "do", "switch", "case", "default", "break", "continue", "return", "throw", "try", "catch", "finally", "new", "this", "null", "undefined", "true", "false", "typeof", "instanceof", "in", "of", "as", "is" },
                CommentPrefix = "//",
                MultiLineCommentStart = "/*",
                MultiLineCommentEnd = "*/",
                StringDelimiters = new List<string> { "\"", "'", "`" }
            });

            RegisterLanguage(new LanguageDefinition
            {
                Name = "html",
                DisplayName = "HTML",
                FileExtensions = new List<string> { ".html", ".htm" },
                Keywords = new List<string> { "html", "head", "body", "title", "meta", "link", "script", "style", "div", "span", "p", "h1", "h2", "h3", "h4", "h5", "h6", "a", "img", "ul", "ol", "li", "table", "tr", "td", "th", "form", "input", "button", "textarea", "select", "option" },
                CommentPrefix = "<!--",
                CommentSuffix = "-->",
                StringDelimiters = new List<string> { "\"", "'" }
            });

            RegisterLanguage(new LanguageDefinition
            {
                Name = "css",
                DisplayName = "CSS",
                FileExtensions = new List<string> { ".css" },
                Keywords = new List<string> { "color", "background", "font-size", "font-family", "margin", "padding", "border", "width", "height", "display", "position", "top", "left", "right", "bottom", "z-index", "opacity", "visibility", "overflow", "text-align", "line-height", "text-decoration", "font-weight", "font-style" },
                CommentPrefix = "/*",
                CommentSuffix = "*/",
                StringDelimiters = new List<string> { "\"", "'" }
            });

            RegisterLanguage(new LanguageDefinition
            {
                Name = "json",
                DisplayName = "JSON",
                FileExtensions = new List<string> { ".json" },
                Keywords = new List<string> { "true", "false", "null" },
                StringDelimiters = new List<string> { "\"" }
            });

            RegisterLanguage(new LanguageDefinition
            {
                Name = "xml",
                DisplayName = "XML",
                FileExtensions = new List<string> { ".xml" },
                Keywords = new List<string> { },
                CommentPrefix = "<!--",
                CommentSuffix = "-->",
                StringDelimiters = new List<string> { "\"", "'" }
            });

            RegisterLanguage(new LanguageDefinition
            {
                Name = "markdown",
                DisplayName = "Markdown",
                FileExtensions = new List<string> { ".md" },
                Keywords = new List<string> { "#", "##", "###", "####", "#####", "######", "**", "*", "`", "```", ">", "-", "1.", "[", "]", "(", ")", "![" },
                StringDelimiters = new List<string> { "`", "```" }
            });
        }

        public async Task<SyntaxHighlightingResult> HighlightCodeAsync(string code, string language)
        {
            var languageDef = GetLanguageDefinition(language);
            if (languageDef == null)
            {
                return new SyntaxHighlightingResult
                {
                    HighlightedCode = code,
                    Tokens = new List<SyntaxToken>()
                };
            }

            var tokens = new List<SyntaxToken>();
            var lines = code.Split('\n');
            
            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                var line = lines[lineIndex];
                var lineTokens = HighlightLine(line, languageDef);
                
                foreach (var token in lineTokens)
                {
                    token.LineNumber = lineIndex + 1;
                    tokens.Add(token);
                }
            }

            return new SyntaxHighlightingResult
            {
                HighlightedCode = code,
                Tokens = tokens
            };
        }

        private List<SyntaxToken> HighlightLine(string line, LanguageDefinition languageDef)
        {
            var tokens = new List<SyntaxToken>();
            var currentToken = "";
            var currentType = SyntaxTokenType.Text;
            var position = 0;

            while (position < line.Length)
            {
                var currentChar = line[position];
                var nextChar = position + 1 < line.Length ? line[position + 1] : '\0';

                if (char.IsWhiteSpace(currentChar))
                {
                    if (!string.IsNullOrEmpty(currentToken))
                    {
                        tokens.Add(new SyntaxToken
                        {
                            Text = currentToken,
                            Type = currentType,
                            StartPosition = position - currentToken.Length,
                            Length = currentToken.Length
                        });
                        currentToken = "";
                    }
                    
                    tokens.Add(new SyntaxToken
                    {
                        Text = currentChar.ToString(),
                        Type = SyntaxTokenType.Whitespace,
                        StartPosition = position,
                        Length = 1
                    });
                    currentType = SyntaxTokenType.Text;
                }
                else if (IsCommentStart(line, position, languageDef))
                {
                    if (!string.IsNullOrEmpty(currentToken))
                    {
                        tokens.Add(new SyntaxToken
                        {
                            Text = currentToken,
                            Type = currentType,
                            StartPosition = position - currentToken.Length,
                            Length = currentToken.Length
                        });
                        currentToken = "";
                    }

                    var commentText = line.Substring(position);
                    tokens.Add(new SyntaxToken
                    {
                        Text = commentText,
                        Type = SyntaxTokenType.Comment,
                        StartPosition = position,
                        Length = commentText.Length
                    });
                    break;
                }
                else if (IsStringStart(currentChar, languageDef))
                {
                    if (!string.IsNullOrEmpty(currentToken))
                    {
                        tokens.Add(new SyntaxToken
                        {
                            Text = currentToken,
                            Type = currentType,
                            StartPosition = position - currentToken.Length,
                            Length = currentToken.Length
                        });
                        currentToken = "";
                    }

                    var stringText = ExtractString(line, position, languageDef);
                    tokens.Add(new SyntaxToken
                    {
                        Text = stringText,
                        Type = SyntaxTokenType.String,
                        StartPosition = position,
                        Length = stringText.Length
                    });
                    position += stringText.Length - 1;
                }
                else if (char.IsLetter(currentChar) || currentChar == '_')
                {
                    if (currentType != SyntaxTokenType.Identifier)
                    {
                        if (!string.IsNullOrEmpty(currentToken))
                        {
                            tokens.Add(new SyntaxToken
                            {
                                Text = currentToken,
                                Type = currentType,
                                StartPosition = position - currentToken.Length,
                                Length = currentToken.Length
                            });
                        }
                        currentToken = "";
                        currentType = SyntaxTokenType.Identifier;
                    }
                    currentToken += currentChar;
                }
                else
                {
                    if (!string.IsNullOrEmpty(currentToken))
                    {
                        tokens.Add(new SyntaxToken
                        {
                            Text = currentToken,
                            Type = currentType,
                            StartPosition = position - currentToken.Length,
                            Length = currentToken.Length
                        });
                        currentToken = "";
                    }

                    var operatorType = GetOperatorType(currentChar);
                    tokens.Add(new SyntaxToken
                    {
                        Text = currentChar.ToString(),
                        Type = operatorType,
                        StartPosition = position,
                        Length = 1
                    });
                    currentType = SyntaxTokenType.Text;
                }

                position++;
            }

            if (!string.IsNullOrEmpty(currentToken))
            {
                tokens.Add(new SyntaxToken
                {
                    Text = currentToken,
                    Type = currentType,
                    StartPosition = position - currentToken.Length,
                    Length = currentToken.Length
                });
            }

            return tokens;
        }

        private bool IsCommentStart(string line, int position, LanguageDefinition languageDef)
        {
            if (string.IsNullOrEmpty(languageDef.CommentPrefix))
                return false;

            return line.Substring(position).StartsWith(languageDef.CommentPrefix);
        }

        private bool IsStringStart(char currentChar, LanguageDefinition languageDef)
        {
            return languageDef.StringDelimiters.Contains(currentChar.ToString());
        }

        private string ExtractString(string line, int position, LanguageDefinition languageDef)
        {
            var startChar = line[position];
            var result = startChar.ToString();
            position++;

            while (position < line.Length)
            {
                var currentChar = line[position];
                result += currentChar;

                if (currentChar == startChar && (position == 0 || line[position - 1] != '\\'))
                {
                    break;
                }
                position++;
            }

            return result;
        }

        private SyntaxTokenType GetOperatorType(char currentChar)
        {
            return currentChar switch
            {
                '+' or '-' or '*' or '/' or '%' or '=' or '<' or '>' or '!' or '&' or '|' or '^' => SyntaxTokenType.Operator,
                '(' or ')' or '[' or ']' or '{' or '}' => SyntaxTokenType.Delimiter,
                '.' or ',' or ';' or ':' => SyntaxTokenType.Delimiter,
                _ => SyntaxTokenType.Text
            };
        }

        public async Task<List<LanguageDefinition>> GetSupportedLanguagesAsync()
        {
            return new List<LanguageDefinition>(_languages.Values);
        }

        public async Task<LanguageDefinition> GetLanguageDefinitionAsync(string language)
        {
            return GetLanguageDefinition(language);
        }

        private LanguageDefinition GetLanguageDefinition(string language)
        {
            return _languages.TryGetValue(language.ToLower(), out var languageDef) ? languageDef : null;
        }

        public async Task<bool> RegisterLanguageAsync(LanguageDefinition language)
        {
            _languages[language.Name.ToLower()] = language;
            return true;
        }

        public async Task<bool> UnregisterLanguageAsync(string language)
        {
            return _languages.Remove(language.ToLower());
        }

        public async Task<Theme> GetCurrentThemeAsync()
        {
            return _currentTheme;
        }

        public async Task<bool> SetThemeAsync(Theme theme)
        {
            _currentTheme = theme;
            return true;
        }

        public async Task<List<Theme>> GetAvailableThemesAsync()
        {
            return new List<Theme>
            {
                new Theme { Name = "Dark", IsDark = true },
                new Theme { Name = "Light", IsDark = false },
                new Theme { Name = "Monokai", IsDark = true },
                new Theme { Name = "Solarized Dark", IsDark = true },
                new Theme { Name = "Solarized Light", IsDark = false }
            };
        }
    }

    public class SyntaxHighlightingResult
    {
        public string HighlightedCode { get; set; }
        public List<SyntaxToken> Tokens { get; set; }
    }

    public class SyntaxToken
    {
        public string Text { get; set; }
        public SyntaxTokenType Type { get; set; }
        public int StartPosition { get; set; }
        public int Length { get; set; }
        public int LineNumber { get; set; }
    }

    public enum SyntaxTokenType
    {
        Text,
        Keyword,
        Identifier,
        String,
        Comment,
        Number,
        Operator,
        Delimiter,
        Whitespace
    }

    public class LanguageDefinition
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public List<string> FileExtensions { get; set; }
        public List<string> Keywords { get; set; }
        public string CommentPrefix { get; set; }
        public string CommentSuffix { get; set; }
        public string MultiLineCommentStart { get; set; }
        public string MultiLineCommentEnd { get; set; }
        public List<string> StringDelimiters { get; set; }
    }

    public class Theme
    {
        public string Name { get; set; }
        public bool IsDark { get; set; }
        public Dictionary<SyntaxTokenType, string> Colors { get; set; }
    }
}
