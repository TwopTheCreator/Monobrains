using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Monobrains.Core;

namespace Monobrains.Services
{
    public class ProjectManagerService : IProjectManagerService
    {
        private ProjectInfo _currentProject;
        private readonly List<ProjectFile> _projectFiles;

        public ProjectManagerService()
        {
            _projectFiles = new List<ProjectFile>();
        }

        public async Task<bool> CreateProjectAsync(string projectPath, ProjectTemplate template)
        {
            try
            {
                Directory.CreateDirectory(projectPath);
                
                _currentProject = new ProjectInfo
                {
                    Name = Path.GetFileName(projectPath),
                    Path = projectPath,
                    Type = template.Type,
                    CreatedDate = DateTime.Now,
                    LastModified = DateTime.Now
                };

                await CreateProjectFilesAsync(projectPath, template);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task CreateProjectFilesAsync(string projectPath, ProjectTemplate template)
        {
            switch (template.Type.ToLower())
            {
                case "csharp":
                    await CreateCSharpProjectAsync(projectPath);
                    break;
                case "python":
                    await CreatePythonProjectAsync(projectPath);
                    break;
                case "javascript":
                    await CreateJavaScriptProjectAsync(projectPath);
                    break;
                case "typescript":
                    await CreateTypeScriptProjectAsync(projectPath);
                    break;
                default:
                    await CreateGenericProjectAsync(projectPath);
                    break;
            }
        }

        private async Task CreateCSharpProjectAsync(string projectPath)
        {
            var projectFile = Path.Combine(projectPath, $"{Path.GetFileName(projectPath)}.csproj");
            var programFile = Path.Combine(projectPath, "Program.cs");
            
            var projectContent = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net6.0</TargetFramework>
  </PropertyGroup>
</Project>";

            var programContent = @"using System;

namespace " + Path.GetFileName(projectPath) + @"
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(""Hello World!"");
        }
    }
}";

            await File.WriteAllTextAsync(projectFile, projectContent);
            await File.WriteAllTextAsync(programFile, programContent);
        }

        private async Task CreatePythonProjectAsync(string projectPath)
        {
            var mainFile = Path.Combine(projectPath, "main.py");
            var requirementsFile = Path.Combine(projectPath, "requirements.txt");
            
            var mainContent = @"def main():
    print(""Hello World!"")

if __name__ == ""__main__"":
    main()";

            await File.WriteAllTextAsync(mainFile, mainContent);
            await File.WriteAllTextAsync(requirementsFile, "");
        }

        private async Task CreateJavaScriptProjectAsync(string projectPath)
        {
            var packageFile = Path.Combine(projectPath, "package.json");
            var mainFile = Path.Combine(projectPath, "index.js");
            
            var packageContent = @"{
  ""name"": """ + Path.GetFileName(projectPath).ToLower() + @""",
  ""version"": ""1.0.0"",
  ""description"": """",
  ""main"": ""index.js"",
  ""scripts"": {
    ""start"": ""node index.js""
  }
}";

            var mainContent = @"console.log(""Hello World!"");";

            await File.WriteAllTextAsync(packageFile, packageContent);
            await File.WriteAllTextAsync(mainFile, mainContent);
        }

        private async Task CreateTypeScriptProjectAsync(string projectPath)
        {
            var packageFile = Path.Combine(projectPath, "package.json");
            var tsconfigFile = Path.Combine(projectPath, "tsconfig.json");
            var mainFile = Path.Combine(projectPath, "index.ts");
            
            var packageContent = @"{
  ""name"": """ + Path.GetFileName(projectPath).ToLower() + @""",
  ""version"": ""1.0.0"",
  ""description"": """",
  ""main"": ""index.js"",
  ""scripts"": {
    ""build"": ""tsc"",
    ""start"": ""node index.js""
  },
  ""devDependencies"": {
    ""typescript"": ""^4.0.0"",
    ""@types/node"": ""^16.0.0""
  }
}";

            var tsconfigContent = @"{
  ""compilerOptions"": {
    ""target"": ""ES2020"",
    ""module"": ""commonjs"",
    ""outDir"": ""./dist"",
    ""rootDir"": ""./"",
    ""strict"": true,
    ""esModuleInterop"": true,
    ""skipLibCheck"": true,
    ""forceConsistentCasingInFileNames"": true
  },
  ""include"": [""**/*.ts""],
  ""exclude"": [""node_modules"", ""dist""]
}";

            var mainContent = @"console.log(""Hello World!"");";

            await File.WriteAllTextAsync(packageFile, packageContent);
            await File.WriteAllTextAsync(tsconfigFile, tsconfigContent);
            await File.WriteAllTextAsync(mainFile, mainContent);
        }

        private async Task CreateGenericProjectAsync(string projectPath)
        {
            var readmeFile = Path.Combine(projectPath, "README.md");
            var readmeContent = $"# {Path.GetFileName(projectPath)}\n\nA new project created with Monobrains IDE.";
            await File.WriteAllTextAsync(readmeFile, readmeContent);
        }

        public async Task<bool> OpenProjectAsync(string projectPath)
        {
            try
            {
                if (Directory.Exists(projectPath))
                {
                    _currentProject = new ProjectInfo
                    {
                        Name = Path.GetFileName(projectPath),
                        Path = projectPath,
                        Type = DetectProjectType(projectPath),
                        CreatedDate = Directory.GetCreationTime(projectPath),
                        LastModified = Directory.GetLastWriteTime(projectPath)
                    };

                    await LoadProjectFilesAsync(projectPath);
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string DetectProjectType(string projectPath)
        {
            if (File.Exists(Path.Combine(projectPath, "*.csproj")))
                return "csharp";
            if (File.Exists(Path.Combine(projectPath, "requirements.txt")))
                return "python";
            if (File.Exists(Path.Combine(projectPath, "package.json")))
                return "javascript";
            if (File.Exists(Path.Combine(projectPath, "tsconfig.json")))
                return "typescript";
            return "generic";
        }

        private async Task LoadProjectFilesAsync(string projectPath)
        {
            _projectFiles.Clear();
            
            var files = Directory.GetFiles(projectPath, "*", SearchOption.AllDirectories);
            
            foreach (var file in files)
            {
                var relativePath = Path.GetRelativePath(projectPath, file);
                _projectFiles.Add(new ProjectFile
                {
                    Name = Path.GetFileName(file),
                    RelativePath = relativePath,
                    FullPath = file,
                    IsIncluded = true,
                    LastModified = File.GetLastWriteTime(file)
                });
            }
        }

        public async Task<bool> SaveProjectAsync()
        {
            return true;
        }

        public async Task<bool> CloseProjectAsync()
        {
            _currentProject = null;
            _projectFiles.Clear();
            return true;
        }

        public async Task<ProjectInfo> GetProjectInfoAsync()
        {
            return _currentProject;
        }

        public async Task<List<ProjectFile>> GetProjectFilesAsync()
        {
            return _projectFiles;
        }

        public async Task<bool> AddFileToProjectAsync(string filePath)
        {
            if (_currentProject == null) return false;
            
            var relativePath = Path.GetRelativePath(_currentProject.Path, filePath);
            _projectFiles.Add(new ProjectFile
            {
                Name = Path.GetFileName(filePath),
                RelativePath = relativePath,
                FullPath = filePath,
                IsIncluded = true,
                LastModified = File.GetLastWriteTime(filePath)
            });
            
            return true;
        }

        public async Task<bool> RemoveFileFromProjectAsync(string filePath)
        {
            var file = _projectFiles.Find(f => f.FullPath == filePath);
            if (file != null)
            {
                _projectFiles.Remove(file);
                return true;
            }
            return false;
        }

        public async Task<BuildResult> BuildProjectAsync()
        {
            if (_currentProject == null)
                return new BuildResult { Success = false, ErrorMessage = "No project loaded" };

            try
            {
                switch (_currentProject.Type.ToLower())
                {
                    case "csharp":
                        return await BuildCSharpProjectAsync();
                    case "python":
                        return await BuildPythonProjectAsync();
                    case "javascript":
                        return await BuildJavaScriptProjectAsync();
                    case "typescript":
                        return await BuildTypeScriptProjectAsync();
                    default:
                        return new BuildResult { Success = true, Output = "Generic project - no build required" };
                }
            }
            catch (Exception ex)
            {
                return new BuildResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        private async Task<BuildResult> BuildCSharpProjectAsync()
        {
            try
            {
                var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "build",
                    WorkingDirectory = _currentProject.Path,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                });

                await process.WaitForExitAsync();
                
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                
                return new BuildResult
                {
                    Success = process.ExitCode == 0,
                    Output = output,
                    ErrorMessage = error
                };
            }
            catch (Exception ex)
            {
                return new BuildResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        private async Task<BuildResult> BuildPythonProjectAsync()
        {
            return new BuildResult { Success = true, Output = "Python project - no build required" };
        }

        private async Task<BuildResult> BuildJavaScriptProjectAsync()
        {
            return new BuildResult { Success = true, Output = "JavaScript project - no build required" };
        }

        private async Task<BuildResult> BuildTypeScriptProjectAsync()
        {
            try
            {
                var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "npm",
                    Arguments = "run build",
                    WorkingDirectory = _currentProject.Path,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                });

                await process.WaitForExitAsync();
                
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                
                return new BuildResult
                {
                    Success = process.ExitCode == 0,
                    Output = output,
                    ErrorMessage = error
                };
            }
            catch (Exception ex)
            {
                return new BuildResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<BuildResult> CleanProjectAsync()
        {
            if (_currentProject == null)
                return new BuildResult { Success = false, ErrorMessage = "No project loaded" };

            try
            {
                var binDir = Path.Combine(_currentProject.Path, "bin");
                var objDir = Path.Combine(_currentProject.Path, "obj");
                
                if (Directory.Exists(binDir))
                    Directory.Delete(binDir, true);
                
                if (Directory.Exists(objDir))
                    Directory.Delete(objDir, true);
                
                return new BuildResult { Success = true, Output = "Project cleaned successfully" };
            }
            catch (Exception ex)
            {
                return new BuildResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<BuildResult> RebuildProjectAsync()
        {
            var cleanResult = await CleanProjectAsync();
            if (!cleanResult.Success)
                return cleanResult;
            
            return await BuildProjectAsync();
        }

        public async Task<List<BuildError>> GetBuildErrorsAsync()
        {
            return new List<BuildError>();
        }

        public async Task<List<BuildWarning>> GetBuildWarningsAsync()
        {
            return new List<BuildWarning>();
        }

        public async Task<ProjectConfiguration> GetProjectConfigurationAsync()
        {
            return new ProjectConfiguration
            {
                ProjectName = _currentProject?.Name ?? "",
                ProjectType = _currentProject?.Type ?? "",
                BuildConfiguration = "Debug",
                TargetFramework = "net6.0",
                OutputPath = "bin/Debug",
                IntermediateOutputPath = "obj/Debug"
            };
        }

        public async Task<bool> SetProjectConfigurationAsync(ProjectConfiguration config)
        {
            return true;
        }

        public async Task<List<ProjectTemplate>> GetAvailableTemplatesAsync()
        {
            return new List<ProjectTemplate>
            {
                new ProjectTemplate { Name = "C# Console Application", Type = "csharp", Description = "A simple C# console application" },
                new ProjectTemplate { Name = "Python Application", Type = "python", Description = "A Python application" },
                new ProjectTemplate { Name = "JavaScript Application", Type = "javascript", Description = "A JavaScript application" },
                new ProjectTemplate { Name = "TypeScript Application", Type = "typescript", Description = "A TypeScript application" },
                new ProjectTemplate { Name = "Empty Project", Type = "generic", Description = "An empty project" }
            };
        }
    }

    public class ProjectInfo
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Type { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModified { get; set; }
    }

    public class ProjectFile
    {
        public string Name { get; set; }
        public string RelativePath { get; set; }
        public string FullPath { get; set; }
        public bool IsIncluded { get; set; }
        public DateTime LastModified { get; set; }
    }

    public class BuildResult
    {
        public bool Success { get; set; }
        public string Output { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class BuildError
    {
        public string Message { get; set; }
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
        public int ColumnNumber { get; set; }
    }

    public class BuildWarning
    {
        public string Message { get; set; }
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
        public int ColumnNumber { get; set; }
    }

    public class ProjectConfiguration
    {
        public string ProjectName { get; set; }
        public string ProjectType { get; set; }
        public string BuildConfiguration { get; set; }
        public string TargetFramework { get; set; }
        public string OutputPath { get; set; }
        public string IntermediateOutputPath { get; set; }
    }

    public class ProjectTemplate
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
    }
}
