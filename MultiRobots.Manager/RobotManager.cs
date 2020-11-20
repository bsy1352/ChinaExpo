using log4net;
using MultiRobots.EthernetIP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultiRobots.Manager
{
    public class RobotManager
    {
        CancellationTokenSource cts;

        ILog logger;

        private cifXUser cifXUser;
        private cifXBase cifXBase;

        private string strBoard;
        private int iChannel;

        private uint ReadSize = 16;
        private uint WriteSize = 16;

        public static string CmdMotorOn = BitStrToHex("10000000");          // 로봇 모터 ON
        public static string CmdRobotRun = BitStrToHex("01000000");         // 로봇 기동
        public static string CmdRobotRunReset = BitStrToHex("10000000");    // 로봇 기동 리셋
        public static string CmdRobotPause = BitStrToHex("00100000");       // 로봇 일시정지
        public static string CmdAlarmRelease = BitStrToHex("00010000");     // 이상해제
        public static string CmdReset = BitStrToHex("00000000");            // 로봇 오더 초기화
        public static string CmdInterLock = BitStrToHex("10000000");        // 로봇 Interlock 

        public static string CmdOrderStart = BitStrToHex("10000000");       // 작업 시작
        public static string CmdOrderStop = BitStrToHex("01000000");        // 작업 정지
        public static string CmdSetHomePosition = BitStrToHex("00100000");  // 홈 포지션
        public static string CmdForward = BitStrToHex("00000000");          // 작업 방향 (정방향)
        public static string CmdBackward = BitStrToHex("10000000");         // 작업 방향 (역방향)


        public static string[] CmdSetAssyJIG = new string[] 
        {
            BitStrToHex("00000000"),        // 지그 조립 영역 초기화
            BitStrToHex("10000000"),        // 지그1번 조립 || 해체
            BitStrToHex("01000000"),        // 지그2번 조립 || 해체
            BitStrToHex("00100000"),        // 지그3번 조립 || 해체
            BitStrToHex("00010000"),        // 지그4번 조립 || 해체
            BitStrToHex("00001000"),        // 지그5번 조립 || 해체            
        };

        public static string[] CmdOrderSteps = new string[]
        {
            string.Format("{0}{1}", BitStrToHex("00000000"), BitStrToHex("00000000")),  // 초기화 
            string.Format("{0}{1}", BitStrToHex("10000000"), BitStrToHex("00000000")),  // Step #0
            string.Format("{0}{1}", BitStrToHex("01000000"), BitStrToHex("00000000")),  // Step #1
            string.Format("{0}{1}", BitStrToHex("00100000"), BitStrToHex("00000000")),  // Step #2
            string.Format("{0}{1}", BitStrToHex("00010000"), BitStrToHex("00000000")),  // Step #3
            string.Format("{0}{1}", BitStrToHex("00001000"), BitStrToHex("00000000")),  // Step #4
            string.Format("{0}{1}", BitStrToHex("00000100"), BitStrToHex("00000000")),  // Step #5
            string.Format("{0}{1}", BitStrToHex("00000010"), BitStrToHex("00000000")),  // Step #6
            string.Format("{0}{1}", BitStrToHex("00000001"), BitStrToHex("00000000")),  // Step #7
            string.Format("{0}{1}", BitStrToHex("00000000"), BitStrToHex("10000000")),  // Step #8
            string.Format("{0}{1}", BitStrToHex("00000000"), BitStrToHex("01000000")),  // Step #9
            string.Format("{0}{1}", BitStrToHex("00000000"), BitStrToHex("00100000")),  // Step #10
            string.Format("{0}{1}", BitStrToHex("00000000"), BitStrToHex("00010000")),  // Step #11
            string.Format("{0}{1}", BitStrToHex("00000000"), BitStrToHex("00001000")),  // Step #12
            string.Format("{0}{1}", BitStrToHex("00000000"), BitStrToHex("00000100")),  // Step #13
            string.Format("{0}{1}", BitStrToHex("00000000"), BitStrToHex("00000010"))   // Step #14            
        };

        /// <summary>
        /// Robot 매니져
        /// </summary>
        public RobotManager()
        {
            strBoard = "";
            iChannel = 0;

            cifXUser = new cifXUser();
            cifXBase = new cifXBase();
        }

        /// <summary>
        /// Ethernet IP 열기
        /// </summary>
        /// <param name="board"></param>
        /// <param name="channel"></param>
        public UInt32 Open(string board, int channel)
        {
            UInt32 lret = 9999;
            lret = cifXUser.xSysdeviceOpen(board);
            lret = cifXUser.xChannelOpen(board, channel);

            if (lret == 0)
            {
                cifXUser.ActiveChannel = channel;
                cifXUser.ActiveBoard = board;

                cts = new CancellationTokenSource();
                interLock(cts.Token);
            }

            Thread.Sleep(2000);

            return lret;
        }

        private void interLock(CancellationToken token)
        {
            //Task.Factory.StartNew(() =>
            //{
            //    while (true)
            //    {
            //        if (token.IsCancellationRequested)
            //            break;

            //        RaiseInterLock(Robots.R3, IsInterLock(Robots.R1));
            //        RaiseInterLock(Robots.R1, IsInterLock(Robots.R3));

            //        //if (Running(Robots.R1) && IsInterLock(Robots.R1))
            //        //{
            //        //    //RaiseInterLock(Robots.R2);
            //        //    RaiseInterLock(Robots.R3, true);
            //        //}
            //        ////else if (Running(Robots.R2) && IsInterLock(Robots.R2))
            //        ////{
            //        ////    RaiseInterLock(Robots.R1);
            //        ////    RaiseInterLock(Robots.R3);
            //        ////}
            //        //else if (Running(Robots.R3) && IsInterLock(Robots.R3))
            //        //{
            //        //    RaiseInterLock(Robots.R1, true);
            //        //    //RaiseInterLock(Robots.R2);
            //        //}

            //        Thread.Sleep(500);
            //    }
            //});
        }

        /// <summary>
        /// Ethernet IP 닫기
        /// </summary>
        public void Close()
        {
            try
            {
                if (cts != null)
                    cts.Cancel();

                UInt32 lret = 9999;

                lret = cifXUser.xChannelClose();
                lret = cifXUser.xSysdeviceClose();
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }

        /// <summary>
        /// 연결확인
        /// </summary>
        public bool IsConnected
        {
            // 로봇 통신 체크로직
            get { return cifXUser.xChannelInfo() == 0; }
        }

        /// <summary>
        /// 바이트 읽기
        /// </summary>
        /// <param name="ulAreaNumber"></param>
        /// <param name="ulOffset"></param>
        /// <param name="ulDataLen"></param>
        /// <returns></returns>
        public byte[] ReadBytes(UInt32 ulAreaNumber, UInt32 ulOffset, UInt32 ulDataLen)
        {
            byte[] pvData = null;

            try
            {
                UInt32 lret = 0;

                if (ulDataLen > 0)
                {
                    pvData = new byte[ulDataLen];

                    lret = cifXUser.xChannelIORead(ulAreaNumber, ulOffset, ulDataLen, ref pvData);
                    if (lret != 0)
                    {
                        logger.ErrorFormat("ReadBytes Error:{0}", cifXBase.SetLastError(lret));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }

            return pvData;
        }

        /// <summary>
        /// 데이터 읽기
        /// </summary>
        /// <param name="ulAreaNumber"></param>
        /// <param name="ulOffset"></param>
        /// <param name="ulDataLen"></param>
        /// <returns></returns>
        public string ReadData(UInt32 ulAreaNumber, UInt32 ulOffset, UInt32 ulDataLen)
        {
            string ret = "";

            try
            {
                UInt32 lret = 0;

                if (ulDataLen > 0)
                {
                    byte[] pvData = new byte[ulDataLen];

                    lret = cifXUser.xChannelIORead(ulAreaNumber, ulOffset, ulDataLen, ref pvData);
                    if (lret != 0)
                    {
                        //logger.ErrorFormat("ReadData Error:{0}", cifXBase.SetLastError(lret));
                    }

                    foreach (byte sByte in pvData)
                        ret += string.Format("{0:X2}", sByte) + " ";
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }

            return ret;
        }


        /// <summary>
        /// 데이터 읽기
        /// </summary>
        /// <param name="ulOffset"></param>
        /// <param name="ulDataLen"></param>
        /// <returns></returns>
        private Tuple<bool, List<Char[]>> Read(UInt32 ulOffset, UInt32 ulDataLen)
        {
            UInt32 lret = 0;
            UInt32 ulAreaNumber = Convert.ToUInt32(0);
            bool isError = false;

            List<Char[]> data = new List<char[]>();

            try
            {
                if (ulDataLen > 0)
                {
                    byte[] pvData = new byte[ulDataLen];

                    lret = cifXUser.xChannelIORead(ulAreaNumber, ulOffset, ulDataLen, ref pvData);
                    if (lret != 0)
                    {
                        isError = true;
                        //logger.ErrorFormat("ReadData Error:{0}", cifXBase.SetLastError(lret));
                    }

                    foreach (byte sByte in pvData)
                    {
                        char[] charArray = Convert.ToString(sByte, 2).PadLeft(8, '0').ToArray();
                        Array.Reverse(charArray);
                        data.Add(charArray);
                    }
                }
            }
            catch (Exception ex)
            {
                isError = true;
                logger.Error(ex.ToString());
            }

            return new Tuple<bool, List<char[]>>(isError, data);
        }

        /// <summary>
        /// 데이터 읽기
        /// </summary>
        /// <param name="ulOffset"></param>
        /// <param name="ulDataLen"></param>
        /// <returns></returns>
        private List<Char[]> ReadData(UInt32 ulOffset, UInt32 ulDataLen)
        {
            UInt32 lret = 0;
            UInt32 ulAreaNumber = Convert.ToUInt32(0);

            List<Char[]> data = new List<char[]>();

            try
            {
                if (ulDataLen > 0)
                {
                    byte[] pvData = new byte[ulDataLen];

                    lret = cifXUser.xChannelIORead(ulAreaNumber, ulOffset, ulDataLen, ref pvData);
                    if (lret != 0)
                    {
                        //logger.ErrorFormat("ReadData Error:{0}", cifXBase.SetLastError(lret));
                        Debug.WriteLine("ReadData Error:{0}", cifXBase.SetLastError(lret));
                    }

                    foreach (byte sByte in pvData)
                    {
                        char[] charArray = Convert.ToString(sByte, 2).PadLeft(8, '0').ToArray();
                        Array.Reverse(charArray);
                        data.Add(charArray);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }

            return data;
        }

        //private char[] ReadData(UInt32 ulOffset, UInt32 ulDataLen)
        //{
        //    UInt32 lret = 0;
        //    UInt32 ulAreaNumber = Convert.ToUInt32(0);

        //    char[] data = new char[8 * ulDataLen];

        //    try
        //    {
        //        if (ulDataLen > 0)
        //        {
        //            byte[] pvData = new byte[ulDataLen];

        //            lret = cifXUser.xChannelIORead(ulAreaNumber, ulOffset, ulDataLen, ref pvData);
        //            if (lret != 0)
        //            {
        //                //logger.ErrorFormat("ReadData Error:{0}", cifXBase.SetLastError(lret));
        //            }

        //            foreach (byte sByte in pvData)
        //            {
        //                char[] charArray = Convert.ToString(sByte, 2).PadLeft(8, '0').ToArray();
        //                Array.Reverse(charArray);
        //                data.Add(charArray);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex.ToString());
        //    }

        //    return data;
        //}

        /// <summary>
        /// 데이터 쓰기
        /// </summary>
        /// <param name="ulAreaNumber"></param>
        /// <param name="ulOffset"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public UInt32 WriteData(UInt32 ulAreaNumber, UInt32 ulOffset, string data)
        {
            UInt32 lret = 9999;

            try
            {
                Thread.Sleep(1);
                bool autoIncrement = false;

                byte[] pvData = cifXBase.CreateOutputData(data, autoIncrement);

                if (pvData.Length > 0)
                {
                    lret = cifXUser.xChannelIOWrite(ulAreaNumber, ulOffset, (UInt32)pvData.Length, ref pvData);
                    if (lret != 0)
                    {
                        //logger.ErrorFormat("WriteData Error:{0}", cifXBase.SetLastError(lret));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }

            return lret;
        }

        /// <summary>
        /// 특정 offset의 특정비트 체크
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private bool IsOn(UInt32 offset, int index)
        {
            return IsOn(offset, 1, index);
        }

        /// <summary>
        /// 특정 offset의 특정비트 체크
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private bool IsOn(UInt32 offset, UInt32 length, int index)
        {
            bool? ret = null;
            List<char[]> data = ReadData(offset, length);
            if (data != null && data.Count > 0)
            {
                ret = data[0][index].Equals('1') ? true : false;
            }

            return (bool)ret;
        }

        private bool? IsTrue(UInt32 offset, int index)
        {
            return IsTrue(offset, 1, index);
        }

        private bool? IsTrue(UInt32 offset, UInt32 length, int index)
        {
            bool? ret = null;
            Tuple<bool, List<char[]>> re = Read(offset, length);
            if (!re.Item1 && re.Item2 != null && re.Item2.Count > 0)
            {
                ret = re.Item2[0][index].Equals('1') ? true : false;
            }

            return ret;
        }

        /// <summary>
        /// bit string to HexString
        /// </summary>
        /// <param name="bitStr"></param>
        /// <returns></returns>
        private static string BitStrToHex(string bitStr)
        {
            string output = string.Empty;

            char[] arr = bitStr.ToCharArray();
            Array.Reverse(arr);
            bitStr = new string(arr);

            int rest = bitStr.Length % 4;
            bitStr = bitStr.PadLeft(rest, '0');


            for (int i = 0; i <= bitStr.Length - 4; i += 4)
            {
                output += string.Format("{0:X}", Convert.ToByte(bitStr.Substring(i, 4), 2));
            }

            return output;
        }

        /// <summary>
        /// 프로그램 On/Off
        /// </summary>
        /// <param name="robot"></param>
        /// <returns></returns>
        public UInt32 SetProgram(Robots robot)
        {
            return WriteData(0, (uint)robot * WriteSize + 4, BitStrToHex("10000000"));
        }

        /// <summary>
        /// 프로그램 set (All Robots)
        /// </summary>
        /// <returns></returns>
        public bool SetProgram()
        {
            uint ret = 0;

            ret = SetProgram(Robots.R1);
            if (ret == 0) ret = SetProgram(Robots.R2);
            if (ret == 0) ret = SetProgram(Robots.R3);

            return ret == 0 ? true : false;
        }

        /// <summary>
        /// 로봇 모터 ON
        /// </summary>
        /// <returns></returns>
        public UInt32 MotorOn(Robots robot)
        {
            return WriteData(0, (uint)robot * WriteSize, CmdMotorOn);
        }

        /// <summary>
        /// 로봇 모터 ON
        /// </summary>
        /// <returns></returns>
        public bool MotorOn()
        {
            uint ret = 0;

            ret = MotorOn(Robots.R1);
            if (ret == 0) ret = MotorOn(Robots.R2);
            if (ret == 0) ret = MotorOn(Robots.R3);

            return ret == 0 ? true : false;
        }

        /// <summary>
        /// 로봇기동
        /// </summary>
        /// <returns></returns>
        public UInt32 RobotRun(Robots robot)
        {
            return WriteData(0, (uint)robot * WriteSize, CmdRobotRun);
        }

        /// <summary>
        /// 로봇기동 (All Robots)
        /// </summary>
        /// <returns></returns>
        public bool RobotRun()
        {
            uint ret = 0;

            ret = RobotRun(Robots.R1);
            if (ret == 0) ret = RobotRun(Robots.R2);
            if (ret == 0) ret = RobotRun(Robots.R3);

            return ret == 0 ? true : false;
        }

        public UInt32 RobotRunReset(Robots robot)
        {
            return WriteData(0, (uint)robot * WriteSize, CmdRobotRunReset);
        }

        public bool RobotRunReset()
        {
            uint ret = 0;

            ret = RobotRunReset(Robots.R1);
            if (ret == 0) ret = RobotRunReset(Robots.R2);
            if (ret == 0) ret = RobotRunReset(Robots.R3);

            return ret == 0 ? true : false;
        }

        /// <summary>
        /// 로봇일시정지
        /// </summary>
        /// <returns></returns>
        public UInt32 RobotPause(Robots robot)
        {
            return WriteData(0, (uint)robot * WriteSize, CmdRobotPause);
        }

        /// <summary>
        /// 로봇일시정지
        /// </summary>
        /// <returns></returns>
        public bool RobotPause()
        {
            uint ret = 0;

            ret = RobotPause(Robots.R1);
            if (ret == 0) ret = RobotPause(Robots.R2);
            if (ret == 0) ret = RobotPause(Robots.R3);

            return ret == 0 ? true : false;
        }

        /// <summary>
        /// 이상해제
        /// </summary>
        /// <returns></returns>
        public UInt32 AlarmRelease(Robots robot)
        {
            return WriteData(0, (uint)robot * WriteSize, CmdAlarmRelease);
        }

        /// <summary>
        /// 이상해제
        /// </summary>
        /// <returns></returns>
        public bool AlarmRelease()
        {
            uint ret = 0;

            ret = AlarmRelease(Robots.R1);
            if (ret == 0) ret = AlarmRelease(Robots.R2);
            if (ret == 0) ret = AlarmRelease(Robots.R3);

            return ret == 0 ? true : false;
        }

        /// <summary>
        /// 전체 읽기 블럭을 모두 읽어온다.
        /// </summary>
        /// <returns></returns>
        public Tuple<bool, List<char[]>> GetRobotData()
        {
            return Read(0, 48);
        }

        /// <summary>
        /// 리셋
        /// </summary>
        /// <returns></returns>
        public UInt32 Reset(Robots robot)
        {
            return WriteData(0, (uint)robot * WriteSize, CmdReset);
        }

        /// <summary>
        /// 리셋 (All Robots)
        /// </summary>
        /// <returns></returns>
        public bool Reset()
        {
            uint ret = 0;

            ret = Reset(Robots.R1);
            if (ret == 0) ret = Reset(Robots.R2);
            if (ret == 0) ret = Reset(Robots.R3);

            return ret == 0 ? true : false;
        }

        /// <summary>
        /// 작업시작
        /// </summary>
        /// <param name="robot"></param>
        /// <returns></returns>
        public UInt32 OrderStart(Robots robot)
        {
            return WriteData(0, (uint)robot * WriteSize + 1, CmdOrderStart);
        }

        /// <summary>
        /// 작업시작 (All Robots)
        /// </summary>
        /// <returns></returns>
        public bool OrderStart()
        {
            uint ret = 0;

            ret = OrderStart(Robots.R1);
            if (ret == 0) ret = OrderStart(Robots.R2);
            if (ret == 0) ret = OrderStart(Robots.R3);

            return ret == 0 ? true : false;
        }

        /// <summary>
        /// 작업정지
        /// </summary>
        /// <param name="robot"></param>
        /// <returns></returns>
        public UInt32 OrderStop(Robots robot)
        {
            return WriteData(0, (uint)robot * WriteSize + 1, CmdOrderStop);
        }

        /// <summary>
        /// 작업정지
        /// </summary>
        /// <returns></returns>
        public bool OrderStop()
        {
            uint ret = 0;

            ret = OrderStop(Robots.R1);
            if (ret == 0) ret = OrderStop(Robots.R2);
            if (ret == 0) ret = OrderStop(Robots.R3);

            return ret == 0 ? true : false;
        }

        /// <summary>
        /// 홈 포지션
        /// </summary>
        /// <param name="robot"></param>
        /// <returns></returns>
        public UInt32 SetHomePosition(Robots robot)
        {
            return WriteData(0, (uint)robot * WriteSize + 1, CmdSetHomePosition);
        }


        /// <summary>
        /// 모터 램프
        /// </summary>
        /// <param name="robot"></param>
        /// <returns></returns>
        public bool IsMotorOn(Robots robot)
        {
            return IsOn((uint)robot * ReadSize, 0);
        }

        /// <summary>
        /// 로봇 재기동 램프 
        /// </summary>
        /// <param name="robot"></param>
        /// <returns></returns>
        public bool RestartOn(Robots robot)
        {
            return IsOn((uint)robot * ReadSize, 1);
        }

        /// <summary>
        /// 정지
        /// <param name="robot"></param>
        /// <returns></returns>
        public bool Pause(Robots robot)
        {
            return IsOn((uint)robot * ReadSize, 2);
        }

        /// <summary>
        /// 알람
        /// <param name="robot"></param>
        /// <returns></returns>
        public bool Alarm(Robots robot)
        {
            return IsOn((uint)robot * ReadSize, 3);
        }

        /// <summary>
        /// 자동모드
        /// </summary>
        /// <param name="robot"></param>
        /// <returns></returns>
        public bool AutoMode(Robots robot)
        {
            return IsOn((uint)robot * ReadSize, 4);
        }

        /// <summary>
        /// 작동중
        /// <param name="robot"></param>
        /// <returns></returns>
        public bool Running(Robots robot)
        {
            return IsOn((uint)robot * ReadSize + 1, 0);
        }

        /// <summary>
        /// 로봇 명령 대기
        /// </summary>
        /// <param name="robot"></param>
        /// <returns></returns>
        public string ReadyCommand(Robots robot)
        {
            return ReadData(0, (uint)robot * ReadSize + 1, 1);

            //return IsOn((uint)robot * ReadSize + 1, 1);
        }

        /// <summary>
        /// 자동 모드 확인 offset R1:0, R2:16, R3:32 => Forth bit 
        /// 명령 대기 확인 
        /// 스탭 실행 확인
        /// Address offset R1:1, R2:17, R3:33
        /// </summary> 
        /// <returns></returns>
        public bool IsReadyCommand()
        {
            var ready = false;

            Tuple<bool, List<char[]>> ret = Read(0, 48);
            if (ret != null && !ret.Item1 && ret.Item2.Count == 48)
            {
                // 모든 로봇이 Automode 인지 확인 하고
                // 로봇 명령대기가 살아있는 경우 스탭 실행이 죽어 있는 경우
                if (ret.Item2[0][4] == '1'
                    && ret.Item2[16][4] == '1'
                    && ret.Item2[32][4] == '1')
                {
                    //string status = new string(ret.Item2[1]);
                    //if (status.Equals("01000000")) ready = true;
                    //if (ready) ready = ret.Item2[1].SequenceEqual(ret.Item2[17]);
                    //if (ready) ready = ret.Item2[1].SequenceEqual(ret.Item2[33]);            
                    ready = (ret.Item2[1][0] == '1' && ret.Item2[1][1] == '1' && ret.Item2[1][2] == '0') ? true : false;
                    if (ready) ready = (ret.Item2[17][0] == '1' && ret.Item2[17][1] == '1' && ret.Item2[17][2] == '0') ? true : false;
                    if (ready) ready = (ret.Item2[33][0] == '1' && ret.Item2[33][1] == '1' && ret.Item2[33][2] == '0') ? true : false;
                }
            }

            return ready;
        }

        /// <summary>
        /// 현재 최종 스탭을 가져온다.
        /// </summary>
        /// <returns></returns>
        public Tuple<bool, int[]> GetCurrentStep()
        {
            bool ret = false;

            int[] steps = new int[3];

            Tuple<bool, List<char[]>> data = Read(0, 48);
            if (data != null && !data.Item1 && data.Item2.Count == 48)
            {
                try
                {
                    char[] ar = data.Item2[6];
                    Array.Reverse(ar);
                    steps[0] = Convert.ToInt32(new string(ar), 2);

                    ar = data.Item2[22];
                    Array.Reverse(ar);
                    steps[1] = Convert.ToInt32(new string(ar), 2);

                    ar = data.Item2[38];
                    Array.Reverse(ar);
                    steps[2] = Convert.ToInt32(new string(ar), 2);
                }
                catch (Exception ex)
                {
                    ret = false;
                }
            }

            return new Tuple<bool, int[]>(ret, steps);
        }

        /// <summary>
        /// 정방향
        /// </summary>
        /// <param name="robot"></param>
        /// <returns></returns>
        public UInt32 SetForward(Robots robot)
        {
            return WriteData(0, (uint)robot * WriteSize + 3, CmdForward);
        }

        /// <summary>
        /// 역방향
        /// </summary>
        /// <param name="robot"></param>
        /// <returns></returns>
        public UInt32 SetBackward(Robots robot)
        {
            return WriteData(0, (uint)robot * WriteSize + 3, CmdBackward);
        }

        /// <summary>
        /// 조립 지그 설정
        /// </summary>
        /// <param name="robot"></param>
        /// <param name="jigSeq"></param>
        /// <returns></returns>
        private UInt32 SetAssyJIG(Robots robot, uint offset, int jigIndex)
        {
            return WriteData(0, (uint)robot * WriteSize + offset, CmdSetAssyJIG[jigIndex]);
        }

        /// <summary>
        /// 조립 지그 설정
        /// </summary>
        /// <param name="robot"></param>
        /// <param name="jigIndex"></param>
        /// <returns></returns>
        public UInt32 SetAssyJIG(Robots robot, int jigIndex)
        {
            int idx = 0;
            uint ret = 9999;

            while(true)
            {
                if (idx > 10) break;

                ret = SetAssyJIG(robot, 2, jigIndex);
                if (ret == 0) break;
                idx++;
            }

            return ret;
        }

        /// <summary>
        /// 조립 지그 설정 (All Robots)
        /// </summary>
        /// <param name="jigIndex"></param>
        /// <returns></returns>
        public bool SetAssyJIG(int jigIndex)
        {
            uint ret = 0;

            ret = SetAssyJIG(Robots.R1, jigIndex);
            if (ret == 0) ret = SetAssyJIG(Robots.R2, jigIndex);
            if (ret == 0) ret = SetAssyJIG(Robots.R3, jigIndex);

            return ret == 0 ? true : false;
        }

        /// <summary>
        /// 특정 순서의 로봇에 스탭 명령을 전송한다.
        /// </summary>
        /// <param name="robot">로봇순서[R1, R2, R3]</param>
        /// <param name="step"></param>
        /// <returns></returns>
        private UInt32 SendOrder(Robots robot, uint offset, int step)
        {
            return WriteData(0, (uint)robot * WriteSize + offset, CmdOrderSteps[step]);
        }

        /// <summary>
        /// 특정 순서의 로봇에 스탭 명령을 전송한다.
        /// </summary>
        /// <param name="robot"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public UInt32 SendOrder(Robots robot, int step)
        {
            int idx = 0;
            uint ret = 9999;
            while(true)
            {
                if (idx > 10) break;

                ret = SendOrder(robot, 8, step);
                if (ret == 0) break;
                idx++;
            }
            return ret;
        }

        /// <summary>
        /// 모든 로봇에 해당 스탭을 전송한다.
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        public bool SendOrder(int step)
        {
            uint ret = 0;

            ret = SendOrder(Robots.R1, step);
            if (ret == 0) ret = SendOrder(Robots.R2, step);
            if (ret == 0) ret = SendOrder(Robots.R3, step);

            return ret == 0 ? true : false;
        }

        /// <summary>
        /// 특정 순서의 로봇의 스탭이 램프 확인
        /// </summary>
        /// <param name="robot">로봇순서[R1, R2, R3]</param>
        /// <param name="step">스탭 순번</param>
        /// <returns></returns>
        public bool GetCommandLamp(Robots robot, int step)
        {
            uint offset = 0;

            if (step / 8 == 1)
            {
                offset = 1;
                step = step - 8;
            }

            return IsOn(((uint)robot * ReadSize) + offset, step);
        }

        /// <summary>
        /// 해당 로봇의 현재 스탭을 가져온다.
        /// </summary>
        /// <param name="robot"></param>
        /// <returns></returns>
        public int GetCurrentStep(Robots robot)
        {
            int ret = -1;

            List<Char[]> stepList = ReadData((uint)robot * ReadSize + 6, 2);
            
            if (stepList != null && stepList.Count == 2)
            {
                for (int i = 1; i <= stepList[0].Length; i++)
                {
                    if (stepList[0][i] == '1')
                    {
                        ret = i;
                    }
                }

                for (int i = 1; i <= stepList[1].Length; i++)
                {
                    if (stepList[1][i] == '1')
                    {
                        ret = i + 8;
                    }
                }

                if (ret == -1) ret = 0;
            }


            return ret;
        }

        /// <summary>
        /// 현재 설정된 지그 번호를 가져온다.
        /// </summary>
        /// <param name="robot"></param>
        /// <returns></returns>
        public int GetCurrentJig(Robots robot)
        {
            int ret = 0;

            List<Char[]> jigNumber = ReadData((uint)robot * ReadSize + 2, 1);

            if (jigNumber != null && jigNumber.Count == 1)
            {
                for (int i = 0; i < jigNumber[0].Length; i++)
                {
                    if (jigNumber[0][i] == '1')
                    {
                        ret = i + 1;
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// 현재 설정된 조립/해체 인지 가져온다.
        /// </summary>
        /// <param name="robot"></param>
        /// <returns></returns>
        public Direction GetDirection(Robots robot)
        {
            if (!IsOn((uint)robot * ReadSize + 3, 0))
                return Direction.Forward;
            else
                return Direction.Backward;
        }

        /// <summary>
        /// 발생된 트리거 번호를 리턴한다.
        /// </summary>
        /// <returns></returns>
        public int GetTriggerSignal()
        {
            int triggerIndex = 0;

            List<char[]> trigger = ReadData(43, 1);
            if (trigger != null && trigger.Count == 1)
            {
                try
                {
                    char[] data = trigger[0];
                    Array.Reverse(data);
                    triggerIndex = Convert.ToInt32(new string(data), 2);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                }
            }
            return triggerIndex;
        }

        /// <summary>
        /// 트리거와 트리거 사이 딜레이 타임 설정
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public UInt32 SetTriggerInterval(int interval)
        {
            char[] arr = Convert.ToString(interval, 2).ToCharArray();
            Array.Reverse(arr);
            string data = new string(arr);
            data = data.PadRight(8, '0');

            return WriteData(0, 38, BitStrToHex(data));
        }

        /// <summary>
        /// 인터락 발생 여부 확인
        /// </summary>
        /// <param name="robot"></param>
        /// <returns></returns>
        public bool IsInterLock(Robots robot)
        {
            return IsOn((uint)robot * ReadSize + 5, 0);
        }

        /// <summary>
        /// 인터락 발생 시키기
        /// </summary>
        /// <param name="robot"></param>
        /// <returns></returns>
        public UInt32 RaiseInterLock(Robots robot, bool isOn)
        {
            return WriteData(0, (uint)robot * WriteSize + 5, isOn ? BitStrToHex("10000000") : BitStrToHex("00000000"));
        }


        public Tuple<bool, List<char[]>> ReadBlock(uint offset, uint len)
        {
            return Read(offset, len);
        }
    }
}
