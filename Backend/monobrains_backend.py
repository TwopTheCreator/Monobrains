#!/usr/bin/env python3
import sys
import json
import subprocess
import tempfile
import os
import ast
import re
import threading
import time
import importlib.util
import inspect
from typing import Dict, List, Any, Optional, Tuple
from dataclasses import dataclass, asdict
from enum import Enum
import traceback
import io
import contextlib

class DiagnosticSeverity(Enum):
    ERROR = "error"
    WARNING = "warning"
    INFORMATION = "information"
    HINT = "hint"

class CompletionItemKind(Enum):
    TEXT = 1
    METHOD = 2
    FUNCTION = 3
    CONSTRUCTOR = 4
    FIELD = 5
    VARIABLE = 6
    CLASS = 7
    INTERFACE = 8
    MODULE = 9
    PROPERTY = 10
    UNIT = 11
    VALUE = 12
    ENUM = 13
    KEYWORD = 14
    SNIPPET = 15
    COLOR = 16
    FILE = 17
    REFERENCE = 18
    FOLDER = 19
    ENUMMEMBER = 20
    CONSTANT = 21
    STRUCT = 22
    EVENT = 23
    OPERATOR = 24
    TYPEPARAMETER = 25

@dataclass
class Diagnostic:
    message: str
    severity: str
    line_number: int
    column_number: int
    file_path: str = ""
    code: str = ""
    description: str = ""

@dataclass
class CodeCompletionItem:
    text: str
    insert_text: str = ""
    description: str = ""
    detail: str = ""
    kind: int = 1
    documentation: str = ""
    sort_text: str = ""
    filter_text: str = ""
    priority: int = 0

@dataclass
class ExecutionResult:
    output: str
    error: str
    exit_code: int
    execution_time: float

@dataclass
class AnalysisResult:
    diagnostics: List[Diagnostic]
    symbol_table: Dict[str, Any]
    complexity: Dict[str, int]
    code_smells: List[Dict[str, Any]]

@dataclass
class CompletionResult:
    items: List[CodeCompletionItem]
    start_position: int
    end_position: int

@dataclass
class DefinitionResult:
    file_path: str
    line_number: int
    column_number: int
    position: int

@dataclass
class ReferenceResult:
    file_path: str
    line_number: int
    column_number: int
    kind: str

@dataclass
class RefactoringResult:
    success: bool
    new_code: str
    edits: List[Dict[str, Any]]
    error_message: str

@dataclass
class FormattingResult:
    success: bool
    formatted_code: str
    error_message: str

@dataclass
class DebuggingSession:
    session_id: str
    is_active: bool
    state: str

@dataclass
class DebuggingStepResult:
    success: bool
    current_line: int
    current_file: str
    variables: List[Dict[str, Any]]
    call_stack: List[Dict[str, Any]]

@dataclass
class Variable:
    name: str
    value: str
    type: str
    is_expanded: bool = False
    children: List['Variable'] = None

@dataclass
class CallStackFrame:
    method_name: str
    file_name: str
    line_number: int
    module_name: str
    local_variables: List[Variable] = None

class LanguageAnalyzer:
    def __init__(self):
        self.language_processors = {
            'python': PythonProcessor(),
            'csharp': CSharpProcessor(),
            'javascript': JavaScriptProcessor(),
            'typescript': TypeScriptProcessor(),
            'java': JavaProcessor(),
            'cpp': CppProcessor(),
            'c': CProcessor(),
            'html': HtmlProcessor(),
            'css': CssProcessor(),
            'json': JsonProcessor(),
            'xml': XmlProcessor(),
            'markdown': MarkdownProcessor()
        }

    def analyze_code(self, code: str, language: str) -> AnalysisResult:
        processor = self.language_processors.get(language.lower())
        if not processor:
            return AnalysisResult([], {}, {}, [])
        
        return processor.analyze(code)

    def get_completion(self, code: str, position: int, language: str) -> CompletionResult:
        processor = self.language_processors.get(language.lower())
        if not processor:
            return CompletionResult([], position, position)
        
        return processor.get_completion(code, position)

    def get_definition(self, code: str, position: int, language: str) -> DefinitionResult:
        processor = self.language_processors.get(language.lower())
        if not processor:
            return DefinitionResult("", 0, 0, position)
        
        return processor.get_definition(code, position)

    def get_references(self, code: str, position: int, language: str) -> List[ReferenceResult]:
        processor = self.language_processors.get(language.lower())
        if not processor:
            return []
        
        return processor.get_references(code, position)

    def format_code(self, code: str, language: str) -> FormattingResult:
        processor = self.language_processors.get(language.lower())
        if not processor:
            return FormattingResult(False, code, "Language not supported")
        
        return processor.format_code(code)

class BaseProcessor:
    def analyze(self, code: str) -> AnalysisResult:
        return AnalysisResult([], {}, {}, [])

    def get_completion(self, code: str, position: int) -> CompletionResult:
        return CompletionResult([], position, position)

    def get_definition(self, code: str, position: int) -> DefinitionResult:
        return DefinitionResult("", 0, 0, position)

    def get_references(self, code: str, position: int) -> List[ReferenceResult]:
        return []

    def format_code(self, code: str) -> FormattingResult:
        return FormattingResult(True, code, "")

class PythonProcessor(BaseProcessor):
    def analyze(self, code: str) -> AnalysisResult:
        diagnostics = []
        symbol_table = {}
        complexity = {"cyclomatic": 0, "cognitive": 0, "lines": len(code.splitlines())}
        code_smells = []

        try:
            tree = ast.parse(code)
            self._analyze_ast(tree, diagnostics, symbol_table, complexity, code_smells)
        except SyntaxError as e:
            diagnostics.append(Diagnostic(
                message=f"Syntax error: {e.msg}",
                severity=DiagnosticSeverity.ERROR.value,
                line_number=e.lineno or 0,
                column_number=e.offset or 0
            ))

        return AnalysisResult(diagnostics, symbol_table, complexity, code_smells)

    def _analyze_ast(self, node, diagnostics, symbol_table, complexity, code_smells):
        if isinstance(node, ast.FunctionDef):
            symbol_table[node.name] = {
                "kind": "function",
                "line": node.lineno,
                "type": "function"
            }
            complexity["cyclomatic"] += 1
        elif isinstance(node, ast.ClassDef):
            symbol_table[node.name] = {
                "kind": "class",
                "line": node.lineno,
                "type": "class"
            }
        elif isinstance(node, ast.Assign):
            for target in node.targets:
                if isinstance(target, ast.Name):
                    symbol_table[target.id] = {
                        "kind": "variable",
                        "line": node.lineno,
                        "type": "variable"
                    }

        for child in ast.iter_child_nodes(node):
            self._analyze_ast(child, diagnostics, symbol_table, complexity, code_smells)

    def get_completion(self, code: str, position: int) -> CompletionResult:
        completions = []
        lines = code[:position].split('\n')
        current_line = lines[-1] if lines else ""
        
        keywords = ['def', 'class', 'if', 'else', 'elif', 'for', 'while', 'try', 'except', 'finally', 'with', 'import', 'from', 'return', 'yield', 'lambda', 'and', 'or', 'not', 'in', 'is', 'True', 'False', 'None']
        
        for keyword in keywords:
            if keyword.startswith(current_line.split()[-1] if current_line.split() else ""):
                completions.append(CodeCompletionItem(
                    text=keyword,
                    insert_text=keyword,
                    kind=CompletionItemKind.KEYWORD.value,
                    description=f"Python keyword: {keyword}"
                ))

        builtins = ['print', 'len', 'str', 'int', 'float', 'list', 'dict', 'tuple', 'set', 'range', 'enumerate', 'zip', 'map', 'filter', 'sorted', 'reversed', 'min', 'max', 'sum', 'abs', 'round']
        
        for builtin in builtins:
            if builtin.startswith(current_line.split()[-1] if current_line.split() else ""):
                completions.append(CodeCompletionItem(
                    text=builtin,
                    insert_text=builtin,
                    kind=CompletionItemKind.FUNCTION.value,
                    description=f"Built-in function: {builtin}"
                ))

        return CompletionResult(completions, position, position)

    def format_code(self, code: str) -> FormattingResult:
        try:
            import autopep8
            formatted = autopep8.fix_code(code)
            return FormattingResult(True, formatted, "")
        except ImportError:
            return FormattingResult(False, code, "autopep8 not installed")
        except Exception as e:
            return FormattingResult(False, code, str(e))

class CSharpProcessor(BaseProcessor):
    def get_completion(self, code: str, position: int) -> CompletionResult:
        completions = []
        keywords = ['public', 'private', 'protected', 'internal', 'static', 'readonly', 'const', 'virtual', 'override', 'abstract', 'sealed', 'class', 'interface', 'struct', 'enum', 'namespace', 'using', 'if', 'else', 'for', 'foreach', 'while', 'do', 'switch', 'case', 'default', 'break', 'continue', 'return', 'throw', 'try', 'catch', 'finally', 'var', 'new', 'this', 'base', 'null', 'true', 'false']
        
        current_line = code[:position].split('\n')[-1]
        for keyword in keywords:
            if keyword.startswith(current_line.split()[-1] if current_line.split() else ""):
                completions.append(CodeCompletionItem(
                    text=keyword,
                    insert_text=keyword,
                    kind=CompletionItemKind.KEYWORD.value,
                    description=f"C# keyword: {keyword}"
                ))

        return CompletionResult(completions, position, position)

class JavaScriptProcessor(BaseProcessor):
    def get_completion(self, code: str, position: int) -> CompletionResult:
        completions = []
        keywords = ['var', 'let', 'const', 'function', 'class', 'if', 'else', 'for', 'while', 'do', 'switch', 'case', 'default', 'break', 'continue', 'return', 'throw', 'try', 'catch', 'finally', 'new', 'this', 'null', 'undefined', 'true', 'false', 'typeof', 'instanceof', 'in', 'of']
        
        current_line = code[:position].split('\n')[-1]
        for keyword in keywords:
            if keyword.startswith(current_line.split()[-1] if current_line.split() else ""):
                completions.append(CodeCompletionItem(
                    text=keyword,
                    insert_text=keyword,
                    kind=CompletionItemKind.KEYWORD.value,
                    description=f"JavaScript keyword: {keyword}"
                ))

        return CompletionResult(completions, position, position)

class TypeScriptProcessor(BaseProcessor):
    def get_completion(self, code: str, position: int) -> CompletionResult:
        completions = []
        keywords = ['var', 'let', 'const', 'function', 'class', 'interface', 'type', 'enum', 'namespace', 'module', 'import', 'export', 'if', 'else', 'for', 'while', 'do', 'switch', 'case', 'default', 'break', 'continue', 'return', 'throw', 'try', 'catch', 'finally', 'new', 'this', 'null', 'undefined', 'true', 'false', 'typeof', 'instanceof', 'in', 'of', 'as', 'is']
        
        current_line = code[:position].split('\n')[-1]
        for keyword in keywords:
            if keyword.startswith(current_line.split()[-1] if current_line.split() else ""):
                completions.append(CodeCompletionItem(
                    text=keyword,
                    insert_text=keyword,
                    kind=CompletionItemKind.KEYWORD.value,
                    description=f"TypeScript keyword: {keyword}"
                ))

        return CompletionResult(completions, position, position)

class JavaProcessor(BaseProcessor):
    def get_completion(self, code: str, position: int) -> CompletionResult:
        completions = []
        keywords = ['public', 'private', 'protected', 'static', 'final', 'abstract', 'class', 'interface', 'enum', 'package', 'import', 'if', 'else', 'for', 'while', 'do', 'switch', 'case', 'default', 'break', 'continue', 'return', 'throw', 'try', 'catch', 'finally', 'new', 'this', 'super', 'null', 'true', 'false']
        
        current_line = code[:position].split('\n')[-1]
        for keyword in keywords:
            if keyword.startswith(current_line.split()[-1] if current_line.split() else ""):
                completions.append(CodeCompletionItem(
                    text=keyword,
                    insert_text=keyword,
                    kind=CompletionItemKind.KEYWORD.value,
                    description=f"Java keyword: {keyword}"
                ))

        return CompletionResult(completions, position, position)

class CppProcessor(BaseProcessor):
    def get_completion(self, code: str, position: int) -> CompletionResult:
        completions = []
        keywords = ['public', 'private', 'protected', 'static', 'const', 'virtual', 'override', 'class', 'struct', 'enum', 'namespace', 'using', 'if', 'else', 'for', 'while', 'do', 'switch', 'case', 'default', 'break', 'continue', 'return', 'throw', 'try', 'catch', 'new', 'delete', 'this', 'nullptr', 'true', 'false']
        
        current_line = code[:position].split('\n')[-1]
        for keyword in keywords:
            if keyword.startswith(current_line.split()[-1] if current_line.split() else ""):
                completions.append(CodeCompletionItem(
                    text=keyword,
                    insert_text=keyword,
                    kind=CompletionItemKind.KEYWORD.value,
                    description=f"C++ keyword: {keyword}"
                ))

        return CompletionResult(completions, position, position)

class CProcessor(BaseProcessor):
    def get_completion(self, code: str, position: int) -> CompletionResult:
        completions = []
        keywords = ['auto', 'break', 'case', 'char', 'const', 'continue', 'default', 'do', 'double', 'else', 'enum', 'extern', 'float', 'for', 'goto', 'if', 'int', 'long', 'register', 'return', 'short', 'signed', 'sizeof', 'static', 'struct', 'switch', 'typedef', 'union', 'unsigned', 'void', 'volatile', 'while']
        
        current_line = code[:position].split('\n')[-1]
        for keyword in keywords:
            if keyword.startswith(current_line.split()[-1] if current_line.split() else ""):
                completions.append(CodeCompletionItem(
                    text=keyword,
                    insert_text=keyword,
                    kind=CompletionItemKind.KEYWORD.value,
                    description=f"C keyword: {keyword}"
                ))

        return CompletionResult(completions, position, position)

class HtmlProcessor(BaseProcessor):
    def get_completion(self, code: str, position: int) -> CompletionResult:
        completions = []
        tags = ['html', 'head', 'body', 'title', 'meta', 'link', 'script', 'style', 'div', 'span', 'p', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'a', 'img', 'ul', 'ol', 'li', 'table', 'tr', 'td', 'th', 'form', 'input', 'button', 'textarea', 'select', 'option']
        
        current_line = code[:position].split('\n')[-1]
        for tag in tags:
            if tag.startswith(current_line.split()[-1] if current_line.split() else ""):
                completions.append(CodeCompletionItem(
                    text=tag,
                    insert_text=f"<{tag}></{tag}>",
                    kind=CompletionItemKind.TEXT.value,
                    description=f"HTML tag: {tag}"
                ))

        return CompletionResult(completions, position, position)

class CssProcessor(BaseProcessor):
    def get_completion(self, code: str, position: int) -> CompletionResult:
        completions = []
        properties = ['color', 'background', 'font-size', 'font-family', 'margin', 'padding', 'border', 'width', 'height', 'display', 'position', 'top', 'left', 'right', 'bottom', 'z-index', 'opacity', 'visibility', 'overflow', 'text-align', 'line-height', 'text-decoration', 'font-weight', 'font-style']
        
        current_line = code[:position].split('\n')[-1]
        for prop in properties:
            if prop.startswith(current_line.split()[-1] if current_line.split() else ""):
                completions.append(CodeCompletionItem(
                    text=prop,
                    insert_text=f"{prop}: ",
                    kind=CompletionItemKind.PROPERTY.value,
                    description=f"CSS property: {prop}"
                ))

        return CompletionResult(completions, position, position)

class JsonProcessor(BaseProcessor):
    def analyze(self, code: str) -> AnalysisResult:
        diagnostics = []
        try:
            json.loads(code)
        except json.JSONDecodeError as e:
            diagnostics.append(Diagnostic(
                message=f"JSON syntax error: {e.msg}",
                severity=DiagnosticSeverity.ERROR.value,
                line_number=e.lineno or 0,
                column_number=e.colno or 0
            ))
        
        return AnalysisResult(diagnostics, {}, {}, [])

class XmlProcessor(BaseProcessor):
    def analyze(self, code: str) -> AnalysisResult:
        diagnostics = []
        try:
            import xml.etree.ElementTree as ET
            ET.fromstring(code)
        except ET.ParseError as e:
            diagnostics.append(Diagnostic(
                message=f"XML syntax error: {e.msg}",
                severity=DiagnosticSeverity.ERROR.value,
                line_number=e.lineno or 0,
                column_number=e.colno or 0
            ))
        
        return AnalysisResult(diagnostics, {}, {}, [])

class MarkdownProcessor(BaseProcessor):
    def get_completion(self, code: str, position: int) -> CompletionResult:
        completions = []
        markdown_elements = ['#', '##', '###', '####', '#####', '######', '**', '*', '`', '```', '>', '-', '1.', '[', ']', '(', ')', '![']
        
        current_line = code[:position].split('\n')[-1]
        for element in markdown_elements:
            if element.startswith(current_line.split()[-1] if current_line.split() else ""):
                completions.append(CodeCompletionItem(
                    text=element,
                    insert_text=element,
                    kind=CompletionItemKind.TEXT.value,
                    description=f"Markdown element: {element}"
                ))

        return CompletionResult(completions, position, position)

class CodeExecutor:
    def __init__(self):
        self.sandboxes = {}
        self.execution_threads = {}

    def execute_code(self, code: str, language: str) -> ExecutionResult:
        start_time = time.time()
        
        try:
            if language.lower() == 'python':
                return self._execute_python(code, start_time)
            elif language.lower() == 'javascript':
                return self._execute_javascript(code, start_time)
            elif language.lower() == 'csharp':
                return self._execute_csharp(code, start_time)
            elif language.lower() == 'java':
                return self._execute_java(code, start_time)
            elif language.lower() == 'cpp':
                return self._execute_cpp(code, start_time)
            elif language.lower() == 'c':
                return self._execute_c(code, start_time)
            else:
                return ExecutionResult("", f"Language {language} not supported", 1, time.time() - start_time)
        except Exception as e:
            return ExecutionResult("", str(e), 1, time.time() - start_time)

    def _execute_python(self, code: str, start_time: float) -> ExecutionResult:
        output = io.StringIO()
        error = io.StringIO()
        
        try:
            with contextlib.redirect_stdout(output), contextlib.redirect_stderr(error):
                exec(code)
            return ExecutionResult(output.getvalue(), error.getvalue(), 0, time.time() - start_time)
        except Exception as e:
            return ExecutionResult(output.getvalue(), str(e), 1, time.time() - start_time)

    def _execute_javascript(self, code: str, start_time: float) -> ExecutionResult:
        try:
            result = subprocess.run(['node', '-e', code], capture_output=True, text=True, timeout=30)
            return ExecutionResult(result.stdout, result.stderr, result.returncode, time.time() - start_time)
        except subprocess.TimeoutExpired:
            return ExecutionResult("", "Execution timeout", 1, time.time() - start_time)
        except FileNotFoundError:
            return ExecutionResult("", "Node.js not found", 1, time.time() - start_time)

    def _execute_csharp(self, code: str, start_time: float) -> ExecutionResult:
        try:
            with tempfile.NamedTemporaryFile(mode='w', suffix='.cs', delete=False) as f:
                f.write(code)
                temp_file = f.name
            
            result = subprocess.run(['dotnet', 'run', '--project', temp_file], capture_output=True, text=True, timeout=30)
            os.unlink(temp_file)
            return ExecutionResult(result.stdout, result.stderr, result.returncode, time.time() - start_time)
        except subprocess.TimeoutExpired:
            return ExecutionResult("", "Execution timeout", 1, time.time() - start_time)
        except FileNotFoundError:
            return ExecutionResult("", "dotnet not found", 1, time.time() - start_time)

    def _execute_java(self, code: str, start_time: float) -> ExecutionResult:
        try:
            with tempfile.NamedTemporaryFile(mode='w', suffix='.java', delete=False) as f:
                f.write(code)
                temp_file = f.name
            
            class_name = os.path.splitext(os.path.basename(temp_file))[0]
            compile_result = subprocess.run(['javac', temp_file], capture_output=True, text=True)
            
            if compile_result.returncode != 0:
                os.unlink(temp_file)
                return ExecutionResult("", compile_result.stderr, compile_result.returncode, time.time() - start_time)
            
            run_result = subprocess.run(['java', class_name], capture_output=True, text=True, timeout=30)
            os.unlink(temp_file)
            os.unlink(f"{class_name}.class")
            return ExecutionResult(run_result.stdout, run_result.stderr, run_result.returncode, time.time() - start_time)
        except subprocess.TimeoutExpired:
            return ExecutionResult("", "Execution timeout", 1, time.time() - start_time)
        except FileNotFoundError:
            return ExecutionResult("", "Java not found", 1, time.time() - start_time)

    def _execute_cpp(self, code: str, start_time: float) -> ExecutionResult:
        try:
            with tempfile.NamedTemporaryFile(mode='w', suffix='.cpp', delete=False) as f:
                f.write(code)
                temp_file = f.name
            
            executable = temp_file.replace('.cpp', '.exe' if os.name == 'nt' else '')
            compile_result = subprocess.run(['g++', temp_file, '-o', executable], capture_output=True, text=True)
            
            if compile_result.returncode != 0:
                os.unlink(temp_file)
                return ExecutionResult("", compile_result.stderr, compile_result.returncode, time.time() - start_time)
            
            run_result = subprocess.run([executable], capture_output=True, text=True, timeout=30)
            os.unlink(temp_file)
            os.unlink(executable)
            return ExecutionResult(run_result.stdout, run_result.stderr, run_result.returncode, time.time() - start_time)
        except subprocess.TimeoutExpired:
            return ExecutionResult("", "Execution timeout", 1, time.time() - start_time)
        except FileNotFoundError:
            return ExecutionResult("", "g++ not found", 1, time.time() - start_time)

    def _execute_c(self, code: str, start_time: float) -> ExecutionResult:
        try:
            with tempfile.NamedTemporaryFile(mode='w', suffix='.c', delete=False) as f:
                f.write(code)
                temp_file = f.name
            
            executable = temp_file.replace('.c', '.exe' if os.name == 'nt' else '')
            compile_result = subprocess.run(['gcc', temp_file, '-o', executable], capture_output=True, text=True)
            
            if compile_result.returncode != 0:
                os.unlink(temp_file)
                return ExecutionResult("", compile_result.stderr, compile_result.returncode, time.time() - start_time)
            
            run_result = subprocess.run([executable], capture_output=True, text=True, timeout=30)
            os.unlink(temp_file)
            os.unlink(executable)
            return ExecutionResult(run_result.stdout, run_result.stderr, run_result.returncode, time.time() - start_time)
        except subprocess.TimeoutExpired:
            return ExecutionResult("", "Execution timeout", 1, time.time() - start_time)
        except FileNotFoundError:
            return ExecutionResult("", "gcc not found", 1, time.time() - start_time)

class PluginManager:
    def __init__(self):
        self.loaded_plugins = {}
        self.plugin_directory = os.path.join(os.path.dirname(__file__), 'plugins')

    def load_plugin(self, plugin_path: str) -> bool:
        try:
            spec = importlib.util.spec_from_file_location("plugin", plugin_path)
            module = importlib.util.module_from_spec(spec)
            spec.loader.exec_module(module)
            
            plugin_name = os.path.basename(plugin_path).replace('.py', '')
            self.loaded_plugins[plugin_name] = module
            return True
        except Exception as e:
            print(f"Error loading plugin {plugin_path}: {e}")
            return False

    def unload_plugin(self, plugin_name: str) -> bool:
        if plugin_name in self.loaded_plugins:
            del self.loaded_plugins[plugin_name]
            return True
        return False

    def get_loaded_plugins(self) -> List[Dict[str, Any]]:
        plugins = []
        for name, module in self.loaded_plugins.items():
            plugin_info = {
                "name": name,
                "version": getattr(module, '__version__', '1.0.0'),
                "description": getattr(module, '__doc__', 'No description'),
                "author": getattr(module, '__author__', 'Unknown'),
                "dependencies": getattr(module, '__dependencies__', []),
                "is_loaded": True
            }
            plugins.append(plugin_info)
        return plugins

class MonobrainsBackend:
    def __init__(self):
        self.analyzer = LanguageAnalyzer()
        self.executor = CodeExecutor()
        self.plugin_manager = PluginManager()
        self.debug_sessions = {}
        self.current_debug_session = None

    def process_command(self, command: Dict[str, Any]) -> Dict[str, Any]:
        action = command.get('action')
        
        try:
            if action == 'execute_python':
                result = self.executor.execute_code(command['code'], 'python')
                return asdict(result)
            elif action == 'analyze_code':
                result = self.analyzer.analyze_code(command['code'], command['language'])
                return asdict(result)
            elif action == 'execute_sandbox':
                result = self.executor.execute_code(command['code'], command['language'])
                return asdict(result)
            elif action == 'lint_code':
                result = self.analyzer.analyze_code(command['code'], command['language'])
                return {"diagnostics": [asdict(d) for d in result.diagnostics]}
            elif action == 'get_completion':
                result = self.analyzer.get_completion(command['code'], command['position'], command['language'])
                return asdict(result)
            elif action == 'get_definition':
                result = self.analyzer.get_definition(command['code'], command['position'], command['language'])
                return asdict(result)
            elif action == 'get_references':
                result = self.analyzer.get_references(command['code'], command['position'], command['language'])
                return {"references": [asdict(r) for r in result]}
            elif action == 'format_code':
                result = self.analyzer.format_code(command['code'], command['language'])
                return asdict(result)
            elif action == 'start_debug':
                session_id = str(time.time())
                session = DebuggingSession(session_id, True, "running")
                self.debug_sessions[session_id] = session
                self.current_debug_session = session
                return asdict(session)
            elif action == 'stop_debug':
                if self.current_debug_session:
                    self.current_debug_session.is_active = False
                    self.current_debug_session.state = "stopped"
                return {"success": True}
            elif action == 'step_over':
                return {"success": True, "current_line": 1, "current_file": "", "variables": [], "call_stack": []}
            elif action == 'step_into':
                return {"success": True, "current_line": 1, "current_file": "", "variables": [], "call_stack": []}
            elif action == 'step_out':
                return {"success": True, "current_line": 1, "current_file": "", "variables": [], "call_stack": []}
            elif action == 'add_breakpoint':
                return {"success": True}
            elif action == 'remove_breakpoint':
                return {"success": True}
            elif action == 'get_variables':
                return {"variables": []}
            elif action == 'get_call_stack':
                return {"frames": []}
            elif action == 'load_plugin':
                success = self.plugin_manager.load_plugin(command['path'])
                return {"success": success}
            elif action == 'unload_plugin':
                success = self.plugin_manager.unload_plugin(command['name'])
                return {"success": success}
            elif action == 'get_loaded_plugins':
                plugins = self.plugin_manager.get_loaded_plugins()
                return {"plugins": plugins}
            else:
                return {"error": f"Unknown action: {action}"}
        except Exception as e:
            return {"error": str(e)}

def main():
    backend = MonobrainsBackend()
    
    print("Monobrains Backend started", file=sys.stderr)
    
    try:
        while True:
            line = input()
            if not line:
                continue
                
            try:
                command = json.loads(line)
                response = backend.process_command(command)
                print(json.dumps(response))
                sys.stdout.flush()
            except json.JSONDecodeError as e:
                error_response = {"error": f"Invalid JSON: {e}"}
                print(json.dumps(error_response))
                sys.stdout.flush()
    except EOFError:
        pass
    except KeyboardInterrupt:
        pass
    finally:
        print("Monobrains Backend stopped", file=sys.stderr)

if __name__ == "__main__":
    main()
