using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FileSystem.Tests
{
    public class FileSystemTests
    {
        Mock<IDirectory> _mockDirectory = new Mock<IDirectory>();
        List<string> _testsFiles0 = new List<string> { "1file", "2file" };
        List<string> _testsFiles1 = new List<string> { "3file", "4file" };
        List<string> _testsFiles2 = new List<string> { "5file", "6file", "7file" };
        List<string> _testsFolders = new List<string> { "1Folder", "2F" };


        private void MockInit()
        {
            _mockDirectory.Setup(x => x.EnumerateFolders("root")).Returns(_testsFolders);
            _mockDirectory.Setup(x => x.EnumerateFiles("root")).Returns(_testsFiles0);
            _mockDirectory.Setup(x => x.EnumerateFiles("1Folder")).Returns(_testsFiles1);
            _mockDirectory.Setup(x => x.EnumerateFiles("2F")).Returns(_testsFiles2);
            _mockDirectory.Setup(x => x.EnumerateFolders("1Folder")).Returns(new List<string>());
            _mockDirectory.Setup(x => x.EnumerateFolders("2F")).Returns(new List<string>());
        }

        [Fact]
        public void CanFindFoldersAndFiles()
        {
            MockInit();
            FileSystemVisitor fileSystemVisitor = new FileSystemVisitor(_mockDirectory.Object, "root", null);

            IEnumerable<string> allElements = _testsFiles0.Concat(_testsFiles1).Concat(_testsFiles2).Concat(_testsFolders);
            IEnumerable<string> result = fileSystemVisitor.Find().Except(allElements);
            Assert.True(result.Count() == 0);
        }

        [Fact]
        public void CanCallEvents()
        {
            MockInit();
            FileSystemVisitor fileSystemVisitor = new FileSystemVisitor(_mockDirectory.Object, "root", x=>x.Length<=5);
            List<string> actual = new List<string>();
            string[] variants = new[] { "OnStart", "OnFinish", "FileFinded", "FolderFinded", "FilteredFileFinded", "FilteredFolderFinded" };
            fileSystemVisitor.OnStart += delegate (object sender, EventArgs e)
            {
                  actual.Add(variants[0]);
            };
            fileSystemVisitor.OnFinish += delegate (object sender, EventArgs e)
            {
                  actual.Add(variants[1]);
            };
            fileSystemVisitor.FileFinded += delegate (object sender, FileSystemVisitorEventArgs e)
            {
                  actual.Add(variants[2]);
            };
            fileSystemVisitor.FolderFinded += delegate (object sender, FileSystemVisitorEventArgs e)
            {
                  actual.Add(variants[3]);
            };
            fileSystemVisitor.FilteredFileFinded += delegate (object sender, FileSystemVisitorEventArgs e)
            {
                  actual.Add(variants[4]);
            };
            fileSystemVisitor.FilteredFolderFinded += delegate (object sender, FileSystemVisitorEventArgs e)
            {
                  actual.Add(variants[5]);
            };
            fileSystemVisitor.Find().ToList();

            var result = actual.GroupBy(x => x).ToDictionary(x=>x.Key,x=>x.Count());

            Assert.Equal(1, result[variants[0]]);
            Assert.Equal(1, result[variants[1]]);
            Assert.Equal(7, result[variants[2]]);
            Assert.Equal(2, result[variants[3]]);
            Assert.Equal(7, result[variants[4]]);
            Assert.Equal(1, result[variants[5]]);//
        }

        [Fact]
        public void CanStopFinding()
        {
            MockInit();
            FileSystemVisitor fileSystemVisitor = new FileSystemVisitor(_mockDirectory.Object, "root", x => x.Length <= 5);
            int i = 0;
            fileSystemVisitor.FilteredFileFinded += delegate (object sender, FileSystemVisitorEventArgs e)
            {
                if (i++ == 3) e.SetStop();
            };
            
            var result = fileSystemVisitor.Find().ToList();

            Assert.True(result.Count()==3);
        }

        [Fact]
        public void CanPassElement()
        {
            MockInit();
            FileSystemVisitor fileSystemVisitor = new FileSystemVisitor(_mockDirectory.Object, "root", x => x.Length <= 5);           
            fileSystemVisitor.FilteredFileFinded += delegate (object sender, FileSystemVisitorEventArgs e)
            {
                if (e.ElementName=="1file") e.SetPass();
            };

            var result = fileSystemVisitor.Find().ToList();

            Assert.True(result.Count(x=>x=="1file")==0);
        }


    }
}
