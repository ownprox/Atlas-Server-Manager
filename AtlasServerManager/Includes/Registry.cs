using System;
using System.Diagnostics;

namespace AtlasServerManager.Includes
{
    public class Registry
    {
        private static Microsoft.Win32.RegistryKey key;

        public static void LoadRegConfig(AtlasServerManager AtlasMgr)
        {
            try
            {
                key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\AtlasServerManager", true);
                if (key == null) key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("SOFTWARE\\AtlasServerManager");

                if (key != null)
                {
                    for (int i = 0; i < AtlasMgr.ServerList.Columns.Count; i++)
                    {
                        AtlasMgr.ServerList.Columns[i].DisplayIndex = (int)key.GetValue("ColOrder" + i, i);
                        AtlasMgr.ServerList.Columns[i].Width = (int)key.GetValue("ColWidth" + i, AtlasMgr.ServerList.Columns[i].Width);
                    }
                    /* BOOL */
                    AtlasMgr.checkAutoServerUpdate.Checked = (int)key.GetValue("AutoServerUpdate", 1) == 1 ? true : false;
                    AtlasMgr.BootWhenOffCheck.Checked = (int)key.GetValue("BootWhenOff", 1) == 1 ? true : false;
                    AtlasMgr.QueryPortCheck.Checked = (int)key.GetValue("QueryPortCheck", 1) == 1 ? true : false;
                    AtlasMgr.GamePortCheck.Checked = (int)key.GetValue("GamePortCheck", 1) == 1 ? true : false;

                    /* DECIMAL */
                    decimal value = 1.0M;
                    decimal.TryParse((string)key.GetValue("ServerUpdate", AtlasMgr.numServerUpdate.Value.ToString()), out value);
                    AtlasMgr.numServerUpdate.Value = value;
                    decimal.TryParse((string)key.GetValue("ServerWarning", AtlasMgr.numServerWarning.Value.ToString()), out value);
                    AtlasMgr.numServerWarning.Value = value;
                    decimal.TryParse((string)key.GetValue("ServerMonitor", AtlasMgr.numServerMonitor.Value.ToString()), out value);
                    AtlasMgr.numServerMonitor.Value = value;

                    /* STRING */
                    AtlasMgr.ServerPath = (string)key.GetValue("ServerDataPath", string.Empty);
                    AtlasMgr.ServerUpdateMessage.Text = (string)key.GetValue("ServerUpdateMessage", AtlasMgr.ServerUpdateMessage.Text);
                    AtlasMgr.ServerUpdatingMessage.Text = (string)key.GetValue("ServerUpdatingMessage", AtlasMgr.ServerUpdatingMessage.Text);

                    LoadRegServers(AtlasMgr);
                }
            }
            catch (Exception e) { System.Windows.Forms.MessageBox.Show("Failed To Load Setting: " + e.StackTrace); }
        }

        public static void SaveRegConfig(AtlasServerManager AtlasMgr)
        {
            try
            {
                key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\AtlasServerManager", true);
                if (key != null)
                {
                    for (int i = 0; i < AtlasMgr.ServerList.Columns.Count; i++)
                    {
                        key.SetValue("ColOrder" + i, AtlasMgr.ServerList.Columns[i].DisplayIndex, Microsoft.Win32.RegistryValueKind.DWord);
                        key.SetValue("ColWidth" + i, AtlasMgr.ServerList.Columns[i].Width, Microsoft.Win32.RegistryValueKind.DWord);
                    }
                    /* BOOL */
                    key.SetValue("AutoServerUpdate", AtlasMgr.checkAutoServerUpdate.Checked ? 1 : 0, Microsoft.Win32.RegistryValueKind.DWord);
                    key.SetValue("BootWhenOff", AtlasMgr.BootWhenOffCheck.Checked ? 1 : 0, Microsoft.Win32.RegistryValueKind.DWord);
                    key.SetValue("QueryPortCheck", AtlasMgr.QueryPortCheck.Checked ? 1 : 0, Microsoft.Win32.RegistryValueKind.DWord);
                    key.SetValue("GamePortCheck", AtlasMgr.GamePortCheck.Checked ? 1 : 0, Microsoft.Win32.RegistryValueKind.DWord);

                    /* DECIMAL */
                    key.SetValue("ServerUpdate", AtlasMgr.numServerUpdate.Value.ToString(), Microsoft.Win32.RegistryValueKind.String);
                    key.SetValue("ServerWarning", AtlasMgr.numServerWarning.Value.ToString(), Microsoft.Win32.RegistryValueKind.String);
                    key.SetValue("ServerMonitor", AtlasMgr.numServerMonitor.Value.ToString(), Microsoft.Win32.RegistryValueKind.String);

                    /* STRING */
                    key.SetValue("ServerDataPath", AtlasMgr.ServerPath, Microsoft.Win32.RegistryValueKind.String);
                    key.SetValue("ServerUpdateMessage", AtlasMgr.ServerUpdateMessage.Text, Microsoft.Win32.RegistryValueKind.String);
                    key.SetValue("ServerUpdatingMessage", AtlasMgr.ServerUpdatingMessage.Text, Microsoft.Win32.RegistryValueKind.String);

                    SaveRegServers(AtlasMgr);
                    key.Close();
                }
            }
            catch (Exception e) { System.Windows.Forms.MessageBox.Show("Failed To Save Setting: " + e.Message); }
        }

        public static void DeleteServer(int Index)
        {
            try
            {
                key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\AtlasServerManager\\Servers", true);
                if (key != null) key.DeleteSubKey("Server" + Index);
            }
            catch (Exception e) { System.Windows.Forms.MessageBox.Show("Failed To Delete Server Setting: " + e.Message); }
        }

        private static void LoadRegServers(AtlasServerManager AtlasMgr)
        {
            key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\AtlasServerManager\\Servers");
            if (key != null)
            {
                string[] Servers = key.GetSubKeyNames();
                foreach (string Srv in Servers)
                    if (Srv.StartsWith("Server"))
                    {
                        AtlasServerData ASD = LoadRegServer(Srv);
                        try
                        {
                            if (ASD.PID != 0) ASD.ServerProcess = Process.GetProcessById(ASD.PID);
                        }
                        catch { ASD.PID = 0; }
                        AtlasMgr.ServerList.Items.Add(new ArkServerListViewItem(ASD));
                    }
            }
        }

        public static AtlasServerData LoadRegServer(string Srv)
        {
            AtlasServerData Asd = new AtlasServerData();
            key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\AtlasServerManager\\Servers\\" + Srv);
            if (key != null)
            {
                /* BOOL */
                Asd.Rcon = (int)key.GetValue("Rcon", Asd.Rcon ? 1 : 0) == 1 ? true : false;
                Asd.WildWipe = (int)key.GetValue("WildWipe", Asd.WildWipe ? 1 : 0) == 1 ? true : false;
                Asd.PVP = (int)key.GetValue("PVP", Asd.PVP ? 1 : 0) == 1 ? true : false;
                Asd.MapB = (int)key.GetValue("MapB", Asd.MapB ? 1 : 0) == 1 ? true : false;
                Asd.Gamma = (int)key.GetValue("Gamma", Asd.Gamma ? 1 : 0) == 1 ? true : false;
                Asd.Third = (int)key.GetValue("Third", Asd.Third ? 1 : 0) == 1 ? true : false;
                Asd.Crosshair = (int)key.GetValue("Crosshair", Asd.Crosshair ? 1 : 0) == 1 ? true : false;
                Asd.HitMarker = (int)key.GetValue("HitMarker", Asd.HitMarker ? 1 : 0) == 1 ? true : false;
                Asd.Imprint = (int)key.GetValue("Imprint", Asd.Imprint ? 1 : 0) == 1 ? true : false;
                Asd.FTD = (int)key.GetValue("FTD", Asd.FTD ? 1 : 0) == 1 ? true : false;
                Asd.Upnp = (int)key.GetValue("UPNP", Asd.Upnp ? 1 : 0) == 1 ? true : false;

                Asd.ProcessAffinity = new bool[Environment.ProcessorCount];
                for (int i = 0; i < Environment.ProcessorCount; i++)
                {
                    Asd.ProcessAffinity[i] = (int)key.GetValue("ProcessAffinity" + i, 1) == 1 ? true : false;
                }

                /* INT */
                Asd.ServerPort = (int)key.GetValue("ServerPort", Asd.ServerPort);
                Asd.QueryPort = (int)key.GetValue("QueryPort", Asd.QueryPort);
                Asd.RconPort = (int)key.GetValue("RconPort", Asd.RconPort);
                Asd.MaxPlayers = (int)key.GetValue("MaxPlayers", Asd.MaxPlayers);
                Asd.ReservedPlayers = (int)key.GetValue("ReservedPlayers", Asd.ReservedPlayers);
                Asd.ServerX = (int)key.GetValue("ServerX", Asd.ServerX);
                Asd.ServerY = (int)key.GetValue("ServerY", Asd.ServerY);
                Asd.PID = (int)key.GetValue("PID", Asd.PID);
                Asd.ProcessPriority = (int)key.GetValue("ProcessPriority", 0);

                /* STRING */
                Asd.Pass = (string)key.GetValue("Pass", Asd.Pass);
                Asd.CustomArgs = (string)key.GetValue("CustomArgs", Asd.CustomArgs);
                Asd.ServerPath = (string)key.GetValue("ServerPath", Asd.ServerPath);
                Asd.AltSaveDirectory = (string)key.GetValue("AltSaveDirectory", Asd.AltSaveDirectory);
                Asd.ServerIp = (string)key.GetValue("ServerIp", Asd.ServerIp);
                Asd.RCONIP = (string)key.GetValue("RconIP", Asd.RCONIP);
                Asd.Loaded = true;
            } else Asd.Loaded = false;
            return Asd;
        }

        private static void SaveRegServers(AtlasServerManager AtlasMgr)
        {
            key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\AtlasServerManager\\Servers", true);
            if (key == null) key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("SOFTWARE\\AtlasServerManager\\Servers");
            if (key != null)
            {
                int ActIndex = 0, CurActIndex;
                foreach (AtlasServerData Asd in AtlasMgr.ServerList.GetServerList()) if (SaveRegServer(Asd, ActIndex)) ActIndex++;
                foreach (string s in key.GetSubKeyNames()) if ((s.Contains("Server")) && int.TryParse(s.Replace("Server", ""), out CurActIndex) && CurActIndex > ActIndex) key.DeleteSubKey("Server" + CurActIndex);
                key.Close();
            }
        }

        public static bool SaveRegServer(AtlasServerData Asd, int ActIndex, bool DefaultServerSave = false, bool SaveLastOverride = false)
        {
            key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\AtlasServerManager\\Servers\\" + (DefaultServerSave ? (SaveLastOverride ? "LastSaved" : "Default") : "Server" + ActIndex), true);
            if (key == null) key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("SOFTWARE\\AtlasServerManager\\Servers\\" + (DefaultServerSave ? (SaveLastOverride ? "LastSaved" : "Default") : "Server" + ActIndex));
            else if (DefaultServerSave && !SaveLastOverride) return true;
            if (key != null)
            {
                /* BOOL */
                key.SetValue("Rcon", Asd.Rcon ? 1 : 0, Microsoft.Win32.RegistryValueKind.DWord);
                key.SetValue("WildWipe", Asd.WildWipe ? 1 : 0, Microsoft.Win32.RegistryValueKind.DWord);
                key.SetValue("PVP", Asd.PVP ? 1 : 0, Microsoft.Win32.RegistryValueKind.DWord);
                key.SetValue("MapB", Asd.MapB ? 1 : 0, Microsoft.Win32.RegistryValueKind.DWord);
                key.SetValue("Gamma", Asd.Gamma ? 1 : 0, Microsoft.Win32.RegistryValueKind.DWord);
                key.SetValue("Third", Asd.Third ? 1 : 0, Microsoft.Win32.RegistryValueKind.DWord);
                key.SetValue("Crosshair", Asd.Crosshair ? 1 : 0, Microsoft.Win32.RegistryValueKind.DWord);
                key.SetValue("HitMarker", Asd.HitMarker ? 1 : 0, Microsoft.Win32.RegistryValueKind.DWord);
                key.SetValue("Imprint", Asd.Imprint ? 1 : 0, Microsoft.Win32.RegistryValueKind.DWord);
                key.SetValue("FTD", Asd.FTD ? 1 : 0, Microsoft.Win32.RegistryValueKind.DWord);
                key.SetValue("UPNP", Asd.Upnp ? 1 : 0, Microsoft.Win32.RegistryValueKind.DWord);

                if (Asd.ProcessAffinity == null)
                {
                    Asd.ProcessAffinity = new bool[Environment.ProcessorCount];
                    for (int i = 0; i < Environment.ProcessorCount; i++) Asd.ProcessAffinity[i] = true;
                }

                for (int i = 0; i < Environment.ProcessorCount; i++)
                {
                    key.SetValue("ProcessAffinity" + i, Asd.ProcessAffinity[i] ? 1 : 0, Microsoft.Win32.RegistryValueKind.DWord);
                }

                /* INT */
                key.SetValue("ServerPort", Asd.ServerPort, Microsoft.Win32.RegistryValueKind.DWord);
                key.SetValue("QueryPort", Asd.QueryPort, Microsoft.Win32.RegistryValueKind.DWord);
                key.SetValue("RconPort", Asd.RconPort, Microsoft.Win32.RegistryValueKind.DWord);
                key.SetValue("MaxPlayers", Asd.MaxPlayers, Microsoft.Win32.RegistryValueKind.DWord);
                key.SetValue("ReservedPlayers", Asd.ReservedPlayers, Microsoft.Win32.RegistryValueKind.DWord);
                key.SetValue("ServerX", Asd.ServerX, Microsoft.Win32.RegistryValueKind.DWord);
                key.SetValue("ServerY", Asd.ServerY, Microsoft.Win32.RegistryValueKind.DWord);
                key.SetValue("PID", Asd.PID, Microsoft.Win32.RegistryValueKind.DWord);
                key.SetValue("ProcessPriority", Asd.ProcessPriority, Microsoft.Win32.RegistryValueKind.DWord);

                /* STRING */
                key.SetValue("Pass", Asd.Pass, Microsoft.Win32.RegistryValueKind.String);
                key.SetValue("CustomArgs", Asd.CustomArgs, Microsoft.Win32.RegistryValueKind.String);
                key.SetValue("ServerPath", Asd.ServerPath, Microsoft.Win32.RegistryValueKind.String);
                key.SetValue("AltSaveDirectory", Asd.AltSaveDirectory, Microsoft.Win32.RegistryValueKind.String);
                key.SetValue("ServerIp", Asd.ServerIp, Microsoft.Win32.RegistryValueKind.String);
                key.SetValue("RconIP", Asd.RCONIP, Microsoft.Win32.RegistryValueKind.String);
                return true;
            }
            return false;
        }

        public static bool GetText(string name, string text, string TranslateTo, bool FirstTranslation, ref string OutText)
        {
            key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\AtlasServerManager\\Lang\\" + TranslateTo, true);
            if (!FirstTranslation && key == null) key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("SOFTWARE\\AtlasServerManager\\Lang\\" + TranslateTo);

            if (key != null)
            {
                if ((string)key.GetValue(name, "") == string.Empty)
                {
                    key.SetValue(name, text, Microsoft.Win32.RegistryValueKind.String);
                    OutText = text;
                }
                else OutText = (string)key.GetValue(name, "");
                return true;
            }
            else if (TranslateTo == "en")
            {
                key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\AtlasServerManager\\Lang\\Default", true);
                if (key == null) key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("SOFTWARE\\AtlasServerManager\\Lang\\Default");

                if (key != null)
                {
                    OutText = (string)key.GetValue(name, text);
                    return true;
                }
            }
            else
            {
                key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\AtlasServerManager\\Lang\\Default", true);
                if (key == null) key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("SOFTWARE\\AtlasServerManager\\Lang\\Default");

                if (key != null)
                {
                    key.SetValue(name, text, Microsoft.Win32.RegistryValueKind.String);
                    OutText = text;
                    return false;
                }
            }
            OutText = text;
            return false;
        }

        public static void ClearAll()
        {
            key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\AtlasServerManager\\Servers");
            if (key != null)
            {
                string[] Servers = key.GetSubKeyNames();
                foreach (string Srv in Servers)
                    if (Srv.StartsWith("Server")) key.DeleteSubKey(Srv);
                key.Close();
            }
        }
    }
}