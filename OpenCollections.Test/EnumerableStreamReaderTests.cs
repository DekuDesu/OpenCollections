﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using OpenCollections;
using System.IO;

namespace OpenCollections.Test
{
    public class EnumerableStreamReaderTests
    {
        Random Generator = new Random();

        string Path = $@"Example{nameof(EnumerableStreamReaderTests)}.txt";

        [Fact]
        public void ProperlyDisposesFileHandle()
        {
            CreateTestFile();
            var reader = new EnumerableStreamReader(Path);

            // ↓↓↓↓ this is intentionally written to hang the file handle do not use this in production
            IEnumerator<string> enumerator = reader.ReadLine().GetEnumerator();

            enumerator.MoveNext();

            _ = enumerator.Current;
            // ↑↑↑↑ this is intentionally written to hang the file handle do not use this in production

            // local function that throws an IO exception if a file handle is in use, otherwise returns null
            int? AttemptToAccessFile()
            {
                using (FileStream stream = new FileInfo(Path).Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
                return null;
            }

            // since we improperly accessed the enumerator, this should throw
            Assert.Throws<IOException>(() => AttemptToAccessFile());

            // dispose the object
            enumerator.Dispose();

            // make sure the disposale works and releases the file handle
            Assert.Null(AttemptToAccessFile());

            // use the reader properly as its intended
            foreach (var item in new EnumerableStreamReader(Path))
            {
                _ = item;
            }

            // make sure the file handle is cleared
            Assert.Null(AttemptToAccessFile());
        }

        [Fact]
        public void ThrowsFileNotFoundException()
        {
            string ShouldThrow()
            {
                foreach (var item in new EnumerableStreamReader("NonExistentFile.txt"))
                {
                    return item;
                }
                return null;
            }
            Assert.Throws<FileNotFoundException>(ShouldThrow);
        }

        [Fact]
        public void ReadLineEnumeratesFIFO()
        {
            IEnumerable<string> expected = CreateTestFile();

            //EnumerableStreamReader reader = new EnumerableStreamReader(Path);

            List<string> actual = new List<string>();

            foreach (var item in new EnumerableStreamReader(Path))
            {
                actual.Add(item);
            }

            (bool result, string error) = Helpers.VerifyCollection(expected, actual);

            Assert.True(result, error);
        }

        private IEnumerable<string> CreateTestFile(IEnumerable<string> TestData = null)
        {
            TestData = TestData ?? GetRandomNumberList();

            if (File.Exists(Path))
            {
                File.Delete(Path);
            }
            using (var writer = File.CreateText(Path))
            {
                foreach (var item in TestData)
                {
                    writer.WriteLine(item);
                }
            }
            return TestData;
        }

        private List<string> GetRandomNumberList(int maxNumbers = 500)
        {
            List<string> numbers = new List<string>();
            for (int i = 0; i < maxNumbers; i++)
            {
                numbers.Add($"{i}");
            }
            return numbers;
        }
    }
}
