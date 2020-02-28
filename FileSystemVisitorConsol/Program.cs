using FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileSystemVisitorConsol
{
    class ConsoleListener
    {
        public event EventHandler UpPress;
        public event EventHandler DownPress;

        public void ReadKeys()
        {
            ConsoleKeyInfo key = new ConsoleKeyInfo();
            while (!Console.KeyAvailable && key.Key != ConsoleKey.Escape)
            {

                key = Console.ReadKey(true);

                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        UpPress?.Invoke(this, new EventArgs());
                        break;
                    case ConsoleKey.DownArrow:
                        DownPress?.Invoke(this, new EventArgs());
                        break;
                }
            }
        }
    }

    class Program
    {
        private static FileSystemVisitor fileSystemVisitor = new FileSystemVisitor("C:\\", x => x.Length < 100);

        static void Main(string[] args)
        {
            ConsoleListener consoleListener = new ConsoleListener();
            consoleListener.UpPress += ConsoleListener_UpPress;
            consoleListener.DownPress += ConsoleListener_DownPress;
            fileSystemVisitor.OnStart += A_OnStart;
            fileSystemVisitor.OnFinish += A_OnFinish;
            fileSystemVisitor.FolderFinded += A_FolderFinded;
            fileSystemVisitor.FilteredFolderFinded += A_FilteredFolderFinded;

            Task taskKeys = new Task(consoleListener.ReadKeys);
            taskKeys.Start();
            Process();
        }

        private static void Process()
        {
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

        private static void ConsoleListener_UpPress(object sender, EventArgs e)
        {
            fileSystemVisitor.StopFind();
        }

        private static void ConsoleListener_DownPress(object sender, EventArgs e)
        {
            fileSystemVisitor.PassFind();
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
