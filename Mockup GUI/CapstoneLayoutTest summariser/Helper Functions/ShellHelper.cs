using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapstoneLayoutTest.Helper_Functions
{
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
        public void WriteLastItem(string strCmdText)
        {
            this.StandardInput.WriteLine(strCmdText);
            this.StandardInput.Flush();
            this.StandardInput.Close();
            this.WaitForExit();
        }


    }
}
