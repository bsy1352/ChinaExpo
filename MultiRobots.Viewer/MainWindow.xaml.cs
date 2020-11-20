using log4net;
using MultiRobots.Viewer.Pages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace MultiRobots.Viewer
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private MultiRobots.Net.Client client;
        private CancellationTokenSource cts;
        private ConcurrentQueue<byte[]> receiveQueue;

        DispatcherTimer timer;

        ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private bool _loadCompleted = false;
        private bool _started = false;
        private bool _setting = true;

        BitmapImage bitmapImagePlay = new BitmapImage(new Uri("Assets/Images/play-circle-outline.png", UriKind.Relative));
        BitmapImage bitmapImageStop = new BitmapImage(new Uri("Assets/Images/stop-circle-outline.png", UriKind.Relative));

        Image btnStateImage;
        TextBlock txtState;

        Vision vision;
        Settings settings;

        private string robotStatus = "";

        public MainWindow()
        {
            InitializeComponent();
            
            this.Loaded += (_sender, _e) =>
            {
                cts = new CancellationTokenSource();
                receiveQueue = new ConcurrentQueue<byte[]>();

                vision = new Vision();
                settings = new Settings();

                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(1);
                timer.Tick += Timer_Tick;

                ConnectServer();
                RobotStatus(cts.Token);
                ReceiveInformation(cts.Token);
            };

            this.Unloaded += (_sender, _e) => 
            {
                if (cts != null)
                {
                    cts.Cancel();
                    cts = null;
                }

                Stop();
            };
        }

        /// <summary>
        /// Timer tick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            ConnectServer();
        }

        /// <summary>
        /// Home button event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            _setting = false;
            frame_content.Source = new Uri("Pages/Home.xaml", UriKind.Relative);
        }

        /// <summary>
        /// Setting button event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSetting_Click(object sender, RoutedEventArgs e)
        {
            _setting = true;
            settings.ParentWindow = this;
            frame_content.NavigationService.Navigate(settings);
        }

        /// <summary>
        /// Power button event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPower_Click(object sender, RoutedEventArgs e)
        {
            this.Close();            
        }

        /// <summary>
        /// Start/Stop button event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnState_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _started = !_started;
                StartStop(_started, true);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Connect server
        /// </summary>
        void ConnectServer()
        {
            try
            {
                client = new Net.Client();
                client.OnDisconnected += OnDisconnect;
                client.OnReceiveData += OnDataReceived;

                client.Connect(IPAddress.Parse("192.168.10.5"), 9999);
                //client.Connect(IPAddress.Parse("127.0.0.1"), 9999);

                if (timer != null && timer.IsEnabled)
                {
                    timer.Stop();
                }
            }
            catch (Exception ex)
            {
                if (timer != null && !timer.IsEnabled)
                {
                    timer.Start();
                }

                logger.Error(ex.Message);
            }
        }


        /// <summary>
        /// Stop server & timer
        /// </summary>
        void Stop()
        {
            try
            {
                if (timer != null && timer.IsEnabled)
                {
                    timer.Stop();
                    timer = null;
                } 
                if (client != null && client.Connected)
                {
                    client.Disconnect();

                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }

        /// <summary>
        /// Disconnect callback method
        /// </summary>
        void OnDisconnect()
        {
            logger.Debug("Disconnected server!");
            if (timer != null && !timer.IsEnabled)
            {
                timer.Start();
            }
        }

        /// <summary>
        /// Receive data callback method
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageSize"></param>
        void OnDataReceived(byte[] message, int messageSize)
        {
            try
            {
                if (receiveQueue != null)
                {
                    receiveQueue.Enqueue(message);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
            //string strMessage = Encoding.Default.GetString(message).Trim('\0');
            //if (strMessage.IndexOf(':') > -1)
            //{
            //    string[] command = strMessage.Split(new char[] { ':' });

            //    switch (command[0])
            //    {
            //        case "/trigger":
            //            int triggerIndex = int.Parse(command[1]);
            //            DisplayVision(triggerIndex);
            //            logger.DebugFormat(string.Format("Trigger number = {0}", command[1]));
            //            break;
            //        case "/running":
            //            if (!_loadCompleted)
            //            {
            //                _loadCompleted = true;
            //                _setting = false;
            //            }
            //            StartStop((command[1] == "1") ? true : false, false);
            //            break;
            //        case "/status":
            //            robotStatus = command[1];
            //            break;
            //    }
            //}
        }

        void ReceiveInformation(CancellationToken token)
        {
            Task.Factory.StartNew(() =>
            {
                byte[] message;

                while (true)
                {
                    if (token.IsCancellationRequested)
                        break;

                    try
                    {
                        if (receiveQueue != null && receiveQueue.TryDequeue(out message))
                        {
                            if (message != null && message.Length > 0)
                            {
                                string strMessage = Encoding.Default.GetString(message).Trim('\0');
                                if (strMessage.IndexOf(':') > -1)
                                {
                                    string[] command = strMessage.Split(new char[] { ':' });

                                    if (!string.IsNullOrEmpty(command[1]))
                                    {
                                        switch (command[0])
                                        {
                                            case "/trigger":
                                                int triggerIndex = 0;
                                                if (int.TryParse(command[1], out triggerIndex))
                                                {
                                                    int.Parse(command[1]);
                                                    DisplayVision(triggerIndex);
                                                    logger.DebugFormat(string.Format("Trigger number = {0}", command[1]));
                                                }
                                                break;
                                            case "/running":

                                                if (!_loadCompleted)
                                                {
                                                    _loadCompleted = true;
                                                    _setting = false;
                                                }
                                                StartStop((command[1] == "1") ? true : false, false);
                                                break;
                                            case "/status":
                                                robotStatus = command[1];
                                                break;
                                            case "/loopcount":
                                                int loopCount = int.Parse(command[1]);
                                                settings.SetLoopCount(loopCount);
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.StackTrace);
                    }

                    Thread.Sleep(200);
                }
            });
        }


        void RobotStatus(CancellationToken token)
        {
            Task.Factory.StartNew(() => 
            {
                while(true)
                {
                    try
                    {
                        if (token.IsCancellationRequested)
                            break;

                        if (client != null && client.Connected)
                            settings.DisplayRobotStatus(robotStatus);
                        else
                            settings.DisplayRobotStatus("00000000,00000000,0000000");
                        robotStatus = "";

                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.Message);
                    }

                    Thread.Sleep(2000);
                }
            });
        }

        /// <summary>
        /// Send data to server
        /// </summary>
        /// <param name="message"></param>
        public void SendData(string message)
        {
            try
            {
                if (client != null && client.Connected)
                {
                    logger.DebugFormat("SendData = {0}", message);
                    client.SendMessage(Encoding.UTF8.GetBytes(message));
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }

        /// <summary>
        /// Display vision picture by trigger index
        /// </summary>
        /// <param name="triggerIndex"></param>
        void DisplayVision(int triggerIndex)
        {
            Task.Factory.StartNew(() => 
            {
                try
                {
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (!_setting)
                        {
                            if (triggerIndex == 0)
                            {
                                vision.RaiseTriggerSignal(triggerIndex);
                            }
                            else if (triggerIndex > 0)
                            {
                                frame_content.NavigationService.Navigate(vision);
                                vision.RaiseTriggerSignal(triggerIndex);
                            }
                        }
                    }));
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                }
            });
        }


        private void StartStop(bool running, bool sendServer)
        {
            Task.Factory.StartNew(() => 
            {
                try
                {
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        btnStateImage = FindVisualChildByName<Image>(btnState, "imgStartStop");
                        txtState = FindVisualChildByName<TextBlock>(btnState, "txtStartStop");

                        if (running)
                        {
                            txtState.Text = "STOP";
                            btnState.Background = new SolidColorBrush(Colors.Red);
                            btnStateImage.Source = bitmapImageStop;

                            if (!_setting) frame_content.NavigationService.Navigate(vision);
                        }
                        else
                        {
                            logger.DebugFormat("robot is not running");

                            txtState.Text = "START";
                            btnState.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00AB56"));
                            btnStateImage.Source = bitmapImagePlay;

                            if (!_setting) frame_content.Source = new Uri("Pages/Home.xaml", UriKind.Relative);
                        }

                        if (sendServer) SendData(_started ? "/restart:" : "/robotstop:");
                    }));

                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    MessageBox.Show(ex.Message);
                }
            });

        }

        private T FindVisualChildByName<T>(DependencyObject parent, string name) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                string controlName = child.GetValue(Control.NameProperty) as string;

                if (controlName == name)
                {
                    return child as T;
                }
                else
                {
                    T result = FindVisualChildByName<T>(child, name);

                    if (result != null)

                        return result;
                }
            }

            return null;
        }


    }
}
