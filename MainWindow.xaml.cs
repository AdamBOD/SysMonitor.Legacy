using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
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

        public static readonly DependencyProperty ScaleValueProperty = DependencyProperty.Register("ScaleValue", typeof(double), typeof(MainWindow), new UIPropertyMetadata(1.0, new PropertyChangedCallback(OnScaleValueChanged), new CoerceValueCallback(OnCoerceScaleValue)));

        public MainWindow()
        {
            InitializeComponent();
            currentWidth = (int)(Screen.PrimaryScreen.Bounds.Width * 0.6953125);
            previousWidth = currentWidth;

            currentHeight = (int)(Screen.PrimaryScreen.Bounds.Height * 0.6953125);
            previousHeight = currentHeight;

            SetValue(ScaleValueProperty, (double)Screen.PrimaryScreen.Bounds.Width / 1920 - 0.085);

            mainWindow.Width = currentWidth;
            mainWindow.Height = currentHeight;
            computerHardware = new Computer();
            stringFactory = new StringFactory();
            fans = new List<Fan>();
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

            memoryCapacity.Content = "Capacity: " + stringFactory.bytesToGB(new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory) + "GB";

            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += OnTimedEvent;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();

        }

        private void OnTimedEvent(object source, EventArgs e)
        {
            foreach (var hardware in computerHardware.Hardware)
            {
                hardware.Update();
            }
            getSensorData();
        }

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
                                    CPUTemp.Content = stringFactory.temperatureString(hardwareSensor.Value);
                                    if (hardwareSensor.Value < 45)
                                    {
                                        CPUStatusImage.Source = new BitmapImage(new Uri(@"Resources\Images\CPU Status\CPUCool.png", UriKind.Relative));
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
                                GPUTemp.Content = stringFactory.temperatureString(hardwareSensor.Value);
                                if (hardwareSensor.Value < 45)
                                {
                                    GPUStatusImage.Source = new BitmapImage(new Uri(@"Resources\Images\CPU Status\CPUCool.png", UriKind.Relative));
                                }
                                else if (hardwareSensor.Value >= 45 && hardwareSensor.Value < 75)
                                {
                                    GPUStatusImage.Source = new BitmapImage(new Uri(@"Resources\Images\CPU Status\CPUHot.png", UriKind.Relative));
                                }
                                else if (hardwareSensor.Value > 75)
                                {
                                    GPUStatusImage.Source = new BitmapImage(new Uri(@"Images\CPU Status\CPUOverheated.png", UriKind.Relative));
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

        private void updateFans()
        {
            int fanIndex = 0;
            foreach (var fan in fans)
            {
                FanControl fanControl = (FanControl)FansContainer.Children[fanIndex];
                fanControl.FanSpeedLabel.Content = $"{fan.getRPM()} RPM";
                if (fan.NeedsUpdate == 0) {
                    fanControl.updateGif(createGifPath(fan.FanType, fan.SpeedClass));
                }
                fanIndex++;
            }
        }

        public static string createGifPath (string fanType, int speedClass)
        {
            string gifPath = "Resources/GIFs/";
            if (fanType == "CPU")
            {
                gifPath += "CPU_Fans/";
                switch (speedClass)
                {
                    case 0:
                        gifPath += "CPU_Fan_Slow.gif";
                        break;
                    case 1:
                        gifPath += "CPU_Fan_Slow_Medium.gif";
                        break;
                    case 2:
                        gifPath += "CPU_Fan_Medium.gif";
                        break;
                    case 3:
                        gifPath += "CPU_Fan_Medium_Fast.gif";
                        break;
                    case 4:
                        gifPath += "CPU_Fan_Fast.gif";
                        break;
                    case 5:
                        gifPath += "CPU_Fan_Very_Fast.gif";
                        break;
                    default:
                        break;
                }
            } else if (fanType == "GPU")
            {
                gifPath += "GPUs/";
                switch (speedClass)
                {
                    case 6:
                        gifPath += "3-Fan-GPU-Slow-03.gif";
                        break;
                    case 7:
                        gifPath += "3-Fan-GPU-Medium-03.gif";
                        break;
                    case 8:
                        gifPath += "3-Fan-GPU-Fast-03.gif";
                        break;
                    default:
                        break;
                }
            } else if (fanType == "Case")
            {
                gifPath += "Fans/";
                switch (speedClass)
                {
                    case 0:
                        gifPath += "Fan_Slow.gif";
                        break;
                    case 1:
                        gifPath += "Fan_Slow_Medium.gif";
                        break;
                    case 2:
                        gifPath += "Fan_Medium.gif";
                        break;
                    case 3:
                        gifPath += "Fan_Medium_Fast.gif";
                        break;
                    case 4:
                        gifPath += "Fan_Fast.gif";
                        break;
                    case 5:
                        gifPath += "Fan_Very_Fast.gif";
                        break;
                    default:
                        break;
                }
            }
            return gifPath;
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

        private static object OnCoerceScaleValue(DependencyObject o, object value)
        {
            /*MainWindow mainWindow = o as MainWindow;
            if (mainWindow != null)
                return mainWindow.OnCoerceScaleValue((double)value);
            else*/
                return value;
        }

        private static void OnScaleValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            /*MainWindow mainWindow = o as MainWindow;
            if (mainWindow != null)
                mainWindow.OnScaleValueChanged((double)e.OldValue, (double)e.NewValue);*/
        }

        protected virtual double OnCoerceScaleValue(double value)
        {
            /*if (double.IsNaN(value))
                return 1.0f;

            value = Math.Max(0.1, value);*/
            return value;
        }

        protected virtual void OnScaleValueChanged(double oldValue, double newValue)
        {

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

        private void MainGrid_SizeChanged(object sender, EventArgs e)
        {
            CalculateScale();
        }

        private void CalculateScale()
        {
            /*double yScale = ActualHeight / 250f;
            double xScale = ActualWidth / 200f;
            double value = Math.Min(xScale, yScale);
            ScaleValue = (double)OnCoerceScaleValue(mainWindow, value);*/
        }

    }
}
