using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapstoneLayoutTest.Helper_Functions
{
    /// <summary>
    /// An extension of process that creates a command line
    /// </summary>
    class Shell : Process
    {
        public Shell()
        {
            this.StartInfo.FileName = "cmd.exe";
            this.StartInfo.RedirectStandardInput = true;
            this.StartInfo.RedirectStandardOutput = true;
            this.StartInfo.CreateNoWindow = true;
            this.StartInfo.UseShellExecute = false;
        }
        /// <summary>
        /// Writes a command and closes the input
        /// </summary>
        /// <param name="strCmdText">the command to write</param>
        public void WriteLastItem(string strCmdText)
        {
            WriteItem(strCmdText);
            this.StandardInput.Close();
            this.WaitForExit();
        }

        /// <summary>
        /// Writes a command
        /// </summary>
        /// <param name="strCmdText">the command to write</param>
        public void WriteItem(string strCmdText)
        {
            this.StandardInput.WriteLine(strCmdText);
            this.StandardInput.Flush();
        }
    }
}
