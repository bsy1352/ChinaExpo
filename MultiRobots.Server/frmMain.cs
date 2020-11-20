using log4net;
using MultiRobots.Manager;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiRobots.Server
{
    public partial class frmMain : Form
    {
        private AutoResetEvent autoEvent;
        private MultiRobots.Net.Server server;
        private RobotManager robotManager;
        private ConcurrentQueue<List<char[]>> sendQueue;

        private int jigIndex = 0;
        private int lastStep = 0;

        public int LoopCount { get; set; } = 1;
        public int CycleCount { get; set; } = 0;
        private bool doneCycle = false; 

        CancellationTokenSource cts;

        ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public frmMain()
        {            

            InitializeComponent();
            RegisterEvents();

            robotManager = new RobotManager();
        }

        /// <summary>
        /// register event 
        /// </summary>
        private void RegisterEvents()
        {
            btnStart.Click += btnStart_Click;
            btnStop.Click += btnStop_Click;
            btnMotor.Click += btnMotorOn_Click;
            btnRestart.Click += btnRestart_Click;
            btnPause.Click += btnPause_Click;
            btnAlarmReset.Click += btnAlarmReset_Click;
            btnJigReset.Click += btnJigReset_Click;

            this.Load += frmMain_Load;
        }

        private void ctxShow_Click(object sender, EventArgs e)
        {
            this.Show();
        }

        private void ctxExit_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Form load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void frmMain_Load(object sender, EventArgs e)
        {
            cts = new CancellationTokenSource();
            
            sendQueue = new ConcurrentQueue<List<char[]>>();           
          
            robotManager.Open("cifX0", 0);
            Thread.Sleep(1000);

            UpdateLogList("리셋");
            robotManager.Reset();
            Thread.Sleep(1000);

            UpdateLogList("프로그램 넣기");
            robotManager.SetProgram();

            // 실행시 파일에 저장한 마지막 실행 스탭을 가져온다.
            lastStep = GetLastStepFromFile();
            LoopCount = GetLoopCount();
            cboWorkCount.SelectedIndex = LoopCount;            

            StartService();            
            StartRobot();
            Restart();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult res = MessageBox.Show("프로그램을 종료 하시겠습니까?"
                    , "확인"
                    , MessageBoxButtons.OKCancel
                    , MessageBoxIcon.Information);
            if (res == DialogResult.OK)
            {

                Stop();
                Process.GetCurrentProcess().Kill();
            }
        }

        /// <summary>
        /// Start socket server
        /// </summary>
        private void StartService()
        {
            try
            {
                autoEvent = new AutoResetEvent(false);

                server = new MultiRobots.Net.Server();
                server.Listen(int.Parse(txtPort.Text));
                server.OnReceiveData += new MultiRobots.Net.Server.ReceiveDataCallback(OnDataReceived);
                server.OnClientConnect += new MultiRobots.Net.Server.ClientConnectCallback(NewClientConnected);
                server.OnClientDisconnect += new MultiRobots.Net.Server.ClientDisconnectCallback(ClientDisconnect);

                UpdateLogList("Start Server");
            }
            catch (Exception ex)
            {
                UpdateLogList(ex.Message);
            }
        }

        /// <summary>
        /// Stop socket server & cancel task
        /// </summary>
        private void Stop()
        {
            try
            {
                if (cts != null)
                    cts.Cancel();

                server.Stop();                

                UpdateLogList("Stop Server");
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }

        /// <summary>
        /// Start robot controller
        /// </summary>
        private void StartRobot()
        {
            try
            {
                cts = new CancellationTokenSource();

                ExecuteRobotSequence(cts.Token);
                GetTriggerSignal(cts.Token);
                SendInformation(cts.Token);

                robotManager.SetTriggerInterval(2);
            }
            catch (Exception ex)
            {
                UpdateLogList(ex.Message);
            }
        }

        /// <summary>
        /// Robot sequence task
        /// </summary>
        /// <param name="token"></param>
        private void ExecuteRobotSequence(CancellationToken token)
        {
            int R1Step = 0, R2Step = 0, R3Step = 0;
            int newStep = 0;

            Task.Factory.StartNew(() => {

                Restart();

                Tuple<bool, int[]> step;

                while (true)
                {
                    try
                    {
                        if (token.IsCancellationRequested)
                            break;

                        if (robotManager.IsReadyCommand())
                        {
                            step = robotManager.GetCurrentStep();
                            if (!step.Item1)
                            {
                                R1Step = step.Item2[0];
                                R2Step = step.Item2[1];
                                R3Step = step.Item2[2];

                                step = null;
                            }
                            else
                            {
                                logger.Error("Read fail current step");

                                Thread.Sleep(500);
                                continue;
                            }
                            

                            logger.DebugFormat("Cycle count = {0}", CycleCount);
                            logger.DebugFormat("R1Step={0}, R2Step={1}, R3Step={2}  => lastStep={3}", R1Step, R2Step, R3Step, lastStep);

                            jigIndex = robotManager.GetCurrentJig(Robots.R1);

                            if (R1Step == 0 && R2Step == 0 && R3Step == 0 && (lastStep == 0 || lastStep == 7 || lastStep == 11))
                            {
                                if (lastStep == 11 && jigIndex == 1 && !doneCycle)
                                {
                                    CycleCount++;

                                    if (LoopCount != 0 && LoopCount <= CycleCount)
                                    {
                                        doneCycle = true;
                                        logger.DebugFormat("모든 사이클을 완료 하였습니다. Cycle Count={0}", CycleCount);

                                        Pause();
                                        Thread.Sleep(1000);

                                        continue;
                                    }
                                }
                                else if (lastStep == 11 && jigIndex == 1 && doneCycle)
                                {
                                    if (SetStep(1))
                                    {
                                        doneCycle = false;
                                        CycleCount = 0;
                                        lastStep = 1;
                                    }
                                }

                                logger.DebugFormat("현재 지그 번호 : {0}", jigIndex);

                                switch (lastStep)
                                {
                                    case 0:
                                    case 7:
                                        if (jigIndex == 5)
                                        {
                                            newStep = 8;
                                        }
                                        else
                                        {
                                            newStep = 1;

                                            if (lastStep == 0 && jigIndex == 0)
                                            {
                                                if (SetJig(1)) jigIndex++;
                                            }
                                            else
                                            {
                                                if (SetJig(jigIndex + 1)) jigIndex++;
                                            }

                                            Thread.Sleep(500);
                                        }

                                        if (SetStep(newStep)) lastStep = newStep;
                                        Thread.Sleep(500);

                                        break;
                                    case 11:
                                        if (jigIndex == 0) jigIndex = 1;

                                        if (jigIndex == 1)
                                        {
                                            newStep = 1;
                                        }
                                        else
                                        {
                                            newStep = 8;

                                            if (SetJig(jigIndex - 1)) jigIndex--;
                                            Thread.Sleep(500);
                                        }

                                        if (SetStep(newStep)) lastStep = newStep;
                                        Thread.Sleep(500);

                                        break;
                                }
                            }
                            else if (R1Step == R2Step && R1Step == R3Step && R1Step == lastStep)
                            {
                                // 최종 스탭을 기준으로 다음 스탭을 설정한다.
                                newStep = lastStep + 1;

                                if (SetStep(newStep)) lastStep = newStep;

                                Thread.Sleep(500);
                            }
                            else if (R1Step == R2Step && R1Step == R3Step && lastStep == 0 && R1Step != 0)
                            {
                                newStep = R1Step + 1;

                                if (SetStep(newStep)) lastStep = newStep;

                                Thread.Sleep(500);
                            }
                            else if (R1Step == R2Step && R1Step == R3Step && lastStep != 0)
                            {
                                newStep = R1Step + 1;

                                if (SetStep(newStep)) lastStep = newStep;

                                Thread.Sleep(500);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.Message);
                    }

                    Thread.Sleep(1000);
                }
            });
        }

        /// <summary>
        /// Trigger signale pooling & send to client
        /// </summary>
        /// <param name="token"></param>
        private void GetTriggerSignal(CancellationToken token)
        {            
            Task.Factory.StartNew(() => 
            {
                int triggerIndex = -1;

                while(true)
                {
                    if (token.IsCancellationRequested)
                        break;

                    int newTriggerIndex = robotManager.GetTriggerSignal();
                    if (triggerIndex != newTriggerIndex)
                    {
                        triggerIndex = newTriggerIndex;

                        if (lastStep == 3 || lastStep == 5)
                        {
                            string message = string.Format("/trigger:{0}", triggerIndex.ToString());
                            SendToClientMessage(message);
                            Thread.Sleep(500);
                            SendToClientMessage(message);
                            Thread.Sleep(500);
                            SendToClientMessage(message);
                            Thread.Sleep(500);
                            SendToClientMessage(message);
                        }
                        //else if (lastStep >= 1 || lastStep >= 6)
                        //{
                        //    string message = "/trigger:0";
                        //    SendToClientMessage(message);
                        //}
                    }

                    Thread.Sleep(200);
                }               
            });
        }

        /// <summary>
        /// Get information about robot status
        /// </summary>
        /// <param name="token"></param>
        private void SendInformation(CancellationToken token)
        {
            Task.Factory.StartNew(() => 
            {
                while(true)
                {
                    if (token.IsCancellationRequested)
                        break;

                    try
                    {
                        Tuple<bool, List<char[]>> data = robotManager.GetRobotData();
                        if (data != null && !data.Item1)
                        {
                            if (!string.IsNullOrEmpty(data.Item2[1][0].ToString()))
                            {
                                SendToClientMessage(string.Format("/running:{0}", data.Item2[1][0]));
                                Thread.Sleep(500);
                            }
                                                        
                            SendToClientMessage(string.Format("/status:{0},{1},{2}", new string(data.Item2[0]), new string(data.Item2[16]), new string(data.Item2[32])));
                            Thread.Sleep(500);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.Message);
                    }

                    Thread.Sleep(500);
                }
            });
        }


        /// <summary>
        /// Send jig number to robot
        /// </summary>
        /// <param name="jigIndex"></param>
        /// <returns></returns>
        private bool SetJig(int jigIndex)
        {
            var ret = false;
            try
            {
                if (robotManager.IsReadyCommand())
                {
                    robotManager.SetAssyJIG(jigIndex);
                    ret = true;

                    UpdateLogList(string.Format("지그선택 = {0}", jigIndex));
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                ret = false;
            }
            return ret;
        }

        /// <summary>
        /// Send step to robot
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        private bool SetStep(int step)
        {
            var ret = false;
            try
            {
                if (robotManager.IsReadyCommand())
                {
                    robotManager.SendOrder(step);
                    Thread.Sleep(500);
                    robotManager.SendOrder(0);
                    ret = true;

                    SetLastStepToFile(step);
                    UpdateLogList(string.Format("Step = {0} 오더 Send", step));
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                ret = false;
            }
            return ret;
        }        

        /// <summary>
        /// Receive data callback method
        /// </summary>
        /// <param name="clientNumber"></param>
        /// <param name="message"></param>
        /// <param name="messageSize"></param>
        void OnDataReceived(int clientNumber, byte[] message, int messageSize)
        {
            try
            {
                string strMessage = Encoding.Default.GetString(message).Trim('\0');
                logger.Debug(strMessage);
                if (strMessage.IndexOf(':') > -1)
                {
                    string[] command = strMessage.Split(new char[] {':'});
                    
                    int robotIndex = -1;
                    if (command[1] != null && !string.IsNullOrEmpty(command[1]))
                    {
                        if (int.TryParse(command[1], out robotIndex))
                        {
                            robotIndex = int.Parse(command[1]);
                        }
                    }

                    logger.DebugFormat("Receive command : {0}", command[0]);

                    switch (command[0])
                    {
                        case "/motoron":
                            MotorOn(robotIndex);
                            break;
                        case "/restart":
                            Restart(robotIndex);
                            break;
                        case "/robotstop":
                            Pause(robotIndex);
                            break;
                        case "/alarmreset":
                            AlaramReset(robotIndex);
                            break;
                        case "/orderstart":
                            robotManager.OrderStart();
                            break;
                        case "/orderstop":
                            robotManager.OrderStop();
                            break;
                        case "/loopcount":
                            LoopCount = int.Parse(command[1]);
                            UpdateLogList(string.Format("Loop 카운트가 변경 되었습니다 = {0}", LoopCount));
                            SetLoopCount(LoopCount);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }

            autoEvent.Set();
        }

        /// <summary>
        /// New client connected callback method
        /// </summary>
        /// <param name="ConnectionID"></param>
        void NewClientConnected(int ConnectionID)
        {
            UpdateLogList("kiosk computer connected!");
            SendToClientMessage(string.Format("/loopcount:{0}", LoopCount));
        }

        /// <summary>
        /// Disconnect callback method
        /// </summary>
        /// <param name="clientNumber"></param>
        void ClientDisconnect(int clientNumber)
        {
            UpdateLogList("kiosk computer disconnected!");
        }

        /// <summary>
        /// Motor on
        /// </summary>
        void MotorOn()
        {
            robotManager.MotorOn();
            UpdateLogList("Motor on");
        }

        void MotorOn(int robotIndex)
        {
            if (robotIndex > -1) robotManager.MotorOn((Robots)robotIndex);
            else robotManager.MotorOn();
        }


        /// <summary>
        /// Restart robot
        /// </summary>
        void Restart()
        {          
            InvokeUI(() =>
            {                
                robotManager.RobotRun();
                Thread.Sleep(300);
                //robotManager.RobotRunReset();

                Thread.Sleep(500);
                UpdateLogList("작업시작");
                robotManager.OrderStart();

                btnStart.Enabled = false;
                btnStop.Enabled = true;                
            });
        }

        void Restart(int robotIndex)
        {
            logger.DebugFormat("Retart = {0}", robotIndex);

            if (robotIndex > -1)
            {
                InvokeUI(() =>
                {
                    robotManager.RobotRun((Robots)robotIndex);
                    Thread.Sleep(300);
                    //robotManager.RobotRunReset();

                    Thread.Sleep(500);
                    UpdateLogList("작업시작");
                    robotManager.OrderStart((Robots)robotIndex);

                    btnStart.Enabled = false;
                    btnStop.Enabled = true;
                });
            }
            else
            {
                Restart();
            }
        }

        /// <summary>
        /// Pause robot 
        /// </summary>
        void Pause()
        {
            robotManager.RobotPause();
        }

        void Pause(int robotIndex)
        {
            if (robotIndex > -1) robotManager.RobotPause((Robots)robotIndex);
            else Pause();
        }

        /// <summary>
        /// Alarm reset
        /// </summary>
        void AlaramReset()
        {
            robotManager.AlarmRelease();
        }

        void AlaramReset(int robotIndex)
        {
            if (robotIndex > -1) robotManager.AlarmRelease((Robots)robotIndex);
            else AlaramReset();
        }

        #region button event handler
        void btnStart_Click(object sender, EventArgs e)
        {
            cts = new CancellationTokenSource();
            StartRobot();
            Restart();
        }

        void btnStop_Click(object sender, EventArgs e)
        {
            try
            {
                Stop();

                btnStart.Enabled = true;
                btnStop.Enabled = false;

                UpdateLogList("Stop Service");
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                UpdateLogList(ex.Message);
            }
        }

        void btnMotorOn_Click(object sender, EventArgs e)
        {
            MotorOn();
        }

        void btnRestart_Click(object sender, EventArgs e)
        {
            Restart();
        }

        void btnPause_Click(object sender, EventArgs e)
        {
            Pause();
        }

        void btnAlarmReset_Click(object sender, EventArgs e)
        {
            AlaramReset();
        }

        void btnOrderStart_Click(object sender, EventArgs e)
        {
            robotManager.OrderStart();
        }

        void btnOrderStop_Click(object sender, EventArgs e)
        {
            robotManager.OrderStop();
        }

        private void btnSendToClient_Click(object sender, EventArgs e)
        {
            if (server.IsListening && server.workerSockets.Count > 0)
            {
                int triggerNo = 0;

                server.SendMessage(Encoding.Default.GetBytes(string.Format("/trigger:{0}", triggerNo.ToString())));
                UpdateLogList("Sent trigger signal to viewer!");
            }
        }

        void btnJigReset_Click(object sender, EventArgs e)
        {
            if (robotManager.Running(Robots.R1))
            {
                MessageBox.Show("운전중에 초기화 할 수 없습니다.");
            }
            else
            {
                DialogResult res = MessageBox.Show("초기화 하시겠습니까? \r\n반드시 모든 로봇을 초기화 하시고 수행하세요!"
                                                , "확인"
                                                , MessageBoxButtons.OKCancel
                                                , MessageBoxIcon.Information);
                if (res == DialogResult.OK)
                {
                    if (robotManager.SetAssyJIG(0))
                    {
                        if(robotManager.SendOrder(0))
                        {
                            SetLastStepToFile(0);
                            lastStep = 0;
                            MessageBox.Show("초기화 되었습니다.");
                        }                            
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Thread invoker
        /// </summary>
        /// <param name="action"></param>
        private void InvokeUI(Action action)
        {
            this.Invoke(action);
        }

        /// <summary>
        /// listview bind 
        /// </summary>
        /// <param name="message"></param>
        private void UpdateLogList(string message)
        {
            InvokeUI(() => 
            {
                logger.Debug(message);
                string[] str = new string[2];
                str[0] = string.Format("{0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString());
                str[1] = message;
                ListViewItem item = new ListViewItem(str);
                listLog.Items.Add(item);
                listLog.EnsureVisible(listLog.Items.Count - 1);
            });
        }

        /// <summary>
        /// Send message to connected client.
        /// </summary>
        /// <param name="message"></param>
        private void SendToClientMessage(string message)
        {            
            SendToClientMessage(Encoding.Default.GetBytes(message));
        }

        private object obj = new object();

        /// <summary>
        /// Send message to connected client.
        /// </summary>
        /// <param name="message"></param>
        private void SendToClientMessage(byte[] message)
        {
            lock(obj)
            {
                if (server.IsListening && server.workerSockets.Count > 0)
                {
                    server.SendMessage(message);
                }
            }
        }

        private void CboWorkCount_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoopCount = int.Parse(cboWorkCount.Items[cboWorkCount.SelectedIndex].ToString());
            SetLoopCount(LoopCount);
            UpdateLogList(string.Format("Loop 카운트가 변경 되었습니다 = {0}", LoopCount));
        }

        /// <summary>
        /// set last step to file
        /// </summary>
        /// <param name="lastStep"></param>
        private void SetLastStepToFile(int lastStep)
        {
            WritePrivateProfileString("ROBOT", "STEP", lastStep.ToString(), string.Format(@"{0}\robot.ini", Application.StartupPath));
        }

        /// <summary>
        /// get last step from file
        /// </summary>
        /// <returns></returns>
        private int GetLastStepFromFile()
        {
            int step = 0;

            try
            {
                StringBuilder lastStep = new StringBuilder();
                GetPrivateProfileString("ROBOT", "STEP", "0", lastStep, 32, string.Format(@"{0}\robot.ini", Application.StartupPath));
                step = int.Parse(lastStep.ToString());
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }

            return step;
        }

        /// <summary>
        /// get loop count from file
        /// </summary>
        /// <returns></returns>
        private int GetLoopCount()
        {
            int loopCount = 0;

            try
            {
                StringBuilder lastLoop = new StringBuilder();
                GetPrivateProfileString("ROBOT", "LOOP", "0", lastLoop, 32, string.Format(@"{0}\robot.ini", Application.StartupPath));
                loopCount = int.Parse(lastLoop.ToString());
                LoopCount = loopCount;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }

            return loopCount;
        }

        /// <summary>
        /// Set loop count to file
        /// </summary>
        /// <param name="loopCount"></param>
        private void SetLoopCount(int loopCount)
        {
            WritePrivateProfileString("ROBOT", "LOOP", loopCount.ToString(), string.Format(@"{0}\robot.ini", Application.StartupPath));
        }

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
    }
}
