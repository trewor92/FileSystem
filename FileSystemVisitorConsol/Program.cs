using FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileSystemVisitorConsol
{
    class Program
    {
        static void Main(string[] args)
        {
            Process();
        }

        private static void Process()
        {
            var fileSystemVisitor = new FileSystemVisitor("D:\\", x => x.Length < 100);
            fileSystemVisitor.OnStart += A_OnStart;
            fileSystemVisitor.OnFinish += A_OnFinish;
            fileSystemVisitor.FolderFinded += A_FolderFinded;
            fileSystemVisitor.FilteredFolderFinded += A_FilteredFolderFinded;

            try
            {
                foreach (var result in fileSystemVisitor.Find())
                    Console.WriteLine(result);
            }
            catch (UnauthorizedAccessException UAEx)
            {
                Console.WriteLine(UAEx.Message);
            }
            catch (PathTooLongException PathEx)
            {
                Console.WriteLine(PathEx.Message);
            }
        }

        private static void A_OnStart(object sender, EventArgs e)
        {
            Console.WriteLine("Событие OnStart");
        }

        private static void A_FilteredFolderFinded(object sender, FileSystemVisitorEventArgs e)
        {
            if (e.ElementName == "Services") 
                e.SetPass();  
            Console.WriteLine("Событие FilteredFolderFinded");
        }

        private static void A_FolderFinded(object sender, FileSystemVisitorEventArgs e)
        {
            if (e.ElementName == "1") 
                e.SetStop();
            Console.WriteLine("Событие FolderFinded");
        }

        private static void A_OnFinish(object sender, EventArgs e)
        {
            Console.WriteLine("Событие OnFinish");
        }

    }
}
