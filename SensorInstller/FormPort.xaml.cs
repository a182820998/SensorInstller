using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SensorInstaller
{
    /// <summary>
    /// Window1.xaml 的互動邏輯
    /// </summary>
    public partial class FormPort : Window
    {
        public FormPort(List<string> portList, string title)
        {
            InitializeComponent();
            comboPort.ItemsSource = portList;
            this.Title = title;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            button1ClickEvent();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public virtual void button1ClickEvent()
        {

        }
    }
}
