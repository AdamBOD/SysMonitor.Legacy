using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenHardwareMonitor.Hardware;

namespace SysMonitor
{
    public partial class MainWindow : Window
    {
        private Computer computerHardware;
        private StringFactory stringFactory;

        private bool gpuFansRegistered = false;
        private bool fansRegistered = false;
        private bool fansRendered = false;
        private List<Fan> fans;

        private int previousWidth;
        private int currentWidth;
        private int previousHeight;
        private int currentHeight;

        public static string GPUString;

        private int CPUHeatClass = -1;
        private int CPUHeatClassChange = 0;

        private int GPUHeatClass = -1;
        private int GPUHeatClassChange = 0;

        private bool titleBarClicked = false;
        private bool windowStateChanging = false;

        public static readonly DependencyProperty ScaleValueProperty = DependencyProperty.Register("ScaleValue", typeof(double), typeof(MainWindow), new UIPropertyMetadata(1.0));

        public MainWindow()
        {
            InitializeComponent();
            currentWidth = (int)(Screen.PrimaryScreen.Bounds.Width * 0.6953125);
            previousWidth = currentWidth;

            currentHeight = (int)(Screen.PrimaryScreen.Bounds.Height * 0.6953125);
            previousHeight = currentHeight;

            double scaleValue = 0.935;
            if (Screen.PrimaryScreen.Bounds.Width > 1920)
                scaleValue = (double)Screen.PrimaryScreen.Bounds.Width / 1920 - 0.085;

            ScaleValue = scaleValue;

            mainWindow.Width = currentWidth;
            mainWindow.Height = currentHeight;
            computerHardware = new Computer();
            stringFactory = new StringFactory();
            fans = new List<Fan>();

            // Check to see if the system is a desktop as there is no need for a config file on laptops as fans aren't query-able
            if (SystemInformation.PowerStatus.BatteryChargeStatus == BatteryChargeStatus.NoSystemBattery)
            {
                if (!File.Exists(@".\conf.json"))
                {
                    ConfigurationWindow configWindow = new ConfigurationWindow();
                    configWindow.ShowDialog();
                    if (configWindow.DialogResult.HasValue && configWindow.DialogResult.Value)
                    {
                        readConfig();
                    }
                    else
                    {
                        App.Current.Shutdown();
                    }
                }
                else
                {
                    readConfig();
                }
            }
            else
            {
                FansBlock.Visibility = Visibility.Hidden;
            }
            
            prepareUI();
        }

        private void readConfig ()
        {
            JObject jsonObject = JObject.Parse(File.ReadAllText(@".\conf.json"));

            using (StreamReader file = File.OpenText(@".\conf.json"))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                JObject confObject = (JObject)JToken.ReadFrom(reader);
                GPUString = confObject["GPUString"].ToString();
            }
        }

        private void prepareUI ()
        {
            mainWindow.StateChanged += MainWindow_StateChanged;

            titleBar.MouseLeftButtonDown += Window_MouseLeftButtonDown;
            titleBar.MouseLeftButtonUp += Window_MouseLeftButtonUp;
            titleBar.MouseMove += TitleBar_MouseMove;

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

            memoryCapacity.Content = "Capacity: " + stringFactory.bytesToGB(new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory) + "GB";

            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += OnTimedEvent;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }

        // Timer elapsed function
        private void OnTimedEvent(object source, EventArgs e)
        {
            foreach (var hardware in computerHardware.Hardware)
            {
                hardware.Update();
            }
            getSensorData();
        }

        // Function that is called periodically to query the OpenHardware library and process the responses for each hardware category: CPU, Memory, GPU, Fans
        private void getSensorData ()
        {
            int fansIndex = 0;
            foreach (var hardware in computerHardware.Hardware)
            {
                foreach (var subhardware in hardware.SubHardware)
                {
                    subhardware.Update();
                    if (subhardware.Sensors.Length > 0)
                    {
                        foreach (var sensor in subhardware.Sensors)
                        {
                            if (sensor.SensorType == SensorType.Fan)
                            {                                
                                if (!fansRegistered)
                                {
                                    if (fansIndex <= subhardware.Sensors.Length - 1)
                                    {
                                        if (fansIndex == 0)
                                        {
                                            fans.Add(new Fan("CPU", (int)sensor.Value));
                                        }
                                        else
                                        {
                                            fans.Add(new Fan("Case", (int)sensor.Value));
                                        }
                                    } else
                                    {
                                        fansRegistered = true;
                                    }
                                } else
                                {
                                    fans[fansIndex].setRPM((int)sensor.Value);
                                }
                                fansIndex++;
                            }
                        }

                        if (!fansRegistered)
                        {
                            fansRegistered = true;
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
                                    if (CPUHeatClass == -1)
                                    {
                                        CPUHeatClass = Helpers.getProcessorHeatClass(hardwareSensor.Value);
                                        CPUStatusImage.Source = new BitmapImage(new Uri(@"" + Helpers.getProcessorTemperatureIconPath(CPUHeatClass), UriKind.Relative));
                                    }

                                    CPUTemp.Content = stringFactory.temperatureString(hardwareSensor.Value);

                                    if (CPUHeatClass != Helpers.getProcessorHeatClass(hardwareSensor.Value))
                                    {
                                        if (CPUHeatClassChange < 5)
                                        {
                                            CPUHeatClassChange++;
                                        }
                                        else
                                        {
                                            CPUHeatClass = Helpers.getProcessorHeatClass(hardwareSensor.Value);
                                            CPUStatusImage.Source = new BitmapImage(new Uri(@"" + Helpers.getProcessorTemperatureIconPath(CPUHeatClass), UriKind.Relative));
                                            CPUHeatClassChange = 0;
                                        }
                                    }
                                    else
                                    {
                                        if (CPUHeatClassChange > 0)
                                        {
                                            CPUHeatClassChange--;
                                        }
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
                        switch (hardwareSensor.SensorType)
                        {
                            case SensorType.Data:
                                if (hardwareSensor.Name == "Used Memory")
                                {
                                    memoryLoad.Content = "Used: " + stringFactory.GBString(hardwareSensor.Value);
                                }
                                break;

                            default:
                                break;
                        }
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
                                if (GPUHeatClass == -1)
                                {
                                    GPUHeatClass = Helpers.getProcessorHeatClass(hardwareSensor.Value);
                                    GPUStatusImage.Source = new BitmapImage(new Uri(@"" + Helpers.getProcessorTemperatureIconPath(GPUHeatClass), UriKind.Relative));
                                }

                                GPUTemp.Content = stringFactory.temperatureString(hardwareSensor.Value);

                                if (GPUHeatClass != Helpers.getProcessorHeatClass(hardwareSensor.Value))
                                {
                                    if (GPUHeatClassChange < 5)
                                    {
                                        GPUHeatClassChange++;
                                    }
                                    else
                                    {
                                        GPUHeatClass = Helpers.getProcessorHeatClass(hardwareSensor.Value);
                                        GPUStatusImage.Source = new BitmapImage(new Uri(@"" + Helpers.getProcessorTemperatureIconPath(GPUHeatClass), UriKind.Relative));
                                        GPUHeatClassChange = 0;
                                    }
                                }
                                else
                                {
                                    if (GPUHeatClassChange > 0)
                                    {
                                        GPUHeatClassChange--;
                                    }
                                }
                                break;
                            case SensorType.Fan:
                                if (!gpuFansRegistered)
                                {
                                    fans.Add(new Fan("GPU", (int)hardwareSensor.Value));
                                    gpuFansRegistered = true;
                                } else
                                {
                                    fans[fans.Count - 1].setRPM((int)hardwareSensor.Value);
                                }
                                break;
                            case SensorType.SmallData:
                                if (hardwareSensor.Name == "GPU Memory Total")
                                {
                                    GPUMemoryCapacity.Content = "Memory Capacity: " + stringFactory.memoryString(hardwareSensor.Value);
                                }
                                else if (hardwareSensor.Name == "GPU Memory Used")
                                {
                                    GPUMemoryUsed.Content = "Memory Used: " + stringFactory.memoryString(hardwareSensor.Value);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            if (!fansRendered)
            {
                renderFans();
            } else
            {
                updateFans();
            }
        }

        // Function to create a fan UI component for each fan fetched from the OpenHardware library
        private void renderFans()
        {
            int fanIndex = 0;
            foreach (var fan in fans)
            {
                fanIndex++;
                FansContainer.Children.Add(new FanControl ($"{fan.getRPM()} RPM", fan.SpeedClass, fan.FanType));
            }
            fansRendered = true;
        }

        // Function to update a fan's speed onm the UI and update its GIF if it has moved into a new speed class
        private void updateFans()
        {
            int fanIndex = 0;
            foreach (var fan in fans)
            {
                FanControl fanControl = (FanControl)FansContainer.Children[fanIndex];
                fanControl.FanSpeedLabel.Content = $"{fan.getRPM()} RPM";
                if (fan.NeedsUpdate == 0) {
                    fanControl.updateGif(Helpers.createGifPath(GPUString, fan.FanType, fan.SpeedClass));
                }
                fanIndex++;
            }
        }

        

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            windowStateChanging = true;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (WindowState == WindowState.Normal)
                {
                    WindowState = WindowState.Maximized;
                }
                else
                {
                    WindowState = WindowState.Normal;
                }
                
            } else if (e.ChangedButton == MouseButton.Left)
            {
                titleBarClicked = true;
                windowStateChanging = false;
            }
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            titleBarClicked = false;
            windowStateChanging = false;
        }

        private void TitleBar_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (titleBarClicked)
            {
                if (WindowState == WindowState.Maximized && !windowStateChanging)
                {
                    WindowState = WindowState.Normal;
                    this.Top = e.GetPosition(this.titleBar).Y;
                }
                try
                {
                    DragMove();
                } catch (Exception error) {
                    Console.WriteLine(error.Message);
                }                
            }
            
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

        public double ScaleValue
        {
            get
            {
                return (double)GetValue(ScaleValueProperty);
            }
            set
            {
                SetValue(ScaleValueProperty, value);
            }
        }
    }
}
