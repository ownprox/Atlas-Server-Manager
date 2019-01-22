using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace AtlasServerManager.Includes
{
    public class AtlasServerData
    {
        public SourceRcon RconConnection = new SourceRcon();
        public Process ServerProcess;
        public string Pass = "", CustomArgs = "", CustomAfterArgs = "", ServerPath = "", FinalServerPath = "", AltSaveDirectory = "", ServerIp = "", RCONIP = "";
        public int ServerPort, QueryPort, RconPort, MaxPlayers, ReservedPlayers, ServerX, ServerY, PID = 0, ProcessPriority;
        public bool[] ProcessAffinity;
        public bool Rcon, FTD, WildWipe, PVP, MapB, Gamma, Third, Crosshair, HitMarker, Imprint, Loaded, AutoStart, Upnp, BattleEye;
        private bool HasMadeFirstContact, AttemptRconSave = false, GamePortWasOpen = false;
        private DateTime LastSourceQueryReply, RconSavedEstimate;

        public void StartServer()
        {
            HasMadeFirstContact = false;
            string ExePath = ServerPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Replace("/", @"\");
            if (ExePath.StartsWith("./") || ExePath.StartsWith(@".\"))
            {
                string tempEndDir = ExePath.Replace("./", "").Replace(@".\", "");
                ExePath = Path.GetDirectoryName(Application.ExecutablePath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Replace("/", @"\") + Path.DirectorySeparatorChar;
                if (tempEndDir.Length > 0) ExePath = Path.Combine(ExePath, tempEndDir);
            }

            if (!ExePath.Contains(".") || !File.Exists(ExePath))
            {
                if(File.Exists(Path.Combine(ExePath + Path.DirectorySeparatorChar, "ShooterGameServer.exe")))
                    ExePath = Path.Combine(ExePath + Path.DirectorySeparatorChar, @"ShooterGameServer.exe");
                else if (File.Exists(Path.Combine(ExePath + Path.DirectorySeparatorChar, @"ShooterGame\Binaries\Win64\ShooterGameServer.exe")))
                    ExePath = Path.Combine(ExePath + Path.DirectorySeparatorChar, @"ShooterGame\Binaries\Win64\ShooterGameServer.exe");
                else if(File.Exists(Path.Combine(ExePath + Path.DirectorySeparatorChar, @"Binaries\Win64\ShooterGameServer.exe")))
                    ExePath = Path.Combine(ExePath + Path.DirectorySeparatorChar, @"Binaries\Win64\ShooterGameServer.exe");
                else if (File.Exists(Path.Combine(ExePath + Path.DirectorySeparatorChar, @"Win64\ShooterGameServer.exe")))
                    ExePath = Path.Combine(ExePath + Path.DirectorySeparatorChar, @"Win64\ShooterGameServer.exe");
            }

            if (!File.Exists(ExePath))
            {
                MessageBox.Show(ExePath + " Is Not found!!!", "ShooterGameServer.exe Not Found!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (IsRunning()) StopServer();
            ServerPath = ExePath.Substring(0, ExePath.IndexOf(@"\ShooterGame"));
            GamePortWasOpen = false;

            Backup.RestoreConfigs(AtlasServerManager.GetInstance(), ServerPath);

            /* Resolve DNS */
            string CurIP = ServerIp;
            if (CurIP != string.Empty)
            {
                int DotCount = 0;
                for (int i = 0; i < CurIP.Length; i++) if (CurIP[i] == '.') DotCount++;
                if (DotCount != 3)
                {
                    System.Net.IPAddress[] ips = System.Net.Dns.GetHostAddresses(CurIP);
                    if (ips.Length > 0)
                    {
                        CurIP = ips[0].ToString();
                        try
                        {
                            string ServerGrid = Path.Combine(ServerPath + Path.DirectorySeparatorChar, @"ShooterGame\ServerGrid.json");
                            if (File.Exists(ServerGrid))
                            {
                                string OwnProxGrid = Path.Combine(ServerPath + Path.DirectorySeparatorChar, @"ShooterGame\ServerGridOwnProx.json");
                                if (File.Exists(OwnProxGrid)) File.Delete(OwnProxGrid);
                                using (StreamWriter sw = new StreamWriter(OwnProxGrid))
                                using (StreamReader sr = new StreamReader(ServerGrid))
                                {
                                    string line = "";
                                    while ((line = sr.ReadLine()) != null)
                                    {
                                        if (line.Contains("\"ip\": \"")) sw.WriteLine("      \"ip\": \"" + CurIP + "\",");
                                        else sw.WriteLine(line);
                                    }
                                }
                                File.Delete(ServerGrid);
                                File.Move(OwnProxGrid, ServerGrid);
                            }
                        }
                        catch (Exception e) { MessageBox.Show("Error: " + e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                    }
                }
            }

            if(Upnp)
            {
                UPNP.AddUPNPServer(ServerPort, QueryPort, AltSaveDirectory);
            }
            
            try
            {
                ServerProcess = new Process
                {
                    StartInfo = new ProcessStartInfo(ExePath, "\"" + "Ocean?ServerX=" + ServerX + "?ServerY=" + ServerY + "?Port=" + ServerPort + "?QueryPort=" + QueryPort + "?AltSaveDirectoryName=" + AltSaveDirectory + "?MaxPlayers=" + MaxPlayers + "?ReservedPlayerSlots=" + ReservedPlayers + "?ServerAdminPassword=" + Pass + "?ServerCrosshair=" + (Crosshair ? "true" : "false") + "?AllowThirdPersonPlayer=" + (Third ? "true" : "false") + "?MapPlayerLocation=" + (MapB ? "true" : "false") + "?serverPVE=" + (!PVP ? "true" : "false") + "?RCONEnabled=" + (Rcon ? ("true?RCONPort=" + RconPort) : "false") + "?EnablePvPGamma=" + (Gamma ? "true" : "false") + "?AllowAnyoneBabyImprintCuddle=" + (Imprint ? "true" : "false") + "?ShowFloatingDamageText=" + FTD + (CurIP == string.Empty ? "" : "?SeamlessIP=" + CurIP) + CustomArgs + "\" -game -server -log -NoCrashDialog" + (BattleEye ? "" : " -NoBattlEye") + (CustomAfterArgs == string.Empty ? "" : " " + CustomAfterArgs))
                    {
                        UseShellExecute = false,
                        WorkingDirectory = Path.GetDirectoryName(ExePath)
                    }
                };

                ServerProcess.Start();
                int AffinityMask = 0;
                for (int i = 0; i < ProcessAffinity.Length; i++) AffinityMask |= (ProcessAffinity[i] ? 1 : 0) << i;
                ServerProcess.ProcessorAffinity = (System.IntPtr)AffinityMask;
                switch(ProcessPriority)
                {
                    case 1:
                        ServerProcess.PriorityClass = ProcessPriorityClass.AboveNormal;
                        break;
                    case 2:
                        ServerProcess.PriorityClass = ProcessPriorityClass.High;
                        break;
                    case 3:
                        ServerProcess.PriorityClass = ProcessPriorityClass.RealTime;
                        break;
                    default:
                        ServerProcess.PriorityClass = ProcessPriorityClass.Normal;
                        break;

                }
                PID = ServerProcess.Id;
            }
            catch (Exception e)
            {
                ServerProcess = null;
                MessageBox.Show(e.Message + ": " + ExePath, "Error");
            }
        }

        public void GetPlayersOnline(AtlasServerManager AtlasMgr, ArkServerListViewItem server)
        {
            new SourceQuery(ServerIp, QueryPort, (SourceQuery.ServerData SrvData) =>
            {
                AtlasMgr.Invoke((MethodInvoker)delegate ()
                {
                    if (server != null && SrvData.Success)
                    {
                        server.SubItems[5].Text = SrvData.Players.ToString();
                        HasMadeFirstContact = true;
                        LastSourceQueryReply = DateTime.Now;
                    }
                    SrvData.sourceQuery.Dispose();
                });
            });
        }

        private bool CheckGamePort()
        {
            bool IsConnected = false;
            try
            {
                System.Net.Sockets.Socket sock = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp);
                sock.Connect(ServerIp, ServerPort);
                IsConnected = sock.Connected;
                sock.Shutdown(System.Net.Sockets.SocketShutdown.Both);
                sock.Close();
            } catch {}
            if (!GamePortWasOpen && IsConnected) GamePortWasOpen = true;
            return IsConnected;
        }

        public int IsRunning(AtlasServerManager AtlasMgr)
        {
            if (ServerProcess == null || ServerProcess.HasExited || ServerProcess.Id == 0) return 0;
            if (AttemptRconSave && DateTime.Now.Subtract(RconSavedEstimate).TotalSeconds <= 0) return 1;
            if (HasMadeFirstContact && AtlasMgr != null)
            {
                if (AtlasMgr.QueryPortCheck.Checked && DateTime.Now.Subtract(LastSourceQueryReply).TotalSeconds > 60) return 2;
                bool bWasGamePortOpen = GamePortWasOpen;
                if (AtlasMgr.GamePortCheck.Checked && !CheckGamePort() && bWasGamePortOpen)
                {
                    if(!AttemptRconSave)
                    {
                        AttemptRconSave = true;
                        SourceRconTools.SendCommand("DoExit", this);
                        RconSavedEstimate = DateTime.Now.AddMinutes(1);
                        return 4;
                    }
                    AttemptRconSave = false;
                    return 3;
                }
            }
            return 1;
        }

        public bool IsRunning() { return (ServerProcess == null || ServerProcess.HasExited || ServerProcess.Id == 0) ? false : true; }

        public void StopServer()
        {
            HasMadeFirstContact = false;
            if (Upnp)
            {
                UPNP.RemoveUPNPServer(ServerPort, QueryPort);
            }

            if (IsRunning())
            {
                ServerProcess.Kill();
                ServerProcess = null;
            }
        }
    }
}