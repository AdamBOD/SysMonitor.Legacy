using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;
using static SysMonitor.MainWindow;

namespace SysMonitor
{
    public partial class FanControl : UserControl
    {
        public static DependencyProperty FanNameProperty = DependencyProperty.Register("FanName", typeof(string), typeof(Fan));
        public string FanName
        {
            get { return (string)GetValue(FanNameProperty); }
            set { SetValue(FanNameProperty, value); }
        }

        public static readonly DependencyProperty FanRPMProperty = DependencyProperty.Register("FanRPM", typeof(string), typeof(Fan));
        public string FanRPM
        {
            get { return (string)GetValue(FanRPMProperty); }
            set { SetValue(FanRPMProperty, value); }
        }

        private string fanType;
        public string FanType
        {
            get { return fanType; }
            set { fanType = value; }
        }

        private int speedClass = 0;

        public int getSpeedClass ()
        {
            return speedClass;
        }

        public void setSpeedClass (int newSpeedClass)
        {
            speedClass = newSpeedClass;
        }

        public FanControl(string fanRPM, int speedClass, string fanType) 
        {
            InitializeComponent();
            this.DataContext = this;
            FanRPM = fanRPM;
            FanType = fanType;
            setSpeedClass(speedClass);

            if (FanType == "CPU")
            {
                FanName = "CPU Fan";
                FanGif.Width = 135;
            } else if (FanType == "Case")
            {
                FanName = "Case Fan";
                FanGif.Width = 135;
            } else if (FanType == "GPU")
            {
                FanName = "GPU Fan";
                FanGif.Height = 135;
            }

            loadGif();
        }

        public void loadGif()
        {
            var image = new BitmapImage(new Uri(@"" + createGifPath(fanType, speedClass), UriKind.Relative));
            ImageBehavior.SetAnimatedSource(FanGif, image);
        }

        public void updateGif (string path)
        {
            var image = new BitmapImage(new Uri(@"" + path, UriKind.Relative));
            ImageBehavior.SetAnimatedSource(FanGif, image);
        }
    }
}
