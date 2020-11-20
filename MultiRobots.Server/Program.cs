using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace MultiRobots.Server
{
    static class Program
    {
        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            int cnt = 0;
            Process[] procs = Process.GetProcesses();
            foreach (Process p in procs)
            {
                Debug.WriteLine(p.ProcessName);
                if (p.ProcessName.Equals("MultiRobots.Server"))
                {
                    cnt++;
                }
            }

            if (cnt > 1)
            {
                MessageBox.Show("이미 실행중 입니다.");
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new frmMain());
            }
        }
    }
}
