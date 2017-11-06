using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace CapstoneLayoutTest.Helper_Functions
{
    /// <summary>
    /// small helper functions for the Control Bar
    /// </summary>
    class ControlBarHelper
    {
        /// <summary>
        /// Sets the pausePlayImage to either pause(true) or play(false)
        /// </summary>
        /// <param name="pausePlay">the state of the button</param>
        public static void SetPausePlayImage(bool pausePlay, Image pausePlayImage)
        {
            Uri uriSource = null;
            if (pausePlay) uriSource = new Uri(@"/CapstoneLayoutTest;component/Images/ic_pause_white_24dp.png", UriKind.Relative);
            else uriSource = new Uri(@"/CapstoneLayoutTest;component/Images/ic_play_arrow_white_24dp.png", UriKind.Relative);
            pausePlayImage.Source = new BitmapImage(uriSource);
        }

        /// <summary>
        /// Converts an int to a string that represents [HH:]MM:SS
        /// </summary>
        /// <param name="seconds">the seconds to convert</param>
        /// <returns>a string representing [HH:]MM:SS</returns>
        public static string IntToTimeString(int seconds)
        {
            string builder = "";
            if (seconds >= 3600) builder += seconds / 3600 + ":";
            if (seconds % 3600 > 60 || seconds % 60 == 0)
            {
                int min = seconds % 3600 / 60;
                if (min < 10) builder += "0";
                builder += min + ":";
            }
            else builder += "00:";

            int sec = seconds % 60;
            if (sec < 10) builder += "0";
            builder += sec;
            return builder;
        }
    }
}
