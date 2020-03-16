using System;

namespace FileSystem
{
    public class FileSystemVisitorEventArgs : EventArgs
    {
        private FileSystemVisitorStatusController _statusController;
        public string ElementName { get; private set; }

        public FileSystemVisitorEventArgs(string elementName, FileSystemVisitorStatusController statusController)
        {
            ElementName = elementName;
            _statusController = statusController;
        }

        public void SetStop()
        {
            _statusController.Stop();
        }

        public void SetPass()
        {
            _statusController.Pass();
        }
    }
}
