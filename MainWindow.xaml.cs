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
        private StringFactory stringFactory;

        public MainWindow()
        {
            InitializeComponent();
            computerHardware = new Computer();
            stringFactory = new StringFactory();
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
            GPULabel.Content = computerHardware.Hardware[3].Name;


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
                        //GPULabel.Content = graphicsCard;
                    }
                }
            }

            


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
                                //fansLabel.Content += Convert.ToString((int)(float)sensor.Value) + " RPM\n";
                            }
                        }
                    }
                }

                if (hardware.HardwareType == HardwareType.CPU)
                {
                    foreach (var hardwareSensor in hardware.Sensors)
                    {
                        switch (hardwareSensor.SensorType)
                        {
                            case SensorType.Load:
                                if (hardwareSensor.Name == "CPU Total")
                                {
                                    CPULoad.Content = stringFactory.loadString (hardwareSensor.Value);
                                }
                                break;

                            case SensorType.Temperature:
                                if (hardwareSensor.Name == "CPU Package")
                                {
                                    CPUTemp.Content = stringFactory.temperatureString(hardwareSensor.Value);
                                    if (hardwareSensor.Value < 45)
                                    {
                                        CPUStatusImage.Source = new BitmapImage(new Uri(@"Images\CPU Status\CPUHot.png", UriKind.Relative));
                                    } else if (hardwareSensor.Value >= 45 && hardwareSensor.Value < 75)
                                    {
                                        CPUStatusImage.Source = new BitmapImage(new Uri(@"Resources\Images\CPU Status\CPUHot.png", UriKind.Relative));
                                    } else if (hardwareSensor.Value > 75)
                                    {
                                        CPUStatusImage.Source = new BitmapImage(new Uri(@"Images\CPU Status\CPUOverheated.png", UriKind.Relative));
                                    }
                                }
                                break;

                            case SensorType.Clock:
                                if (hardwareSensor.Name == "CPU Core #2")
                                {
                                    CPUClock.Content = stringFactory.clockString(hardwareSensor.Value);
                                }
                                break;

                            default:
                                break;
                        }
                    }
                } else if (hardware.HardwareType == HardwareType.RAM)
                {
                    foreach (var hardwareSensor in hardware.Sensors)
                    {
                        Console.WriteLine(hardwareSensor);
                        /*switch (hardwareSensor.SensorType)
                        {
                            case SensorType.Load
                        }*/
                    }                
                } else if (hardware.HardwareType == HardwareType.GpuNvidia || hardware.HardwareType == HardwareType.GpuAti)
                {
                    foreach (var hardwareSensor in hardware.Sensors)
                    {
                        switch (hardwareSensor.SensorType)
                        {
                            case SensorType.Load:
                                if (hardwareSensor.Name == "GPU Core")
                                {
                                    GPULoad.Content = stringFactory.loadString(hardwareSensor.Value);
                                }                                
                                break;

                            case SensorType.Clock:
                                if (hardwareSensor.Name == "GPU Core")
                                {
                                    GPUCoreClock.Content = "Core " + stringFactory.clockString(hardwareSensor.Value);
                                } else if (hardwareSensor.Name == "GPU Memory")
                                {
                                    GPUMemoryClock.Content = "Memory " + stringFactory.clockString(hardwareSensor.Value);
                                }
                                break;
                            case SensorType.Temperature:
                                GPUTemp.Content = stringFactory.temperatureString(hardwareSensor.Value);
                                break;
                            case SensorType.Fan:
                                //TO-DO: CHANGE GPU FAN IN FANS SECTION
                                break;
                            case SensorType.SmallData:
                                if (hardwareSensor.Name == "GPU Memory Total")
                                {
                                    GPUMemoryCapacity.Content = "Memory Capacity: " + hardwareSensor.Value;
                                } else if (hardwareSensor.Name == "GPU Memory Used")
                                {
                                    GPUMemoryUsed.Content = "Memory Used: " + hardwareSensor.Value;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            /*for (int i = 0; i < computerHardware.Hardware.Length; i++)
            {
                Console.WriteLine("----------------------\n" + computerHardware.Hardware[i].Name + "\n----------------------");
                for (int j = 0; j < computerHardware.Hardware[i].Sensors.Length; j++)
                {
                    Console.WriteLine(computerHardware.Hardware[i].Sensors[j].SensorType + " (" + computerHardware.Hardware[i].Sensors[j].Name + ")" + ": " + computerHardware.Hardware[i].Sensors[j].Value + computerHardware.Hardware[i].Sensors[j].Hardware);
                }
            }*/
        }

        private void getSensorData ()
        {
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
                                //fansLabel.Content += Convert.ToString((int)(float)sensor.Value) + " RPM\n";
                            }
                        }
                    }
                }

                if (hardware.HardwareType == HardwareType.CPU)
                {
                    foreach (var hardwareSensor in hardware.Sensors)
                    {
                        switch (hardwareSensor.SensorType)
                        {
                            case SensorType.Load:
                                if (hardwareSensor.Name == "CPU Total")
                                {
                                    CPULoad.Content = stringFactory.loadString(hardwareSensor.Value);
                                }
                                break;

                            case SensorType.Temperature:
                                if (hardwareSensor.Name == "CPU Package")
                                {
                                    CPUTemp.Content = stringFactory.temperatureString(hardwareSensor.Value);
                                    if (hardwareSensor.Value < 45)
                                    {
                                        CPUStatusImage.Source = new BitmapImage(new Uri(@"Images\CPU Status\CPUHot.png", UriKind.Relative));
                                    }
                                    else if (hardwareSensor.Value >= 45 && hardwareSensor.Value < 75)
                                    {
                                        CPUStatusImage.Source = new BitmapImage(new Uri(@"Resources\Images\CPU Status\CPUHot.png", UriKind.Relative));
                                    }
                                    else if (hardwareSensor.Value > 75)
                                    {
                                        CPUStatusImage.Source = new BitmapImage(new Uri(@"Images\CPU Status\CPUOverheated.png", UriKind.Relative));
                                    }
                                }
                                break;

                            case SensorType.Clock:
                                if (hardwareSensor.Name == "CPU Core #2")
                                {
                                    CPUClock.Content = stringFactory.clockString(hardwareSensor.Value);
                                }
                                break;

                            default:
                                break;
                        }
                    }
                }
                else if (hardware.HardwareType == HardwareType.RAM)
                {
                    foreach (var hardwareSensor in hardware.Sensors)
                    {
                        Console.WriteLine(hardwareSensor);
                        /*switch (hardwareSensor.SensorType)
                        {
                            case SensorType.Load
                        }*/
                    }
                }
                else if (hardware.HardwareType == HardwareType.GpuNvidia || hardware.HardwareType == HardwareType.GpuAti)
                {
                    foreach (var hardwareSensor in hardware.Sensors)
                    {
                        switch (hardwareSensor.SensorType)
                        {
                            case SensorType.Load:
                                if (hardwareSensor.Name == "GPU Core")
                                {
                                    GPULoad.Content = stringFactory.loadString(hardwareSensor.Value);
                                }
                                break;

                            case SensorType.Clock:
                                if (hardwareSensor.Name == "GPU Core")
                                {
                                    GPUCoreClock.Content = "Core " + stringFactory.clockString(hardwareSensor.Value);
                                }
                                else if (hardwareSensor.Name == "GPU Memory")
                                {
                                    GPUMemoryClock.Content = "Memory " + stringFactory.clockString(hardwareSensor.Value);
                                }
                                break;
                            case SensorType.Temperature:
                                GPUTemp.Content = stringFactory.temperatureString(hardwareSensor.Value);
                                break;
                            case SensorType.Fan:
                                //TO-DO: CHANGE GPU FAN IN FANS SECTION
                                break;
                            case SensorType.SmallData:
                                if (hardwareSensor.Name == "GPU Memory Total")
                                {
                                    GPUMemoryCapacity.Content = "Memory Capacity: " + hardwareSensor.Value;
                                }
                                else if (hardwareSensor.Name == "GPU Memory Used")
                                {
                                    GPUMemoryUsed.Content = "Memory Used: " + hardwareSensor.Value;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
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
            prepareUI();
        }
    }
}
