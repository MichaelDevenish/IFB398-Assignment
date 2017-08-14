using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapstoneLayoutTest
{
    class VideoData
    {
        private string name;
        private string status;
        private string url;
        private bool complete;

        public string Name { get { return name; } set { name = value; } }
        public string Status
        {
            get { return status; }
            set
            {
                if (value == "Complete")
                {
                    complete = true;
                }
                status = value;
            }
        }
        public string URL { get { return url; } set { url = value; } }
        public bool Complete { get { return complete; } }
        public VideoData()
        {
        }
    }
}
