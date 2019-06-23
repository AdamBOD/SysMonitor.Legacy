using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SysMonitor
{
    public partial class ConfigurationWindow : Window
    {
        private const string Path = @".\conf.json";
        Border selectedElement;
        string selectedGPU;

        public ConfigurationWindow()
        {
            InitializeComponent();
            configWindow.Width = (int)(Screen.PrimaryScreen.Bounds.Width * 0.30);
            configWindow.Height = (int)(Screen.PrimaryScreen.Bounds.Height * 0.6);
        }

        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedElement != null)
            {
                selectedElement.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#60666E");
            }
            selectedGPU = ((System.Windows.Controls.Button)sender).Name;
            selectedElement = (Border)FindName(selectedGPU + "_Border");
            selectedElement.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#09a313");
            SaveButton.Visibility = Visibility.Visible;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            selectedGPU = selectedGPU.Replace("_", "-");

            string selectedGPUPrefix = selectedGPU.Substring(0, 3);
            if (selectedGPUPrefix == "One")
            {
                selectedGPU = selectedGPU.Replace("One", "1");
            }
            else if (selectedGPUPrefix == "Two")
            {
                selectedGPU = selectedGPU.Replace("Two", "2");
            }
            else if (selectedGPUPrefix == "Thr")
            {
                selectedGPU = selectedGPU.Replace("Three", "3");
            }

            JObject sysMonitorConfig = new JObject(
                new JProperty("GPUString", selectedGPU));

            File.WriteAllText(Path, contents: sysMonitorConfig.ToString());

            using (StreamWriter file = File.CreateText(Path))
            using (JsonTextWriter writer = new JsonTextWriter(file))
            {
                sysMonitorConfig.WriteTo(writer);
            }
            DialogResult = true;
        }
    }
}
