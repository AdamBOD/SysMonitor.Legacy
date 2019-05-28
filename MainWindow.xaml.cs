using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Management;
using OpenHardwareMonitor;
//using OpenHardwareMonitor.Collections;
using OpenHardwareMonitor.Hardware;

namespace SysMonitor
{
    public partial class MainWindow : Window
    {
        private Computer computerHardware;
        public CPUIDSDK pSDK;

        public MainWindow()
        {
            InitializeComponent();
            computerHardware = new Computer();
            prepareUI();
        }

        private void prepareUI ()
        {
            titleBar.MouseDown += Window_MouseDown;

            closeWindowButton.Click += new RoutedEventHandler(closeApp);
            minWindowButton.Click += new RoutedEventHandler(minimizeApp);
            maxWindowButton.Click += new RoutedEventHandler(maximizeApp);

            computerHardware.MainboardEnabled = true;
            computerHardware.FanControllerEnabled = true;
            computerHardware.CPUEnabled = true;
            computerHardware.GPUEnabled = true;
            computerHardware.RAMEnabled = true;
            computerHardware.HDDEnabled = true;
            computerHardware.Open();

            CPULabel.Content = computerHardware.Hardware[1].Name;
            CPULabel.Content = computerHardware.Hardware[0].Sensors.Length;
            //CPULabel.Content = computerHardware.GetReport();
            //computerHardware.Hardware[0].Sensors += MainWindow_SensorAdded;
            for (int i = 0; i < computerHardware.Hardware.Length; i++)
            {
                Console.WriteLine("----------------------\n" + computerHardware.Hardware[i].Name + "\n----------------------");
                for (int j = 0; j < computerHardware.Hardware[i].Sensors.Length; j++)
                {
                    Console.WriteLine(computerHardware.Hardware[i].Sensors[j].SensorType + " (" + computerHardware.Hardware[i].Sensors[j].Name + ")" + ": " + computerHardware.Hardware[i].Sensors[j].Value + computerHardware.Hardware[i].Sensors[j].Hardware);
                }
            }
            
            ManagementObjectSearcher searcher
                = new ManagementObjectSearcher("SELECT * FROM Win32_DisplayConfiguration");

            string graphicsCard = string.Empty;
            foreach (ManagementObject mo in searcher.Get())
            {
                foreach (PropertyData property in mo.Properties)
                {
                    if (property.Name == "Description")
                    {
                        graphicsCard = property.Value.ToString();
                    }
                }
            }
            //GPULabel.Content = graphicsCard;


            foreach (var hardware in computerHardware.Hardware)
            {
                // This will be in the mainboard
                foreach (var subhardware in hardware.SubHardware)
                {
                    // This will be in the SuperIO
                    subhardware.Update();
                    if (subhardware.Sensors.Length > 0) // Index out of bounds check
                    {
                        foreach (var sensor in subhardware.Sensors)
                        {
                            // Look for the main fan sensor
                            if (sensor.SensorType == SensorType.Fan)
                            {
                                Console.WriteLine("CPU Fan Speed:" + Convert.ToString((int)(float)sensor.Value) + " RPM");
                                GPULabel.Content += Convert.ToString((int)(float)sensor.Value) + " RPM\n";
                            }
                        }
                    }
                }
            }

            

            pSDK = new CPUIDSDK();
            pSDK.CreateInstance();

            var info = get_sensor_list(6, CPUIDSDK.SENSOR_CLASS_FAN);

            CPULabel.Content = info;

            SelectQuery query =
           new SelectQuery("Win32_Fan");

            // Instantiate an object searcher
            // with this query
            ManagementObjectSearcher searcher01 =
                new ManagementObjectSearcher("SELECT * FROM Win32_Fan");

            // Call Get() to retrieve the collection
            // of objects and loop through it
            //foreach (ManagementObject envVar in searcher01.Get())
            //fansLabel.Content += envVar["DesiredSpeed"].ToString();
            searcher01.Get();
            foreach (ManagementObject mo in searcher01.Get())
            {
                /*foreach (PropertyData property in mo.SystemProperties)
                {
                    fansLabel.Content += property.Value.ToString() + " ";
                }*/
                fansLabel.Content += mo["Name"].ToString();
                fansLabel.Content += "\n";
            }
        }

        public List<Sensor> get_sensor_list(int device_index, int sensor_class)
        {
            int sensor_index;
            int NbSensors;
            bool result;
            int sensor_id = 0;
            string sensorname = "";
            int iValue = 0;
            float fValue = 0;
            float fMinValue = 0;
            float fMaxValue = 0;
            var sensors = new List<Sensor>();
            NbSensors = pSDK.GetNumberOfSensors(device_index, sensor_class);
            for (sensor_index = 0; sensor_index < NbSensors; sensor_index += 1)
            {
                result = pSDK.GetSensorInfos(device_index,
                  sensor_index,
                  sensor_class,
                  ref sensor_id,
                  ref sensorname,
                  ref iValue,
                  ref fValue,
                  ref fMinValue,
                  ref fMaxValue);
                if (result == true)
                {
                    var data = new Sensor();
                    data.name = sensorname;
                    data.value = Math.Round(fValue, 2);
                    sensors.Add(data);
                }
            }
            return sensors;
        }

        private void ComputerHardware_HardwareAdded(IHardware hardware)
        {
            CPULabel.Content += hardware.Name;
        }

        private void MainWindow_SensorAdded(ISensor sensor)
        {
            CPULabel.Content = sensor.SensorType;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void closeApp (object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        private void minimizeApp(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void maximizeApp(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            } else
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(computerHardware.GetReport());
        }
    }
}
