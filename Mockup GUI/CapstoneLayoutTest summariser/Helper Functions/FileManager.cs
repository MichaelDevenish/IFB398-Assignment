using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CapstoneLayoutTest.Helper_Functions
{
    class FileManager
    {

        /// <summary>
        /// Pass the name of a file, e.g test.mp4, and it will get the count of files that
        /// have been created by the splitting code for that file
        /// </summary>
        /// <param name="name">the name of the file to test</param>
        /// <returns>the count of subfiles</returns>
        public static int GetNumberofSubfiles(string name, string parentDir)
        {
            List<string> countList = new List<string>();
            FileInfo[] Files = new DirectoryInfo(parentDir).GetFiles("*");
            string[] splitPath = { System.IO.Path.GetFileNameWithoutExtension(name), System.IO.Path.GetExtension(name) };
            foreach (FileInfo file in Files)
                if (file.Name.Contains(splitPath[0] + "-"))
                    countList.Add(file.Name);
            return countList.Count();
        }

        /// <summary>
        /// compresses the files that matches the name and adds them to the save list
        /// </summary>
        /// <param name="list">the save list</param>
        /// <param name="name">the name of the file to save</param>
        public static void CompressAndSaveResults(ListView list, string name, string saveLocation, string processingLocation)
        {
            if (!Directory.Exists(saveLocation)) Directory.CreateDirectory(saveLocation);
            using (ZipArchive zip = ZipFile.Open(saveLocation + name + ".zip", ZipArchiveMode.Create))
            {
                zip.CreateEntryFromFile(processingLocation + name + ".mp4", "video.mp4");
                zip.CreateEntryFromFile(processingLocation + name + ".csv", "output.csv");
            }
            list.Items.Add(new VideoData { Name = name, URL = saveLocation + name + ".zip" });
        }

        /// <summary>
        /// Copies a video file from a to b, if it is a mov file converts it to mp4
        /// </summary>
        /// <param name="originPath">where the file is</param>
        /// <param name="newPathName">where the file goes</param>
        /// <param name="filetype">the type of the file</param>
        public static void CopyVideo(string originPath, string newPathName, string filetype)
        {
            if (filetype == "mp4") File.Copy(@originPath, @newPathName);
            else
            {
                string movProcess = "ffmpeg -i \"" + originPath
                    + "\" -vcodec h264 -acodec aac -strict -2 \"" + newPathName + "\"";

                Shell movcmd = new Shell();
                movcmd.Start();
                movcmd.WriteLastItem(movProcess);
            }
        }

    }
}
