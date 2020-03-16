using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileSystem
{
    public delegate bool FilterDelegate(string element);

    public class FileSystemVisitorStatusController
    {
        private enum FileSystemVisitorStatus
        { 
            Work,Stop,Pass
        }

        private FileSystemVisitorStatus currentStatus = FileSystemVisitorStatus.Work;

        public void Work()
        {
            currentStatus = FileSystemVisitorStatus.Work;
        }

        public void Pass()
        {
            currentStatus = FileSystemVisitorStatus.Pass;
        }

        public void Stop()
        {
            currentStatus = FileSystemVisitorStatus.Stop;
        }

        public bool IsWorked
        {
            get
            {
                return currentStatus == FileSystemVisitorStatus.Work;
            }
        }

        public bool IsPassed
        {
            get
            {
                return currentStatus == FileSystemVisitorStatus.Pass;
            }
        }

        public bool IsStopped
        {
            get
            {
                return currentStatus == FileSystemVisitorStatus.Stop;
            }
        }
    }

    public class FileSystemVisitor
    {
        private FileSystemVisitorStatusController _statusController= new FileSystemVisitorStatusController();
        private IDirectory _directory=new StandartDirectoryViewer();
        private readonly FilterDelegate _filter;
        private readonly string _path;

        public FileSystemVisitor(string path)
        {
            _path = path;
        }

        public FileSystemVisitor(string path, FilterDelegate filter) : this(path)
        {
            _filter = filter;
        }

        public FileSystemVisitor(IDirectory directory, string path, FilterDelegate filter):this(path,filter)
        {
            _directory = directory;
            _filter = filter;
        }

        public event EventHandler<EventArgs> Start;
        public event EventHandler<EventArgs> Finish;
        public event EventHandler<FileSystemVisitorEventArgs> FileFinded;
        public event EventHandler<FileSystemVisitorEventArgs> FolderFinded;
        public event EventHandler<FileSystemVisitorEventArgs> FilteredFileFinded;
        public event EventHandler<FileSystemVisitorEventArgs> FilteredFolderFinded;

        protected virtual void OnStart()
        {
            new EventArgs().Raise(this, ref Start);
        }
        protected virtual void OnFinish()
        {
            new EventArgs().Raise(this, ref Finish);
        }
        protected virtual void OnFileFinded(FileSystemVisitorEventArgs e)
        {
            e.Raise(this, ref FileFinded);
        }

        protected virtual void OnFolderFinded(FileSystemVisitorEventArgs e)
        {
            e.Raise(this, ref FolderFinded);
        }

        protected virtual void OnFilteredFileFinded(FileSystemVisitorEventArgs e)
        {
            e.Raise(this, ref FilteredFileFinded);
        }

        protected virtual void OnFilteredFolderFinded(FileSystemVisitorEventArgs e)
        {
            e.Raise(this, ref FilteredFolderFinded);
        }

        private IEnumerable<string> RecursiveFind(string path)
        {
            var folders = _directory.EnumerateFolders(path);
            var files = _directory.EnumerateFiles(path).Select(fileName=> GetFilteredOrNull<FileInfo>(fileName)).Where(x=>x!=null);

            foreach (var file in files)
            {
                yield return file;
            }

            foreach (string folder in folders)
            {
                var folderName = GetFilteredOrNull<DirectoryInfo>(folder);
                if (folderName != null)
                {
                    yield return folderName;
                }

                foreach (string subFolder in RecursiveFind(folder))
                {
                    yield return subFolder;
                }
            }
        }
   
        public IEnumerable<string> Find()
        {
            OnStart();
            
            foreach (var element in RecursiveFind(_path))
            {
                if (_statusController.IsWorked) yield return element;
                if (_statusController.IsStopped) yield break;
                if (_statusController.IsPassed) _statusController.Work();
            }
            OnFinish(); 
        }

        public void StopFind()
        {
            _statusController.Stop();
        }
        public void PassFind()
        {
            _statusController.Pass();
        }

        private string GetOnlyName(string element)
        {
            return element.Substring(element.LastIndexOf("\\") + 1);
        }

        private string GetFilteredOrNull<T>(string element)
        {
            var result = GetOnlyName(element);

            if (typeof(T) == typeof(FileInfo))
                OnFileFinded(new FileSystemVisitorEventArgs(result, _statusController));
            else if (typeof(T) == typeof(DirectoryInfo))
                OnFolderFinded(new FileSystemVisitorEventArgs(result, _statusController));
            else
                throw new Exception("Generic type is not correct");

            if (_filter == null) 
                return result;
            else if (_filter(result))
            {
                if (typeof(T) == typeof(FileInfo))
                    OnFilteredFileFinded(new FileSystemVisitorEventArgs(result, _statusController));
                else if (typeof(T) == typeof(DirectoryInfo))
                    OnFilteredFolderFinded(new FileSystemVisitorEventArgs(result, _statusController));
                else
                    throw new Exception("Generic type is not correct");
                return result;
            }
            else return null;
        }
    }
}
