using System.Collections.Generic;
using System.IO;

namespace FileSystem
{
    public interface IDirectory
    {
        IEnumerable<string> EnumerateFiles(string path);
        IEnumerable<string> EnumerateFolders(string path);
    }

    public class StandartDirectoryViewer : IDirectory
    {
        public IEnumerable<string> EnumerateFiles(string path)
        {
            return Directory.EnumerateFiles(path);
        }

        public IEnumerable<string> EnumerateFolders(string path)
        {
            return Directory.EnumerateDirectories(path);
        }
    }
}