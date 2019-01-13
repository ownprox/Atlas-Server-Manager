using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace AtlasServerManager.Includes
{
    class ServerStatus
    {
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetPhysicallyInstalledSystemMemory(out long TotalMemoryInKilobytes);
        private static long TotalMemA;
        private static double TotalMem;

        public static void UpdateStatus(AtlasServerManager AtlasMgr, CancellationToken token)
        {
            PerformanceCounter cpu = new PerformanceCounter("Processor", "% Processor Time", "_Total"), MemA = new PerformanceCounter("Memory", "Available KBytes");
            int Players = 0, TotalPlayers = 0;
            double AMem = 0;
            GetPhysicallyInstalledSystemMemory(out TotalMemA);
            TotalMem = TotalMemA / 1048576;
            while (true)
            {
                if (token.IsCancellationRequested) break;
                try
                {
                    cpu.NextValue();
                    AtlasMgr.Invoke((MethodInvoker)delegate ()
                    {
                        if (AtlasMgr.ServerList.Items.Count > 0)
                        {
                            foreach (ArkServerListViewItem ASD in AtlasMgr.ServerList.Items)
                                if (ASD.GetServerData().IsRunning())
                                    ASD.GetServerData().GetPlayersOnline(AtlasMgr, ASD);
                        }
                    });

                    Thread.Sleep(3000);
                    AMem = (TotalMemA - MemA.NextValue()) / 1048576;
                    AtlasMgr.Invoke((MethodInvoker)delegate ()
                    {
                        if (AtlasMgr.ServerList.Items.Count > 0)
                            foreach (ArkServerListViewItem ASD in AtlasMgr.ServerList.Items)
                            {
                                int.TryParse(ASD.SubItems[5].Text, out Players);
                                TotalPlayers += Players;
                                ASD.UpdateStatus();
                            }
                    });
                    AtlasMgr.Text = AtlasMgr.ASMTitle + " | CPU: " + (int)cpu.NextValue() + "%, Mem: " + AMem.ToString("#.##") + " GB / " + TotalMem + " GB - Players Online: " + TotalPlayers;
                    TotalPlayers = 0;
                    Thread.Sleep(2000);
                }
                catch
                {
                }
            }
        }
    }
}