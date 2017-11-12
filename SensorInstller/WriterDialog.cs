using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SensorInstaller
{
    class WriterDialog : FormPort
    {
        public WriterDialog(List<string> portList, string title) : base(portList, title)
        {
        }

        public override void button1ClickEvent()
        {
            string port = comboPort.Text.ToString();
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = @"C:\Arduino\hardware\tools\avr\bin\avrdude.exe";
            start.Arguments = @"-CC:\Arduino\hardware\tools\avr/etc/avrdude.conf -v -patmega328p -carduino -P"
                              + port
                              + @" -b115200 -D -Uflash:w:C:\SensorInstallerSource\newUploader\BuildPath\newUploader.ino.hex:i";
            start.WindowStyle = ProcessWindowStyle.Normal;
            start.CreateNoWindow = false;
            int exitCode;

            using (Process proc = Process.Start(start))
            {
                proc.WaitForExit();
                exitCode = proc.ExitCode;
            }

            if (exitCode == 0)
            {
                MessageBox.Show("燒錄成功");
                this.Close();
            }
            else
            {
                MessageBox.Show("燒錄失敗");
            }
        }
    }
}
