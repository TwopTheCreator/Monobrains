using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Monobrains.Core;

namespace Monobrains.Services
{
    public class FileManagerService : IFileManagerService
    {
        public async Task<bool> SaveFileAsync(string filePath, string content)
        {
            try
            {
                await File.WriteAllTextAsync(filePath, content);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<string> LoadFileAsync(string filePath)
        {
            try
            {
                return await File.ReadAllTextAsync(filePath);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public async Task<bool> FileExistsAsync(string filePath)
        {
            return File.Exists(filePath);
        }

        public async Task<FileInfo> GetFileInfoAsync(string filePath)
        {
            try
            {
                var fileInfo = new System.IO.FileInfo(filePath);
                return new FileInfo
                {
                    Name = fileInfo.Name,
                    FullPath = fileInfo.FullName,
                    Size = fileInfo.Length,
                    LastModified = fileInfo.LastWriteTime,
                    IsDirectory = false,
                    Extension = fileInfo.Extension
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<FileInfo>> GetDirectoryContentsAsync(string directoryPath)
        {
            var files = new List<FileInfo>();
            
            try
            {
                if (Directory.Exists(directoryPath))
                {
                    var directoryInfo = new DirectoryInfo(directoryPath);
                    
                    foreach (var file in directoryInfo.GetFiles())
                    {
                        files.Add(new FileInfo
                        {
                            Name = file.Name,
                            FullPath = file.FullName,
                            Size = file.Length,
                            LastModified = file.LastWriteTime,
                            IsDirectory = false,
                            Extension = file.Extension
                        });
                    }
                    
                    foreach (var directory in directoryInfo.GetDirectories())
                    {
                        files.Add(new FileInfo
                        {
                            Name = directory.Name,
                            FullPath = directory.FullName,
                            Size = 0,
                            LastModified = directory.LastWriteTime,
                            IsDirectory = true,
                            Extension = ""
                        });
                    }
                }
            }
            catch (Exception)
            {
                // Return empty list on error
            }
            
            return files;
        }

        public async Task<bool> CreateDirectoryAsync(string directoryPath)
        {
            try
            {
                Directory.CreateDirectory(directoryPath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                File.Delete(filePath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteDirectoryAsync(string directoryPath)
        {
            try
            {
                Directory.Delete(directoryPath, true);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> CopyFileAsync(string sourcePath, string destinationPath)
        {
            try
            {
                File.Copy(sourcePath, destinationPath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> MoveFileAsync(string sourcePath, string destinationPath)
        {
            try
            {
                File.Move(sourcePath, destinationPath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<FileInfo>> SearchFilesAsync(string directoryPath, string pattern)
        {
            var files = new List<FileInfo>();
            
            try
            {
                if (Directory.Exists(directoryPath))
                {
                    var searchFiles = Directory.GetFiles(directoryPath, pattern, SearchOption.AllDirectories);
                    
                    foreach (var filePath in searchFiles)
                    {
                        var fileInfo = await GetFileInfoAsync(filePath);
                        if (fileInfo != null)
                        {
                            files.Add(fileInfo);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Return empty list on error
            }
            
            return files;
        }

        public async Task<FileWatcher> WatchFileAsync(string filePath, Action<FileChangeEvent> onChange)
        {
            var watcher = new FileWatcher(filePath, onChange);
            await watcher.StartWatchingAsync();
            return watcher;
        }

        public async Task StopWatchingFileAsync(string filePath)
        {
            await Task.CompletedTask;
        }
    }

    public class FileInfo
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public long Size { get; set; }
        public DateTime LastModified { get; set; }
        public bool IsDirectory { get; set; }
        public string Extension { get; set; }
    }

    public class FileWatcher
    {
        private readonly string _filePath;
        private readonly Action<FileChangeEvent> _onChange;
        private FileSystemWatcher _watcher;

        public FileWatcher(string filePath, Action<FileChangeEvent> onChange)
        {
            _filePath = filePath;
            _onChange = onChange;
        }

        public async Task StartWatchingAsync()
        {
            try
            {
                var directory = Path.GetDirectoryName(_filePath);
                var fileName = Path.GetFileName(_filePath);
                
                _watcher = new FileSystemWatcher(directory, fileName)
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName
                };
                
                _watcher.Changed += OnFileChanged;
                _watcher.Deleted += OnFileDeleted;
                _watcher.Renamed += OnFileRenamed;
                
                _watcher.EnableRaisingEvents = true;
            }
            catch (Exception)
            {
                // Handle error
            }
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            _onChange?.Invoke(new FileChangeEvent
            {
                Type = FileChangeType.Changed,
                FilePath = e.FullPath,
                Timestamp = DateTime.Now
            });
        }

        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            _onChange?.Invoke(new FileChangeEvent
            {
                Type = FileChangeType.Deleted,
                FilePath = e.FullPath,
                Timestamp = DateTime.Now
            });
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            _onChange?.Invoke(new FileChangeEvent
            {
                Type = FileChangeType.Renamed,
                FilePath = e.FullPath,
                OldFilePath = e.OldFullPath,
                Timestamp = DateTime.Now
            });
        }

        public void StopWatching()
        {
            _watcher?.Dispose();
        }
    }

    public class FileChangeEvent
    {
        public FileChangeType Type { get; set; }
        public string FilePath { get; set; }
        public string OldFilePath { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public enum FileChangeType
    {
        Changed,
        Created,
        Deleted,
        Renamed
    }
}
