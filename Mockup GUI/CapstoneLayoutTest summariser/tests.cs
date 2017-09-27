using CapstoneLayoutTest;
using System;
using System.IO;
using Xunit;

namespace MyFirstUnitTests
{
    public class Class1
    {
        const string testFilesLocation = "../../TestFiles/";

        [Fact]
        public void NullCSVTest()
        {
            CSVDatasetLoader loader = new CSVDatasetLoader(testFilesLocation + "EmptyTest.csv");
            Assert.Equal(loader.GenerateDataset("testData").Nodes.Count, 0);
        }
        [Fact]
        public void CSVNameSetTest()
        {
            CSVDatasetLoader loader = new CSVDatasetLoader(testFilesLocation + "EmptyTest.csv");
            Assert.Equal(loader.GenerateDataset("testData").DatasetName, "testData");
        }
        [Fact]
        public void NotNullCSVTest()
        {
            CSVDatasetLoader loader = new CSVDatasetLoader(testFilesLocation + "SimpleTest.csv");
            Assert.Equal(loader.GenerateDataset("testData").Nodes.Count, 19);
            //test things other than count
            //test the other functions AppendSortedEndList and AppendSortedStartList
        }
        [Fact]
        public void NullNameCSVTest()
        {
            var exception = Record.Exception(() => new CSVDatasetLoader(null));
            Assert.IsType(typeof(ArgumentNullException), exception);
        }
        [Fact]
        public void NonExistCSVTest()
        {
            var exception = Record.Exception(() => new CSVDatasetLoader(testFilesLocation + "DosentExist.csv"));
            Assert.IsType(typeof(FileNotFoundException), exception);
        }
        [Fact]
        public void CSVBeingUsedTest()
        {
            using (var filestream = File.Open(testFilesLocation + "SimpleTest.csv", FileMode.Open))
            {
                var exception = Record.Exception(() => new CSVDatasetLoader(testFilesLocation + "SimpleTest.csv"));
                Assert.IsType(typeof(IOException), exception);
            }
        }

    }
}