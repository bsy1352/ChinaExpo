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
    /// Vision.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Vision : UserControl
    {
        ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Vision()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            canvas.Background = new SolidColorBrush(Colors.Black);
            lblInspectResult.Background = new SolidColorBrush(Colors.Black);
        }

        /// <summary>
        /// 트리거 신호에 따른 이미지 변경
        /// </summary>
        /// <param name="triggerIndex"></param>
        public void RaiseTriggerSignal(int triggerIndex)
        {
            try
            {
                if (triggerIndex == 0)
                {
                    canvas.Background = new SolidColorBrush(Colors.Black);
                    lblInspectResult.Background = new SolidColorBrush(Colors.Black);
                }
                else
                {
                    BitmapImage raspberryPie
                        = new BitmapImage(new Uri(string.Format("Assets/Images/trigger_{0}.bmp", triggerIndex), UriKind.Relative));
                    ImageBrush imageBrush = new ImageBrush(raspberryPie);
                    canvas.Background = imageBrush;
                    lblInspectResult.Background = new SolidColorBrush(Colors.Lime);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);      
            }
        }
    }
}
