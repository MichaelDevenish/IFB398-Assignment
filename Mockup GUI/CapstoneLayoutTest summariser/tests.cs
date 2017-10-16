using CapstoneLayoutTest;
using CapstoneLayoutTest.Helper_Functions;
using DataGraph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace CapstoneUnitTests
{
    public class Class1
    {
        const string testFilesLocation = "../../TestFiles/";
        #region Helpers
        public static Task StartSTATask(Action action)
        {
            var tcs = new TaskCompletionSource<object>();
            var thread = new Thread(() =>
            {
                try
                {
                    action();
                    tcs.SetResult(new object());
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }
        #endregion
        #region CSVDatasetLoader
        #region Simple Tests
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
        #endregion
        #region sorted list tests
        [Fact]
        public void SimpleStartTest()
        {
            CSVDatasetLoader loader = new CSVDatasetLoader(testFilesLocation + "SimpleTest.csv");
            Assert.Equal(loader.AppendSortedStartList(new List<double>()), new List<double>() { 0, 30, 44.9, 60, 90, 120, 150, 180, 210, 240, 270, 300, 360, 390, 420, 450, 480, 510, 540 });
        }
        [Fact]
        public void SimpleEndTest()
        {
            CSVDatasetLoader loader = new CSVDatasetLoader(testFilesLocation + "SimpleTest.csv");
            Assert.Equal(loader.AppendSortedEndList(new List<double>()), new List<double>() { 29.9, 41.7, 59.9, 89.9, 118.8, 148.8, 178.8, 209.9, 238.8, 269.9, 298.8, 329.9, 389.9, 418.8, 449.9, 478.8, 500.9, 538.8, 557.6 });
        }
        [Fact]
        public void UnsortedStartTest()
        {
            CSVDatasetLoader loader = new CSVDatasetLoader(testFilesLocation + "outoforder.csv");
            Assert.Equal(loader.AppendSortedStartList(new List<double>()), new List<double>() { 0, 95, 109.5, 119.6, 162.8, 183.1 });
        }
        [Fact]
        public void UnsortedEndTest()
        {
            CSVDatasetLoader loader = new CSVDatasetLoader(testFilesLocation + "outoforder.csv");
            Assert.Equal(loader.AppendSortedEndList(new List<double>()), new List<double>() { 75.3, 103, 118, 158.6, 178.9, 225.3 });
        }
        [Fact]
        public void AddStartTest()
        {
            List<double> list = new List<double>();
            CSVDatasetLoader loader = new CSVDatasetLoader(testFilesLocation + "copyTest.csv");
            CSVDatasetLoader loader2 = new CSVDatasetLoader(testFilesLocation + "copyTest2.csv");
            list = loader.AppendSortedStartList(list);
            Assert.Equal(loader2.AppendSortedStartList(list), new List<double>() { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 });
        }
        [Fact]
        public void AddEndTest()
        {
            List<double> list = new List<double>();
            CSVDatasetLoader loader = new CSVDatasetLoader(testFilesLocation + "copyTest.csv");
            CSVDatasetLoader loader2 = new CSVDatasetLoader(testFilesLocation + "copyTest2.csv");
            list = loader.AppendSortedEndList(list);
            Assert.Equal(loader2.AppendSortedEndList(list), new List<double>() { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 });
        }
        #endregion
        #region GenerateDataset tests
        [Fact]
        public void CorrectNameCSVTest()
        {
            CSVDatasetLoader loader = new CSVDatasetLoader(testFilesLocation + "SimpleTest.csv");
            Assert.Equal(loader.GenerateDataset("testData").Nodes[0].NodeName, "Longboarding");
            Assert.Equal(loader.GenerateDataset("testData").Nodes[1].NodeName, "Longboarding");
            Assert.Equal(loader.GenerateDataset("testData").Nodes[2].NodeName, "Longboarding");
            Assert.Equal(loader.GenerateDataset("testData").Nodes[3].NodeName, "BMX");
            Assert.Equal(loader.GenerateDataset("testData").Nodes[4].NodeName, "BMX");
            Assert.Equal(loader.GenerateDataset("testData").Nodes[5].NodeName, "Doing motocross");
            Assert.Equal(loader.GenerateDataset("testData").Nodes[6].NodeName, "Bungee jumping");
            Assert.Equal(loader.GenerateDataset("testData").Nodes[7].NodeName, "Tumbling");
            Assert.Equal(loader.GenerateDataset("testData").Nodes[8].NodeName, "Tumbling");
            Assert.Equal(loader.GenerateDataset("testData").Nodes[9].NodeName, "Belly dance");
            Assert.Equal(loader.GenerateDataset("testData").Nodes[10].NodeName, "Fixing the roof");
            Assert.Equal(loader.GenerateDataset("testData").Nodes[11].NodeName, "Bungee jumping");
            Assert.Equal(loader.GenerateDataset("testData").Nodes[12].NodeName, "Bungee jumping");
            Assert.Equal(loader.GenerateDataset("testData").Nodes[13].NodeName, "Bungee jumping");
            Assert.Equal(loader.GenerateDataset("testData").Nodes[14].NodeName, "Rollerblading");
            Assert.Equal(loader.GenerateDataset("testData").Nodes[15].NodeName, "Skateboarding");
            Assert.Equal(loader.GenerateDataset("testData").Nodes[16].NodeName, "Surfing");
            Assert.Equal(loader.GenerateDataset("testData").Nodes[17].NodeName, "Surfing");
            Assert.Equal(loader.GenerateDataset("testData").Nodes[18].NodeName, "Surfing");
        }
        [Fact]
        public void CorrectSameNameCSVTest()
        {
            CSVDatasetLoader loader = new CSVDatasetLoader(testFilesLocation + "sameTest.csv");
            Assert.Equal(loader.GenerateDataset("testData").Nodes[0].NodeName, "Hurling");
            Assert.Equal(loader.GenerateDataset("testData").Nodes[1].NodeName, "Hurling");
            Assert.Equal(loader.GenerateDataset("testData").Nodes[2].NodeName, "Hurling");
            Assert.Equal(loader.GenerateDataset("testData").Nodes[3].NodeName, "Hurling");
            Assert.Equal(loader.GenerateDataset("testData").Nodes[4].NodeName, "Hurling");
            Assert.Equal(loader.GenerateDataset("testData").Nodes[5].NodeName, "Hurling");
        }
        [Fact]
        public void CorrectValueCSVTest()
        {
            CSVDatasetLoader loader = new CSVDatasetLoader(testFilesLocation + "sameTest.csv");
            Assert.Equal(((SummariserNode)loader.GenerateDataset("testData").Nodes[0]).Value(), new double[] { 0, 75.3 });
            Assert.Equal(((SummariserNode)loader.GenerateDataset("testData").Nodes[1]).Value(), new double[] { 95, 103 });
            Assert.Equal(((SummariserNode)loader.GenerateDataset("testData").Nodes[2]).Value(), new double[] { 109.5, 118 });
            Assert.Equal(((SummariserNode)loader.GenerateDataset("testData").Nodes[3]).Value(), new double[] { 119.6, 158.6 });
            Assert.Equal(((SummariserNode)loader.GenerateDataset("testData").Nodes[4]).Value(), new double[] { 162.8, 178.9 });
            Assert.Equal(((SummariserNode)loader.GenerateDataset("testData").Nodes[5]).Value(), new double[] { 183.1, 225.3 });

        }
        [Fact]
        public void CorrectColourCSVTest()
        {
            CSVDatasetLoader loader = new CSVDatasetLoader(testFilesLocation + "colourTest.csv");
            Assert.Equal(((SolidColorBrush)((SummariserNode)loader.GenerateDataset("testData").Nodes[0]).Colour).Color, Color.FromRgb(255, 0, 0));
            Assert.Equal(((SolidColorBrush)((SummariserNode)loader.GenerateDataset("testData").Nodes[1]).Colour).Color, Color.FromRgb(255, 127, 0));
            Assert.Equal(((SolidColorBrush)((SummariserNode)loader.GenerateDataset("testData").Nodes[2]).Colour).Color, Color.FromRgb(255, 255, 0));
            Assert.Equal(((SolidColorBrush)((SummariserNode)loader.GenerateDataset("testData").Nodes[3]).Colour).Color, Color.FromRgb(127, 255, 0));
            Assert.Equal(((SolidColorBrush)((SummariserNode)loader.GenerateDataset("testData").Nodes[4]).Colour).Color, Color.FromRgb(0, 255, 0));
        }
        [Fact]
        public void CorrectOrderValueCSVTest()
        {
            CSVDatasetLoader loader = new CSVDatasetLoader(testFilesLocation + "outoforder.csv");
            Assert.Equal(((SummariserNode)loader.GenerateDataset("testData").Nodes[0]).Value(), new double[] { 95, 103 });
            Assert.Equal(((SummariserNode)loader.GenerateDataset("testData").Nodes[1]).Value(), new double[] { 119.6, 158.6 });
            Assert.Equal(((SummariserNode)loader.GenerateDataset("testData").Nodes[2]).Value(), new double[] { 109.5, 118 });
            Assert.Equal(((SummariserNode)loader.GenerateDataset("testData").Nodes[3]).Value(), new double[] { 0, 75.3 });
            Assert.Equal(((SummariserNode)loader.GenerateDataset("testData").Nodes[4]).Value(), new double[] { 162.8, 178.9 });
            Assert.Equal(((SummariserNode)loader.GenerateDataset("testData").Nodes[5]).Value(), new double[] { 183.1, 225.3 });
        }
        [Fact]
        public void SameValueCSVTest()
        {
            CSVDatasetLoader loader = new CSVDatasetLoader(testFilesLocation + "cloned.csv");
            Assert.Equal(((SummariserNode)loader.GenerateDataset("testData").Nodes[0]).Value(), new double[] { 95, 103 });
            Assert.Equal(((SummariserNode)loader.GenerateDataset("testData").Nodes[1]).Value(), new double[] { 95, 103 });
            Assert.Equal(((SummariserNode)loader.GenerateDataset("testData").Nodes[2]).Value(), new double[] { 95, 103 });
            Assert.Equal(((SummariserNode)loader.GenerateDataset("testData").Nodes[3]).Value(), new double[] { 95, 103 });
            Assert.Equal(((SummariserNode)loader.GenerateDataset("testData").Nodes[4]).Value(), new double[] { 95, 103 });
        }
        #endregion
        #endregion
        #region DataManager Tests
        #region CreateTestListView testers
        [Fact]
        public async Task LviewCreationTestName()
        {
            await StartSTATask(() =>
            {
                ListView lview = DataManager.CreateListView();
                lview.Items.Add(new VideoData { Name = "test1", URL = "testurl1" });
                lview.Items.Add(new VideoData { Name = "test2", URL = "testurl2" });
                lview.Items.Add(new VideoData { Name = "test3", URL = "testurl3" });
                lview.Items.Add(new VideoData { Name = "test4", URL = "testurl4" });
                lview.Items.Add(new VideoData { Name = "test5", URL = "testurl5" });
                Assert.Equal("test1", (lview.Items.GetItemAt(0) as VideoData).Name);
            });
        }
        [Fact]
        public async Task LviewCreationTestURL()
        {
            await StartSTATask(() =>
            {
                ListView lview = DataManager.CreateListView();
                lview.Items.Add(new VideoData { Name = "test1", URL = "testurl1" });
                lview.Items.Add(new VideoData { Name = "test2", URL = "testurl2" });
                lview.Items.Add(new VideoData { Name = "test3", URL = "testurl3" });
                lview.Items.Add(new VideoData { Name = "test4", URL = "testurl4" });
                lview.Items.Add(new VideoData { Name = "test5", URL = "testurl5" });
                Assert.Equal("testurl1", (lview.Items.GetItemAt(0) as VideoData).URL);
            });
        }
        [Fact]
        public async Task LviewCreationTestEnd()
        {
            await StartSTATask(() =>
            {
                ListView lview = DataManager.CreateListView();
                lview.Items.Add(new VideoData { Name = "test1", URL = "testurl1" });
                lview.Items.Add(new VideoData { Name = "test2", URL = "testurl2" });
                lview.Items.Add(new VideoData { Name = "test3", URL = "testurl3" });
                lview.Items.Add(new VideoData { Name = "test4", URL = "testurl4" });
                lview.Items.Add(new VideoData { Name = "test5", URL = "testurl5" });
                Assert.Equal("test5", (lview.Items.GetItemAt(4) as VideoData).Name);
            });
        }
        [Fact]
        public async Task LviewCreationTestURLEnd()
        {
            await StartSTATask(() =>
            {
                ListView lview = DataManager.CreateListView();
                lview.Items.Add(new VideoData { Name = "test1", URL = "testurl1" });
                lview.Items.Add(new VideoData { Name = "test2", URL = "testurl2" });
                lview.Items.Add(new VideoData { Name = "test3", URL = "testurl3" });
                lview.Items.Add(new VideoData { Name = "test4", URL = "testurl4" });
                lview.Items.Add(new VideoData { Name = "test5", URL = "testurl5" });
                Assert.Equal("testurl5", (lview.Items.GetItemAt(4) as VideoData).URL);
            });
        }
        #endregion
        [Fact]
        public async Task DataManagerTest()
        {
            await StartSTATask(() =>
            {
                ListView lview = DataManager.CreateListView();
                lview.Items.Add(new VideoData { Name = "test1", URL = testFilesLocation + "cloned.csv" });
                lview.Items.Add(new VideoData { Name = "test2", URL = testFilesLocation + "sameTest.csv" });
                lview.Items.Add(new VideoData { Name = "test3", URL = testFilesLocation + "outoforder.csv" });
                lview.Items.Add(new VideoData { Name = "test4", URL = testFilesLocation + "EmptyTest.csv" });
                lview.Items.Add(new VideoData { Name = "test5", URL = testFilesLocation + "colourTest.csv" });
                DataManager.SaveFile("test1.txt", lview);
                ListView lviewLoad = DataManager.CreateListView();
                DataManager.LoadFile("test1.txt", lviewLoad);
                File.Delete("test1.txt");
                Assert.Equal(lview.Items.Count, lviewLoad.Items.Count);
            });
        }
        [Fact]
        public async Task DataManagerOverwriteTest()
        {
            await StartSTATask(() =>
            {
                ListView lview = DataManager.CreateListView();
                lview.Items.Add(new VideoData { Name = "test1", URL = testFilesLocation + "cloned.csv" });
                lview.Items.Add(new VideoData { Name = "test2", URL = testFilesLocation + "sameTest.csv" });
                lview.Items.Add(new VideoData { Name = "test3", URL = testFilesLocation + "outoforder.csv" });
                lview.Items.Add(new VideoData { Name = "test4", URL = testFilesLocation + "EmptyTest.csv" });
                lview.Items.Add(new VideoData { Name = "test5", URL = testFilesLocation + "colourTest.csv" });
                DataManager.SaveFile("test1.txt", lview);
                lview.Items.RemoveAt(3);
                DataManager.SaveFile("test1.txt", lview);
                ListView lviewLoad = DataManager.CreateListView();
                DataManager.LoadFile("test1.txt", lviewLoad);
                File.Delete("test1.txt");
                Assert.Equal(lview.Items.Count, lviewLoad.Items.Count);
            });
        }
        [Fact]
        public async Task DataManagerNonExistAtIndexTest()
        {
            await StartSTATask(() =>
            {
                ListView lview = DataManager.CreateListView();
                lview.Items.Add(new VideoData { Name = "test1", URL = testFilesLocation + "cloned.csv" });
                lview.Items.Add(new VideoData { Name = "test2", URL = testFilesLocation + "sameTest.csv" });
                lview.Items.Add(new VideoData { Name = "test3", URL = testFilesLocation + "dosentExist.csv" });
                lview.Items.Add(new VideoData { Name = "test4", URL = testFilesLocation + "EmptyTest.csv" });
                DataManager.SaveFile("test1.txt", lview);
                ListView lviewLoad = DataManager.CreateListView();
                DataManager.LoadFile("test1.txt", lviewLoad);
                File.Delete("test1.txt");
                Assert.Equal((lviewLoad.Items.GetItemAt(2) as VideoData).Name, "test4");
            });
        }
        [Fact]
        public async Task DataManagerNonExistCountTest()
        {
            await StartSTATask(() =>
            {
                ListView lview = DataManager.CreateListView();
                lview.Items.Add(new VideoData { Name = "test1", URL = testFilesLocation + "cloned.csv" });
                lview.Items.Add(new VideoData { Name = "test2", URL = testFilesLocation + "sameTest.csv" });
                lview.Items.Add(new VideoData { Name = "test3", URL = testFilesLocation + "dosentExist.csv" });
                lview.Items.Add(new VideoData { Name = "test4", URL = testFilesLocation + "EmptyTest.csv" });
                DataManager.SaveFile("test1.txt", lview);
                ListView lviewLoad = DataManager.CreateListView();
                DataManager.LoadFile("test1.txt", lviewLoad);
                File.Delete("test1.txt");
                Assert.Equal(lview.Items.Count - 1, lviewLoad.Items.Count);
            });
        }
        [Fact]
        public async Task DataManagerNonExistLastAtIndexTest()
        {
            await StartSTATask(() =>
            {
                ListView lview = DataManager.CreateListView();
                lview.Items.Add(new VideoData { Name = "test1", URL = testFilesLocation + "cloned.csv" });
                lview.Items.Add(new VideoData { Name = "test2", URL = testFilesLocation + "sameTest.csv" });
                lview.Items.Add(new VideoData { Name = "test3", URL = testFilesLocation + "EmptyTest.csv" });
                lview.Items.Add(new VideoData { Name = "test4", URL = testFilesLocation + "dosentExist.csv" });
                DataManager.SaveFile("test1.txt", lview);
                ListView lviewLoad = DataManager.CreateListView();
                DataManager.LoadFile("test1.txt", lviewLoad);
                File.Delete("test1.txt");
                var exception = Record.Exception(() => lviewLoad.Items.GetItemAt(3));
                Assert.IsType(typeof(ArgumentOutOfRangeException), exception);
            });
        }
        #endregion
        #region ControlBarHelper Tests
        #region PausePlay
        [Fact]
        public async Task PauseTest()
        {
            await StartSTATask(() =>
            {
                Image i = new Image();
                ControlBarHelper.SetPausePlayImage(true, i);
                Assert.Equal(new Uri(@"/CapstoneLayoutTest;component/Images/ic_pause_white_24dp.png", UriKind.Relative), (i.Source as BitmapImage).UriSource);
            });
        }
        [Fact]
        public async Task PlayTest()
        {
            await StartSTATask(() =>
            {
                Image i = new Image();
                ControlBarHelper.SetPausePlayImage(false, i);
                Assert.Equal(new Uri(@"/CapstoneLayoutTest;component/Images/ic_play_arrow_white_24dp.png", UriKind.Relative), (i.Source as BitmapImage).UriSource);
            });
        }
        [Fact]
        public async Task PlaySwitchTest()
        {
            await StartSTATask(() =>
            {
                Image i = new Image();
                ControlBarHelper.SetPausePlayImage(true, i);
                ControlBarHelper.SetPausePlayImage(false, i);
                Assert.Equal(new Uri(@"/CapstoneLayoutTest;component/Images/ic_play_arrow_white_24dp.png", UriKind.Relative), (i.Source as BitmapImage).UriSource);
            });
        }

        [Fact]
        public async Task PauseSwitchTest()
        {
            await StartSTATask(() =>
            {
                Image i = new Image();
                ControlBarHelper.SetPausePlayImage(false, i);
                ControlBarHelper.SetPausePlayImage(true, i);
                Assert.Equal(new Uri(@"/CapstoneLayoutTest;component/Images/ic_pause_white_24dp.png", UriKind.Relative), (i.Source as BitmapImage).UriSource);
            });
        }
        #endregion
        #region time
        [Fact]
        public void NoTime()
        {
            Assert.Equal(ControlBarHelper.IntToTimeString(0), "00:00");
        }
        [Fact]
        public void Seconds()
        {
            Assert.Equal(ControlBarHelper.IntToTimeString(1), "00:01");
            Assert.Equal(ControlBarHelper.IntToTimeString(10), "00:10");
            Assert.Equal(ControlBarHelper.IntToTimeString(59), "00:59");
        }

        [Fact]
        public void Minutes()
        {
            Assert.Equal(ControlBarHelper.IntToTimeString(60), "01:00");
            Assert.Equal(ControlBarHelper.IntToTimeString(600), "10:00");
            Assert.Equal(ControlBarHelper.IntToTimeString(660), "11:00");
            Assert.Equal(ControlBarHelper.IntToTimeString(3540), "59:00");

        }
        [Fact]
        public void Hours()
        {
            Assert.Equal(ControlBarHelper.IntToTimeString(3600), "1:00:00");
            Assert.Equal(ControlBarHelper.IntToTimeString(36000), "10:00:00");
            Assert.Equal(ControlBarHelper.IntToTimeString(39600), "11:00:00");
            Assert.Equal(ControlBarHelper.IntToTimeString(360000), "100:00:00");
            Assert.Equal(ControlBarHelper.IntToTimeString(18000), "5:00:00");
        }
        [Fact]
        public void SecondsAndMinutes()
        {
            Assert.Equal(ControlBarHelper.IntToTimeString(61), "01:01");
            Assert.Equal(ControlBarHelper.IntToTimeString(70), "01:10");
            Assert.Equal(ControlBarHelper.IntToTimeString(119), "01:59");
            Assert.Equal(ControlBarHelper.IntToTimeString(610), "10:10");
            Assert.Equal(ControlBarHelper.IntToTimeString(671), "11:11");
            Assert.Equal(ControlBarHelper.IntToTimeString(3599), "59:59");
        }
        [Fact]
        public void SecondsAndMinutesAndHours()
        {
            Assert.Equal(ControlBarHelper.IntToTimeString(3661), "1:01:01");
            Assert.Equal(ControlBarHelper.IntToTimeString(40271), "11:11:11");
            Assert.Equal(ControlBarHelper.IntToTimeString(21355), "5:55:55");
        }
        [Fact]
        public void SecondsAndHours()
        {
            Assert.Equal(ControlBarHelper.IntToTimeString(3601), "1:00:01");
            Assert.Equal(ControlBarHelper.IntToTimeString(3659), "1:00:59");
            Assert.Equal(ControlBarHelper.IntToTimeString(36001), "10:00:01");
            Assert.Equal(ControlBarHelper.IntToTimeString(36059), "10:00:59");
        }
        [Fact]
        public void MinutesAndHours()
        {
            Assert.Equal(ControlBarHelper.IntToTimeString(3660), "1:01:00");
            Assert.Equal(ControlBarHelper.IntToTimeString(7140), "1:59:00");
            Assert.Equal(ControlBarHelper.IntToTimeString(36060), "10:01:00");
            Assert.Equal(ControlBarHelper.IntToTimeString(39540), "10:59:00");
        }
        #endregion
        #endregion
    }
}