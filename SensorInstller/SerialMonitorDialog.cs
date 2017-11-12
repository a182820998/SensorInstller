using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SensorInstaller
{
    class SerialMonitorDialog : FormPort
    {
        public SerialMonitorDialog(List<string> portList, string title) : base(portList, title)
        {
        }

        public override void button1ClickEvent()
        {
            string port = comboPort.Text.ToString();
            ProcessStartInfo start = new ProcessStartInfo();
            start.Arguments = @"-serial " + port + " -sercfg 9600,8,n,1,X";
            start.FileName = @"C:\Program Files\PuTTY\plink.exe";
            start.WindowStyle = ProcessWindowStyle.Normal;
            start.CreateNoWindow = false;
            int exitCode;

            using (Process proc = Process.Start(start))
            {
                this.Close();
                proc.WaitForExit();
                exitCode = proc.ExitCode;
            }
        }
    }
}
