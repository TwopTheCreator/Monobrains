# Monobrains IDE

A comprehensive, multi-language code editor and IDE built with C# WPF frontend and Python backend.

## Features

- **Multi-Language Support**: C#, Python, JavaScript, TypeScript, Java, C++, C, HTML, CSS, JSON, XML, Markdown
- **Advanced Syntax Highlighting**: Real-time syntax highlighting with customizable themes
- **Intelligent Code Completion**: Context-aware code completion and IntelliSense
- **Powerful Debugging**: Step-through debugging with breakpoints and variable inspection
- **Code Execution**: Run code in secure sandboxes for multiple languages
- **Code Analysis**: Real-time linting, error detection, and code quality analysis
- **Refactoring Tools**: Extract methods, rename symbols, format code, and more
- **Project Management**: Create, build, and manage projects of various types
- **Plugin System**: Extensible architecture with plugin support
- **Modern UI**: Beautiful Material Design interface with dark/light themes

## Requirements

- **.NET 6.0** or later
- **Python 3.9** or later
- **Windows 10** or later (for WPF support)

## Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/monobrains/monobrains-ide.git
   cd monobrains-ide
   ```

2. Build the project:
   ```bash
   # Windows
   build.bat
   
   # Linux/macOS
   chmod +x build.sh
   ./build.sh
   ```

3. Run Monobrains IDE:
   ```bash
   cd Monobrains/bin/Release/net6.0-windows
   ./Monobrains.exe
   ```

## Usage

### Creating a New Project

1. Go to **File** → **New Project**
2. Select a project template (C#, Python, JavaScript, TypeScript, or Generic)
3. Choose a location and name for your project
4. Click **Create**

### Opening Files

1. Go to **File** → **Open File** or press `Ctrl+O`
2. Select the file you want to open
3. The file will open in a new tab

### Running Code

1. Open a file with executable code
2. Press `F5` to start debugging or `Ctrl+F5` to run without debugging
3. View output in the Output panel

### Debugging

1. Set breakpoints by clicking in the left margin or pressing `F9`
2. Start debugging with `F5`
3. Use `F10` (Step Over), `F11` (Step Into), or `Shift+F11` (Step Out)
4. Inspect variables in the Variables panel
5. View call stack in the Call Stack panel

### Code Completion

- Type code and press `Ctrl+Space` to trigger completion
- Use arrow keys to navigate suggestions
- Press `Enter` or `Tab` to insert the selected completion

### Refactoring

1. Right-click on code to see available refactoring options
2. Or use **Edit** → **Refactor** menu
3. Available refactorings include:
   - Rename Symbol
   - Extract Method
   - Extract Variable
   - Format Code
   - Organize Imports

## Architecture

### Frontend (C# WPF)
- **MainWindow**: Main application window with tabs, menus, and panels
- **EditorTabViewModel**: View model for individual editor tabs
- **Services**: Various services for different IDE functionality

### Backend (Python)
- **Language Processors**: Handle syntax highlighting, completion, and analysis for different languages
- **Code Executor**: Execute code in secure sandboxes
- **Plugin Manager**: Load and manage plugins
- **Debugging Engine**: Handle debugging sessions and breakpoints

## Supported Languages

| Language | Syntax Highlighting | Code Completion | Debugging | Execution |
|----------|-------------------|----------------|-----------|-----------|
| C# | ✅ | ✅ | ✅ | ✅ |
| Python | ✅ | ✅ | ✅ | ✅ |
| JavaScript | ✅ | ✅ | ❌ | ✅ |
| TypeScript | ✅ | ✅ | ❌ | ✅ |
| Java | ✅ | ✅ | ❌ | ✅ |
| C++ | ✅ | ✅ | ❌ | ✅ |
| C | ✅ | ✅ | ❌ | ✅ |
| HTML | ✅ | ✅ | ❌ | ❌ |
| CSS | ✅ | ✅ | ❌ | ❌ |
| JSON | ✅ | ✅ | ❌ | ❌ |
| XML | ✅ | ✅ | ❌ | ❌ |
| Markdown | ✅ | ✅ | ❌ | ❌ |

## Plugin Development

Monobrains IDE supports plugins written in Python. To create a plugin:

1. Create a Python file in the `Plugins` directory
2. Implement the required plugin interface
3. The plugin will be automatically loaded when the IDE starts

Example plugin structure:
```python
__version__ = "1.0.0"
__author__ = "Your Name"
__description__ = "Plugin description"

def initialize():
    # Plugin initialization code
    pass

def cleanup():
    # Plugin cleanup code
    pass
```

## Configuration

Monobrains IDE stores configuration in:
- **Windows**: `%APPDATA%\Monobrains\config.json`
- **Linux/macOS**: `~/.config/monobrains/config.json`

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+N` | New File |
| `Ctrl+O` | Open File |
| `Ctrl+S` | Save File |
| `Ctrl+Shift+S` | Save As |
| `Ctrl+F` | Find |
| `Ctrl+H` | Replace |
| `Ctrl+G` | Go to Line |
| `Ctrl+Shift+F` | Format Code |
| `F5` | Start Debugging |
| `Ctrl+F5` | Start Without Debugging |
| `Shift+F5` | Stop Debugging |
| `F9` | Toggle Breakpoint |
| `F10` | Step Over |
| `F11` | Step Into |
| `Shift+F11` | Step Out |
| `Ctrl+Space` | Trigger Completion |
| `F1` | Help |

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Material Design for the beautiful UI components
- AvalonEdit for the text editor foundation
- Python community for the robust backend implementation
- .NET community for the excellent WPF framework

## Support

For support, please open an issue on GitHub or contact the development team.

---

**Monobrains IDE** - Empowering developers with intelligent code editing and debugging capabilities.
