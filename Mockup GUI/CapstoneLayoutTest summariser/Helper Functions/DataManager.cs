using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CapstoneLayoutTest.Helper_Functions
{
    class DataManager
    {
        public static void SaveFile(string filePath, ListView saveFrom)
        {
            VideoData[] items = new VideoData[saveFrom.Items.Count];
            saveFrom.Items.CopyTo(items, 0);

            // Serialize the items and save it to a file.
            using (FileStream fs = File.Create(filePath))
            {
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                bf.Serialize(fs, items);
            }

        }

        public static void LoadFile(string filePath, ListView loadTo)
        {
            if (File.Exists(filePath))
            {
                VideoData[] items = null;

                using (FileStream fs = File.OpenRead(filePath))
                {
                    System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    items = (VideoData[])bf.Deserialize(fs);
                }

                loadTo.Items.Clear();
                foreach (VideoData item in items)
                {
                    if (File.Exists(item.URL))
                        loadTo.Items.Add(item);
                }
            }
        }
    }
}
