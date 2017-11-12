using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Management;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SensorInstaller
{
    public partial class MainWindow : Window
    {
        private ComboBox[] _comboSensorList;

        public MainWindow()
        {
            InitializeComponent();
            comboSensorQuantity.ItemsSource = quantityList();
            _comboSensorList = new ComboBox[6]
            {
                comboSensor1, comboSensor2, comboSensor3, comboSensor4, comboSensor5, comboSensor6
            };

            foreach (ComboBox item in _comboSensorList)
                item.ItemsSource = typeList();
        }

        private List<string> quantityList()
        {
            List<string> list = new List<string>();

            for (int i = 1; i < 6; i++)
            {
                list.Add((i + 1).ToString());
            }

            return list;
        }

        private List<string> typeList()
        {
            List<string> list = new List<string>();
            list.Add("濕度");
            list.Add("光度");
            list.Add("煙霧");
            list.Add("溫度");
            return list;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            List<string> typeSetText = new List<string>();
            List<int> typeSetIndex = new List<int>();

            foreach (ComboBox item in _comboSensorList)
            {
                if (item.IsEnabled)
                {
                    typeSetText.Add(item.Text.ToString());
                    typeSetIndex.Add(item.SelectedIndex);
                }
            }

            MessageBoxResult result = MessageBox.Show("種類清單 : " + String.Join(" ,", typeSetText.ToArray()) + "\r\n" + "按下確認產生source code",
                                      "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                editSource(typeSetIndex);
            }
            else
            {
                MessageBox.Show("取消");
            }
        }

        private void editSource(List<int> typeSetIndexList)
        {
            try
            {
                StreamReader reader = new StreamReader(@"C:\Users\user\Documents\Visual Studio 2015\Projects\csharp\SensorInstller\SensorInstller\ArduinoSource\Uploader\Uploader.ino", Encoding.UTF8);
                List<string> newSource = new List<string>();
                string[] floatParameters = new string[typeSetIndexList.Count];
                string[] sentParameters = new string[typeSetIndexList.Count];
                string[] sentArgs = new string[typeSetIndexList.Count];
                string[] phpParameters = new string[typeSetIndexList.Count];

                for (int i = 0; i < typeSetIndexList.Count; i++)
                {
                    if (typeSetIndexList[i] == 0)
                    {
                        floatParameters[i] = "    float sensor" + (i + 1).ToString() + "Value = moisture(sensor" + (i + 1).ToString() + "Pin);";
                    }
                    else if (typeSetIndexList[i] == 1)
                    {
                        floatParameters[i] = "    float sensor" + (i + 1).ToString() + "Value = light(sensor" + (i + 1).ToString() + "Pin);";
                    }
                    else if (typeSetIndexList[i] == 2)
                    {
                        floatParameters[i] = "    float sensor" + (i + 1).ToString() + "Value = smoke(sensor" + (i + 1).ToString() + "Pin);";
                    }
                    else if (typeSetIndexList[i] == 3)
                    {
                        floatParameters[i] = "    float sensor" + (i + 1).ToString() + "Value = temp(sensor" + (i + 1).ToString() + "Pin);";
                    }

                    sentParameters[i] = "String(sensor" + (i + 1).ToString() + "Value, 2)";
                    sentArgs[i] = "String value" + (i + 1).ToString();
                    phpParameters[i] = "    cmd += \"&s_" + (i + 1).ToString() + "=\";cmd += value" + (i + 1).ToString() + ";";
                }

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();

                    if (line == "    //query(getValue)")
                    {
                        line = String.Join("\r\n", floatParameters);
                    }
                    else if (line == "    //query(sentOnCloud)")
                    {
                        line = "    SentOnCloud(" + String.Join(", ", sentParameters) + ");";
                    }
                    else if (line == "//query(void sentOnCloud)")
                    {
                        line = "void SentOnCloud(" + String.Join(", ", sentArgs) + ")";
                    }
                    else if (line == "    //query(cmd+=)")
                    {
                        line = String.Join("\r\n", phpParameters);
                    }

                    newSource.Add(line);
                }

                string source = String.Join("\r\n", newSource.ToArray());
                reader.Close();
                StreamWriter writer = new StreamWriter(@"C:\SensorInstallerSource\newUploader\newUploader.ino");
                writer.Write(source);
                writer.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo startBuildInit = new ProcessStartInfo();
            startBuildInit.Arguments = @"-dump-prefs -logger=machine -hardware C:\Arduino\hardware"
                                       + @" -tools C:\Arduino\tools-builder -tools C:\Arduino\hardware\tools\avr"
                                       + @" -built-in-libraries C:\Arduino\libraries -libraries C:\Users\user\Documents\Arduino\libraries"
                                       + @" -fqbn=arduino:avr:uno -ide-version=10805 -build-path C:\SensorInstallerSource\newUploader\BuildPath"
                                       + @" -warnings=none -prefs=build.warn_data_percentage=75"
                                       + @" -prefs=runtime.tools.arduinoOTA.path=C:\Arduino\hardware\tools\avr"
                                       + @" -prefs=runtime.tools.avr-gcc.path=C:\Arduino\hardware\tools\avr"
                                       + @" -prefs=runtime.tools.avrdude.path=C:\Arduino\hardware\tools\avr"
                                       + @" -verbose C:\SensorInstallerSource\newUploader\newUploader.ino";
            startBuildInit.FileName = @"C:\Arduino\arduino-builder.exe";
            startBuildInit.WindowStyle = ProcessWindowStyle.Normal;
            startBuildInit.CreateNoWindow = false;
            int exitCodeInit;

            using (Process proc = Process.Start(startBuildInit))
            {
                proc.WaitForExit();
                exitCodeInit = proc.ExitCode;
            }

            if (exitCodeInit == 0)
            {
                MessageBoxResult result = MessageBox.Show("builder初始完成，按下確認開始產生二進檔", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    ProcessStartInfo startBuildHex = new ProcessStartInfo();
                    startBuildHex.FileName = @"C:\Arduino\arduino-builder.exe";
                    startBuildInit.Arguments = @"-compile -logger=machine -hardware C:\Arduino\hardware"
                                               + @" -tools C:\Arduino\tools-builder -tools C:\Arduino\hardware\tools\avr"
                                               + @" -built-in-libraries C:\Arduino\libraries -libraries C:\Users\user\Documents\Arduino\libraries"
                                               + @" -fqbn=arduino:avr:uno -ide-version=10805 -build-path C:\SensorInstallerSource\newUploader\BuildPath"
                                               + @" -warnings=none -prefs=build.warn_data_percentage=75"
                                               + @" -prefs=runtime.tools.arduinoOTA.path=C:\Arduino\hardware\tools\avr"
                                               + @" -prefs=runtime.tools.avr-gcc.path=C:\Arduino\hardware\tools\avr"
                                               + @" -prefs=runtime.tools.avrdude.path=C:\Arduino\hardware\tools\avr"
                                               + @" -verbose C:\SensorInstallerSource\newUploader\newUploader.ino";
                    startBuildInit.WindowStyle = ProcessWindowStyle.Normal;
                    startBuildInit.CreateNoWindow = false;
                    int exitCodeHex;

                    using (Process proc = Process.Start(startBuildInit))
                    {
                        proc.WaitForExit();
                        exitCodeHex = proc.ExitCode;
                    }

                    if (exitCodeHex == 0)
                    {
                        MessageBox.Show("原始碼編譯成功，已產生二進檔");
                    }
                }
                else
                {
                    MessageBox.Show("取消");
                }
            }
            else
            {
                MessageBox.Show("初始失敗");
            }
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            List<string> portList = detectArduinoPort();
            WriterDialog dialog = new WriterDialog(portList, "燒錄器");
            dialog.ShowDialog();
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            List<string> portList = detectArduinoPort();
            SerialMonitorDialog dialog = new SerialMonitorDialog(portList, "序列部監控");
            dialog.ShowDialog();
        }

        private List<string> detectArduinoPort()
        {
            ManagementScope connectionScope = new ManagementScope();
            SelectQuery serialQuery = new SelectQuery("SELECT * FROM Win32_SerialPort");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(connectionScope, serialQuery);
            List<string> portList = new List<string>();

            try
            {
                foreach (ManagementObject item in searcher.Get())
                {
                    string description = item["Description"].ToString();
                    string deviceId = item["DeviceID"].ToString();

                    if (description.Contains("Arduino"))
                    {
                        portList.Add(deviceId);
                    }
                }

                return portList;
            }
            catch (Exception e)
            {
                portList.Add(e.ToString());
            }

            return portList;
        }

        private void comboSensorQuantity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmd = sender as ComboBox;

            foreach (ComboBox item in _comboSensorList)
            {
                item.IsEnabled = false;
            }

            for (int i = 0; i < cmd.SelectedIndex + 2; i++)
            {
                _comboSensorList[i].IsEnabled = true;
            }
        }
    }
}