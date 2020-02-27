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

    public class FileSystemVisitorEventArgs : EventArgs
    {
        public FileSystemVisitorEventArgs(string elementName, FileSystemVisitorStatusController statusController)
        {
            ElementName = elementName;
            _statusController = statusController;
        }

        private FileSystemVisitorStatusController _statusController;

        public string ElementName { get; private set; }

        public void SetStop()
        {
            _statusController.Stop();
        }

        public void SetPass()
        {
            _statusController.Pass();
        }

    }

    public class FileSystemVisitor
    {
        public event EventHandler OnStart;
        public event EventHandler OnFinish;
        public event EventHandler<FileSystemVisitorEventArgs> FileFinded;
        public event EventHandler<FileSystemVisitorEventArgs> FolderFinded;
        public event EventHandler<FileSystemVisitorEventArgs> FilteredFileFinded;
        public event EventHandler<FileSystemVisitorEventArgs> FilteredFolderFinded;
        private FileSystemVisitorStatusController _statusController;
        private readonly FilterDelegate _filter;
        private readonly string _path;

        public FileSystemVisitor(string path)
        {
            _path = path;
            _statusController = new FileSystemVisitorStatusController();
        }
        public FileSystemVisitor(string path, FilterDelegate filter):this(path)
        {
            _filter = filter;
        }

        private IEnumerable<string> RecursiveFind(string path)
        {
            var folders = Directory.EnumerateDirectories(path);
            var files = Directory.EnumerateFiles(path).Select(fileName=> GetFilteredFileOrNull(fileName)).Where(x=>x!=null);

            foreach (var file in files)
            {
                yield return file;
            }

            foreach (string folder in folders)
            {
                var folderName = GetFilteredFolderOrNull(folder);
                if (folderName != null)
                {
                    yield return folderName;
                }

                foreach (string subFolder in RecursiveFind(folder))
                {
                    yield return GetOnlyName(subFolder);
                }
            }
        }
   
        public IEnumerable<string> Find()
        {
            OnStart?.Invoke(this,new EventArgs());

            foreach (var element in RecursiveFind(_path))
            {
                if (_statusController.IsWorked) yield return element;
                if (_statusController.IsStopped) yield break;
                if (_statusController.IsPassed) _statusController.Work();
            }
            OnFinish?.Invoke(this, new EventArgs()); 
        }

        private string GetOnlyName(string element)
        {
            return element.Substring(element.LastIndexOf("\\") + 1);
        }

        private string GetFilteredFileOrNull(string file)
        {
            var result = GetOnlyName(file);
            FileFinded?.Invoke(this, new FileSystemVisitorEventArgs(result,_statusController));
            if (_filter == null) 
                return result;
            else if (_filter(result))
            {
                FilteredFileFinded?.Invoke(this, new FileSystemVisitorEventArgs(result, _statusController));
                return result;
            }
            else return null;
        }

        private string GetFilteredFolderOrNull(string folder)
        {
            var result = GetOnlyName(folder);
            FolderFinded?.Invoke(this, new FileSystemVisitorEventArgs(result, _statusController));
            if (_filter == null)
                return result;
            else if (_filter(result))
            {
                FilteredFolderFinded?.Invoke(this, new FileSystemVisitorEventArgs(result, _statusController));
                return result;
            }
            else return null;
        }
    }
}
