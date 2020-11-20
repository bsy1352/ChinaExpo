using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MultiRobots.Viewer.Pages
{
    /// <summary>
    /// Settings.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Settings : UserControl
    {
        ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private SolidColorBrush on = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00AB56"));
        private SolidColorBrush off = new SolidColorBrush(Colors.Red);
        private SolidColorBrush normal = new SolidColorBrush(Colors.Gray);

        MainWindow parentWindow;

        public MainWindow ParentWindow
        {
            get { return parentWindow; }
            set { parentWindow = value; }
        }

        public Settings()
        {
            InitializeComponent();
        }

        public void DisplayRobotStatus(string status)
        {           
            try
            {
                //logger.DebugFormat("status={0}", status);

                if (!string.IsNullOrEmpty(status))
                {
                    string[] arrStatus = status.Split(new char[] { ',' });

                    char[] r1 = new char[8];
                    char[] r2 = new char[8];
                    char[] r3 = new char[8];

                    if (arrStatus.Length == 3)
                    {
                        r1 = arrStatus[0].ToCharArray();
                        r2 = arrStatus[1].ToCharArray();
                        r3 = arrStatus[2].ToCharArray();
                    }

                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        R1AutoMode.Fill = (r1[4] == '1') ? on : off;
                        btnR1MotorOn.Background = r1[0] == '1' ? on : normal;
                        //btnR1Run.Background = r1[1] == '1' ? on : normal;
                        //btnR1Pause.Background = r1[2] == '1' ? on : normal;
                        //btnR1AlarmReset.Background = r1[3] == '1' ? on : normal;

                        R2AutoMode.Fill = r2[4] == '1' ? on : off;
                        btnR2MotorOn.Background = r2[0] == '1' ? on : normal;
                        //btnR2Run.Background = r2[1] == '1' ? on : normal;
                        //btnR2Pause.Background = r2[2] == '1' ? on : normal;
                        //btnR2AlarmReset.Background = r2[3] == '1' ? on : normal;

                        R3AutoMode.Fill = r3[4] == '1' ? on : off;
                        btnR3MotorOn.Background = r3[0] == '1' ? on : normal;
                        //btnR3Run.Background = r3[1] == '1' ? on : normal;
                        //btnR3Pause.Background = r3[2] == '1' ? on : normal;
                        //btnR3AlarmReset.Background = r3[3] == '1' ? on : normal;
                    }));
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }

        private void BtnMotorOn_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            SendData(string.Format("/motoron:{0}", button.Tag.ToString()));
        }

        private void BtnRun_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            SendData(string.Format("/restart:{0}", button.Tag.ToString()));
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            SendData(string.Format("/robotstop:{0}", button.Tag.ToString()));
        }

        private void BtnAlarmReset_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            SendData(string.Format("/alarmreset:{0}", button.Tag.ToString()));
        }

        private void SendData(string message)
        {
            try
            {
                if (ParentWindow != null)
                {
                    ParentWindow.SendData(message);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }

        private void BtnRestart_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            SendData(string.Format("/restart:", button.Tag.ToString()));
        }

        private void BtnPause_Click_1(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            SendData(string.Format("/robotstop:", button.Tag.ToString()));
        }

        private void BtnAlarmReset_Click_1(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            SendData(string.Format("/alarmreset:", button.Tag.ToString()));
        }

        private void ToggleInfinity_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ToggleButton button = (ToggleButton)sender;
                if (button.Selected)
                {
                    SendData("/loopcount:0");
                }
                else
                {
                    SendData("/loopcount:1");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }

        public void SetLoopCount(int loopCount)
        {
            try
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    toggleInfinity.Selected = loopCount == 0 ? false : true;
                }));
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }
    }
}
