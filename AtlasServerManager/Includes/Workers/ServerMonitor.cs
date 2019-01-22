using System.Threading;

namespace AtlasServerManager.Includes
{
    class ServerMonitor
    {
        public static void CheckServerStatus(AtlasServerManager AtlasMgr, CancellationToken token)
        {
            int ServerStatus = 0, SleepTime = 3000;
            bool SavedAfterLaunch = false;
            while (true)
            {
                AtlasMgr.Log("123");
                if (token.IsCancellationRequested) break;
                AtlasMgr.Invoke((System.Windows.Forms.MethodInvoker)delegate ()
                { 
                    if (!ServerUpdater.Updating && AtlasMgr.BootWhenOffCheck.Checked)
                    {
                        foreach (ArkServerListViewItem ASLVI in AtlasMgr.ServerList.Items)
                        {
                            if (ASLVI.Checked && (ServerStatus = ASLVI.GetServerData().IsRunning(AtlasMgr)) != 1)
                            {
                                switch(ServerStatus)
                                {
                                    case 0:
                                        AtlasMgr.Log("Server: " + ASLVI.Text + " Was Offline Booting Now, Process was not running!");
                                        break;
                                    case 2:
                                        AtlasMgr.Log("Server: " + ASLVI.Text + " Was Offline Booting Now, Query Port was not running!");
                                        break;
                                    case 3:
                                        AtlasMgr.Log("Server: " + ASLVI.Text + " Booting Now!");
                                        break;
                                    case 4:
                                        AtlasMgr.Log("Server: " + ASLVI.Text + " Game Port is offline Attempting Rcon Save Please wait 1 minute!");
                                        continue;
                                }
                                ASLVI.GetServerData().StartServer();
                                SavedAfterLaunch = false;
                                if (AtlasMgr.StartupDelayNum.Value > 0) Thread.Sleep((int)AtlasMgr.StartupDelayNum.Value * 1000);
                            }
                            ASLVI.UpdateStatus();
                        }

                        if(!SavedAfterLaunch)
                        {
                            SavedAfterLaunch = true;
                            Registry.SaveRegConfig(AtlasMgr);
                        }
                    }
                    SleepTime = (int)AtlasMgr.numServerMonitor.Value * 1000;
                });
                Thread.Sleep(SleepTime);
            }
        }
    }
}