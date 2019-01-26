using System;
using System.Windows.Forms;
using AtlasServerManager.Includes;
using System.IO;
using System.Diagnostics;

namespace AtlasServerManager
{
    public partial class AtlasServerManager : Form
    {
        public string SteamPath, ArkManagerPath, ServerPath = string.Empty, ASMTitle;
        public static AtlasServerManager GetInstance() { return instance; }
        public bool FirstDl = false;
        private static AtlasServerManager instance;
        private delegate void RichTextBoxUpdateEventHandler(string txt);
        private InputDialog inputDialog;
        #region Translation
        public void SetLanguage(string language)
        {
            ((ToolStripMenuItem)languageToolStripMenuItem.DropDown.Items.Find(language, false)[0]).Checked = true;
        }

        public string GetLanguage()
        {
            foreach (ToolStripMenuItem dropDownItem in languageToolStripMenuItem.DropDown.Items)
            {
                if (dropDownItem.Checked)
                {
                    return dropDownItem.Name;
                }
            }
            return "en";
        }
         void TranslationHelp(string t, Control.ControlCollection control)
        {
            if (Program.LanguageJObject == null)
            {
                return;
            }
            foreach (Control con in control)
            {
                TranslationHelp(t + con.Name, con);
            }
        }
         void TranslationHelp(string t, object control)
        {
            if (Program.LanguageJObject == null)
            {
                return;
            }
            if (control.GetType() == typeof(MenuStrip))
            {
                t += "-" + ((MenuStrip)control).Name;
                //tojson(t, ((MenuStrip)control).Text);
                ((MenuStrip)control).Text = TranslationGet(t, ((MenuStrip)control).Text);
                if (((MenuStrip)control).Items.Count > 0)
                {
                    foreach (ToolStripMenuItem item in ((MenuStrip)control).Items)
                    {
                        TranslationHelp(t, item);
                    }
                }
            }
            else if (control.GetType() == typeof(ToolStripMenuItem))
            {
                t += "-" + ((ToolStripMenuItem)control).Name;
                //tojson(t, ((ToolStripMenuItem)control).Text);
                ((ToolStripMenuItem)control).Text = TranslationGet(t, ((ToolStripMenuItem)control).Text);
                if (((ToolStripMenuItem)control).DropDownItems.Count > 0)
                {
                    foreach (ToolStripDropDownItem dropDownItem in ((ToolStripMenuItem)control).DropDownItems)
                    {
                        TranslationHelp(t, dropDownItem);
                    }
                }
            }
            else if (control.GetType() == typeof(ToolStripDropDownItem))
            {
                t += "-" + ((ToolStripDropDownItem)control).Name;
                //tojson(t, ((ToolStripDropDownItem)control).Text);
                ((ToolStripDropDownItem)control).Text = TranslationGet(t, ((ToolStripDropDownItem)control).Text);

            }
            else if (control.GetType() == typeof(TabControl))
            {
                t += "-" + ((TabControl)control).Name;
                //tojson(t, ((TabControl)control).Text);
                ((TabControl)control).Text = TranslationGet(t, ((TabControl)control).Text);
                if (((TabControl)control).TabPages.Count > 0)
                {
                    foreach (TabPage tabControl in ((TabControl)control).TabPages)
                    {
                        TranslationHelp(t, tabControl);
                    }
                }
            }
            else if (control.GetType() == typeof(TabPage) || control.GetType() == typeof(GroupBox) || control.GetType() == typeof(SplitContainer) || control.GetType() == typeof(SplitterPanel))
            {
                t += "-" + ((Control)control).Name;
                //tojson(t, ((TabPage)control).Text);
                ((Control)control).Text = TranslationGet(t, ((Control)control).Text);
                if (((Control)control).Controls.Count > 0)
                {
                    foreach (Control control1 in ((Control)control).Controls)
                    {
                        TranslationHelp(t, control1);
                    }
                }
            }
            else if (control.GetType() == typeof(Button) || control.GetType() == typeof(Label) || control.GetType() == typeof(CheckBox) || control.GetType() == typeof(NumericUpDown))
            {
                t += "-" + ((Control)control).Name;
                //tojson(t, ((Control)control).Text);
                ((Control)control).Text = TranslationGet(t, ((Control)control).Text);

            }
            else if (control.GetType() == typeof(ArkListView))
            {
                if (((ArkListView)control).Columns.Count > 0)
                {
                    foreach (ColumnHeader column in ((ArkListView)control).Columns)
                    {
                        t += "-" + column.Name;
                        //tojson(t, ((Control)control).Text);
                        column.Text = TranslationGet(t, column.Text);
                    }
                }
            }
//            else
//            {
//                Log(control.GetType().ToString());
//            }
        }
        string TranslationGet(string key, string defaultText)
        {
            if (Program.LanguageJObject != null && Program.LanguageJObject.ContainsKey(key))
            {
                return Program.LanguageJObject[key].ToString();
            }
//            else
//            {
//                tojson(key, defaultText);
//
//            }

            return defaultText;
        }
        /// <summary>
        /// Capture untranslated control use
        /// </summary>
        /// <param name="t"></param>
        /// <param name="text"></param>
        void tojson(string t, string text)
        {
            richTextBox1.AppendText("\n\"" + t + "\":\"" + text + "\",");
        }
        #endregion

        public AtlasServerManager(string arkManagerPath, string language)
        {
            this.ArkManagerPath = arkManagerPath;
            InitializeComponent();
            SetLanguage(language);


            instance = this;
            // 
            // ServerList
            // 
            ServerList = new ArkListView
            {
                AllowColumnReorder = true,
                BackColor = System.Drawing.SystemColors.Window,
                CheckBoxes = true,
                ContextMenuStrip = contextMenuStrip1,
                Dock = DockStyle.Fill,
                FullRowSelect = true,
                GridLines = true,
                Location = new System.Drawing.Point(4, 4),
                Margin = new Padding(4),
                MultiSelect = false,
                Name = "ServerList",
                RightToLeft = System.Windows.Forms.RightToLeft.No,
                Size = new System.Drawing.Size(668, 256),
                TabIndex = 0,
                UseCompatibleStateImageBehavior = false,
                View = View.Details
            };
            ServerList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeader3, columnHeader1, columnHeader6, columnHeader7, columnHeader4, columnHeader5});
            tabPage1.Controls.Add(ServerList);
            inputDialog = new InputDialog();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            TranslationHelp("Main", this.Controls);
            //return;
            //Translate.TranslateMenu(menuStrip1.Items, "zh-TW");
            //Translate.TranslateComponents(Controls, "zh-TW");
            //Translate.TranslateListView(ServerList.Columns, "zh-TW");
            //Translate.FirstTranslate = true;
            ASMTitle = Text;
            //ArkManagerPath = Path.GetDirectoryName(Application.ExecutablePath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Replace("/", @"\") + Path.DirectorySeparatorChar;
            SteamPath = Path.Combine(ArkManagerPath, @"Steam\");
            Registry.LoadRegConfig(this);
            Worker.Init(this, ServerList.Items.Count > 0);
            if (File.Exists(ArkManagerPath + "ShooterGameServer.exe")) ServerPath = ArkManagerPath + "ShooterGameServer.exe";
            else
            {
                string[] Files = Directory.GetFiles(ArkManagerPath, "*.exe", SearchOption.AllDirectories);
                foreach (string file in Files)
                {
                    if (!file.Contains("steamapps") && Path.GetFileNameWithoutExtension(file) == "ShooterGameServer")
                    {
                        ServerPath = file;
                        break;
                    }
                }
            }
            if (!checkAutoServerUpdate.Checked) ServerUpdater.Updating = false;
            SetupCallbacks();
        }

        private void AddServer()
        {
            AddServer AddSrv = new AddServer(ServerPath);
            TranslationHelp(AddSrv.Name, AddSrv.Controls);
            if (AddSrv.ShowDialog() == DialogResult.OK)
            {
                ServerList.Items.Add(new ArkServerListViewItem(AddSrv.ServerData));
                Registry.SaveRegConfig(this);
                Log(AddSrv.ServerData.AltSaveDirectory + " Added!");
                Worker.ForceUpdaterRestart(this);
            }
            AddSrv.Dispose();
        }

        private void RemoveServer()
        {

            if (ServerList.FocusedItem != null && MessageBox.Show(string.Format(TranslationGet("RemoveServer-text", "Are you sure you want to delete ServerX: {0}, ServerY: {1}, Port: {2} ?\n Press 'Yes' To Delete!"),
                        ((ArkServerListViewItem)ServerList.FocusedItem).GetServerData().ServerX, ((ArkServerListViewItem)ServerList.FocusedItem).GetServerData().ServerY, ((ArkServerListViewItem)ServerList.FocusedItem).GetServerData().ServerPort)
                    , string.Format(TranslationGet("RemoveServer-caption","Delete ServerX:{0}, ServerY: {1}, Port: {2}?"), ((ArkServerListViewItem)ServerList.FocusedItem).GetServerData().ServerX, ((ArkServerListViewItem)ServerList.FocusedItem).GetServerData().ServerY, ((ArkServerListViewItem)ServerList.FocusedItem).GetServerData().ServerPort), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                ArkServerListViewItem ASLVI = ((ArkServerListViewItem)ServerList.FocusedItem);
                Log(ASLVI.GetServerData().AltSaveDirectory + TranslationGet("Removed", " Removed!"));
                Registry.DeleteServer(ServerList.FocusedItem.Index);
                ServerList.Items.RemoveAt(ServerList.FocusedItem.Index);
                if (ServerList.Items.Count == 0) Worker.StopUpdating();
            }
        }

        private void EditServer()
        {
            if (ServerList.FocusedItem != null)
            {
                ArkServerListViewItem ASLVI = ((ArkServerListViewItem)ServerList.FocusedItem);
                AddServer AddSrv = new AddServer(ASLVI.GetServerData(), ServerPath);
                TranslationHelp(AddSrv.Name, AddSrv.Controls);
                if (AddSrv.ShowDialog() == DialogResult.OK) ASLVI.SetServerData(AddSrv.ServerData);
                Log(ASLVI.GetServerData().AltSaveDirectory + TranslationGet("Edited", " Edited!"));
                AddSrv.Dispose();
            }
        }

        private void StartServer()
        {
            if (ServerList.FocusedItem != null)
            {
                ArkServerListViewItem ASLVI = (ArkServerListViewItem)ServerList.FocusedItem;
                ASLVI.GetServerData().StartServer();
                ASLVI.UpdateStatus();
                Registry.SaveRegConfig(this);
                Log(ASLVI.GetServerData().AltSaveDirectory + TranslationGet("Started", " Started!"));
            }
        }

        private void StopServer()
        {
            if (ServerList.FocusedItem != null)
            {
                ArkServerListViewItem ASLVI = (ArkServerListViewItem)ServerList.FocusedItem;
                ASLVI.GetServerData().StopServer();
                ASLVI.UpdateStatus();
                Registry.SaveRegConfig(this);
                Log(ASLVI.GetServerData().AltSaveDirectory + TranslationGet("Stopped", " Stopped!"));
            }
        }

        private void RconBroadcast(bool AllServers)
        {
            if (AllServers || ServerList.FocusedItem != null)
            {
                ArkServerListViewItem ASLVI = AllServers ? null : (ArkServerListViewItem)ServerList.FocusedItem;
                inputDialog.Text = "Broadcast to " + (AllServers ? "All" : ASLVI.GetServerData().AltSaveDirectory);
                inputDialog.SendButton.Text = "Broadcast";
                if (inputDialog.ShowDialog() == DialogResult.OK)
                {
                    if (AllServers) SourceRconTools.SendCommandToAll("broadcast " + inputDialog.InputText.Text);
                    else SourceRconTools.SendCommand("broadcast " + inputDialog.InputText.Text, ASLVI);
                    Log("Broadcasted!");
                }
            }
            else
            {
                MessageBox.Show(TranslationGet("Please_select_a_server", "Please select a server!!!"));
            }
        }

        private void RconSaveWorld(bool AllServers)
        {
            if (AllServers || ServerList.FocusedItem != null)
            {
                if (AllServers) SourceRconTools.SendCommandToAll("saveworld");
                else
                {
                    ArkServerListViewItem ASLVI = (ArkServerListViewItem)ServerList.FocusedItem;
                    SourceRconTools.SendCommand("saveworld", ASLVI);
                }
                Log(TranslationGet("SavedWorld", "Saved World!"));
            }
            else
            {
                MessageBox.Show(TranslationGet("Please_select_a_server", "Please select a server!!!"));
            }
        }

        private void RconCloseSaveWorld(bool AllServers)
        {
            if (AllServers || ServerList.FocusedItem != null)
            {
                if (AllServers) SourceRconTools.SendCommandToAll("DoExit");
                else
                {
                    ArkServerListViewItem ASLVI = (ArkServerListViewItem)ServerList.FocusedItem;
                    SourceRconTools.SendCommand("DoExit", ASLVI);
                }
                Log("Closed Saved World!");
            }
            else
            {
                MessageBox.Show("Please select a server!!!");
            }
        }

        private void RconCustomCommand(bool AllServers)
        {
            if (AllServers || ServerList.FocusedItem != null)
            {
                ArkServerListViewItem ASLVI = AllServers ? null : (ArkServerListViewItem)ServerList.FocusedItem;
                inputDialog.Text = "Send Custom Command to " + (AllServers ? "All" : ASLVI.GetServerData().AltSaveDirectory);
                inputDialog.SendButton.Text = "Send Command";
                if (inputDialog.ShowDialog() == DialogResult.OK)
                {
                    if (AllServers) SourceRconTools.SendCommandToAll(inputDialog.InputText.Text);
                    else SourceRconTools.SendCommand(inputDialog.InputText.Text, ASLVI);
                    Log("Custom Command Executed: " + inputDialog.InputText.Text);
                }
            }
            else
            {
                MessageBox.Show("Please select a server!!!");
            }
        }

        private void RconPlugin(bool AllServers, bool Load)
        {
            if (AllServers || ServerList.FocusedItem != null)
            {
                ArkServerListViewItem ASLVI = AllServers ? null : (ArkServerListViewItem)ServerList.FocusedItem;
                inputDialog.Text = (Load ? "Load" : "Unload") + " Plugin to " + (AllServers ? "All" : ASLVI.GetServerData().AltSaveDirectory);
                inputDialog.SendButton.Text = (Load ? "Load" : "Unload") + " Plugin";
                if (inputDialog.ShowDialog() == DialogResult.OK)
                {
                    if (AllServers) SourceRconTools.SendCommandToAll("plugins." + (Load ? "load " : "unload ") + inputDialog.InputText.Text);
                    else SourceRconTools.SendCommand("plugins." + (Load ? "load " : "unload ") + inputDialog.InputText.Text, ASLVI);
                    Log("Plugin " + (Load ? "Loaded" : "Unloaded") + ": " + inputDialog.InputText.Text);
                }
            }
            else
            {
                MessageBox.Show("Please select a server!!!");
            }
        }

        private void Language_Click(object sender, EventArgs e)
        {
            //languageToolStripMenuItem
            foreach (ToolStripMenuItem dropDownItem in languageToolStripMenuItem.DropDown.Items)
            {
                if (dropDownItem.Name == ((ToolStripMenuItem)sender).Name && !((ToolStripMenuItem)sender).Checked)
                {
                    dropDownItem.Checked = true;
                    Includes.Registry.SaveRegkey("Language", ((ToolStripMenuItem)sender).Name);
                    Application.Exit();
                    Application.Restart();
                }
                else
                {
                    dropDownItem.Checked = false;
                }
            }
        }

        private void SetupCallbacks()
        {
            BackupButton.Click += (e, a) => Backup.BackupConfigs(this);

            FormClosing += (e, a) =>
            {
                Worker.DestroyAll();
                Registry.SaveRegConfig(this);
            };

            checkAutoServerUpdate.CheckedChanged += (e, a) =>
            {
                if (!checkAutoServerUpdate.Checked)
                    ServerUpdater.Updating = false;
            };

            ServerList.MouseDoubleClick += (e, a) => EditServer();
            ServerList.SelectedIndexChanged += (e, a) =>
            {
                if (ServerList.FocusedItem != null)
                {
                    ArkServerListViewItem ASLVI = (ArkServerListViewItem)ServerList.FocusedItem;
                    StartButton.Enabled = true;
                    if (ASLVI.GetServerData().IsRunning())
                    {
                        StartButton.BackColor = System.Drawing.Color.Red;
                        StartButton.ForeColor = System.Drawing.Color.White;
                        StartButton.Text = "Stop";
                    }
                    else
                    {
                        StartButton.BackColor = System.Drawing.Color.Green;
                        StartButton.ForeColor = System.Drawing.Color.White;
                        StartButton.Text = "Start";
                    }
                }
                else
                {
                    StartButton.BackColor = System.Drawing.Color.DarkGray;
                    StartButton.ForeColor = System.Drawing.Color.DimGray;
                    StartButton.Enabled = false;
                }
            };

            StartButton.Click += (e, a) =>
            {
                if (ServerList.FocusedItem != null)
                {
                    ArkServerListViewItem ASLVI = (ArkServerListViewItem)ServerList.FocusedItem;
                    if (StartButton.Text == "Start") ASLVI.GetServerData().StartServer();
                    else ASLVI.GetServerData().StopServer();
                    ASLVI.UpdateStatus();
                }
            };

            addToolStripMenuItem.Click += (e, a) => AddServer();
            removeToolStripMenuItem.Click += (e, a) => RemoveServer();
            editSettingsToolStripMenuItem.Click += (e, a) => EditServer();

            addToolStripMenuItem1.Click += (e, a) => AddServer();
            removeToolStripMenuItem1.Click += (e, a) => RemoveServer();
            editSettingsToolStripMenuItem1.Click += (e, a) => EditServer();

            startToolStripMenuItem.Click += (e, a) => StartServer();
            stopToolStripMenuItem.Click += (e, a) => StopServer();

            startToolStripMenuItem1.Click += (e, a) => StartServer();
            stopToolStripMenuItem1.Click += (e, a) => StopServer();

            button1.Click += (e, a) =>
            {
                if (ServerUpdater.Updating)
                {
                    MessageBox.Show("Already Updating", "Update in progress", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                Log("[Atlas] Forcing Update");
                ServerUpdater.ForcedUpdate = true;
                Worker.ForceUpdaterRestart(this);
            };

            ClearConfigButton.Click += (e, a) =>
            {
                if (MessageBox.Show("Are you sure you want to erase all your configurations?", "Configuration Reset", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Registry.ClearAll();
                    ServerList.Clear();
                }
            };

            broadcastToolStripMenuItem.Click += (e, a) => RconBroadcast(false);
            saveWorldToolStripMenuItem.Click += (e, a) => RconSaveWorld(false);
            closeSaveWorldToolStripMenuItem.Click += (e, a) => RconCloseSaveWorld(false);
            customCommandToolStripMenuItem.Click += (e, a) => RconCustomCommand(false);
            loadPluginToolStripMenuItem.Click += (e, a) => RconPlugin(false, true);
            unloadPluginToolStripMenuItem.Click += (e, a) => RconPlugin(false, false);

            broadcastToolStripMenuItem1.Click += (e, a) => RconBroadcast(true);
            saveWorldToolStripMenuItem1.Click += (e, a) => RconSaveWorld(true);
            closeSaveWorldToolStripMenuItem1.Click += (e, a) => RconCloseSaveWorld(true);
            customCommandToolStripMenuItem1.Click += (e, a) => RconCustomCommand(true);
            loadPluginToolStripMenuItem1.Click += (e, a) => RconPlugin(true, true);
            unloadPluginToolStripMenuItem1.Click += (e, a) => RconPlugin(true, false);


            broadcastToolStripMenuItem2.Click += (e, a) => RconBroadcast(false);
            saveWorldToolStripMenuItem2.Click += (e, a) => RconSaveWorld(false);
            closeSaveWorldToolStripMenuItem2.Click += (e, a) => RconCloseSaveWorld(false);
            customCommandToolStripMenuItem2.Click += (e, a) => RconCustomCommand(false);
            loadPluginToolStripMenuItem2.Click += (e, a) => RconPlugin(false, true);
            unloadPluginToolStripMenuItem2.Click += (e, a) => RconPlugin(false, false);

            broadcastToolStripMenuItem3.Click += (e, a) => RconBroadcast(true);
            saveWorldToolStripMenuItem3.Click += (e, a) => RconSaveWorld(true);
            closeSaveWorldToolStripMenuItem3.Click += (e, a) => RconCloseSaveWorld(true);
            customCommandToolStripMenuItem3.Click += (e, a) => RconCustomCommand(true);
            loadPluginToolStripMenuItem3.Click += (e, a) => RconPlugin(true, true);
            unloadPluginToolStripMenuItem3.Click += (e, a) => RconPlugin(true, false);
        }

        bool bFirst = true;
        public void Log(string txt)
        {
            if (richTextBox1.InvokeRequired)
            {
                if (richTextBox1 == null) return;
                richTextBox1.Invoke(new RichTextBoxUpdateEventHandler(Log), new object[] { txt });
            }
            else
            {
                if (txt == null || txt == string.Empty || txt.Length < 8) return;
                if (txt.Contains("downloading") || txt.Contains("validat") || txt.Contains("committing") || txt.Contains("preallocating"))
                {
                    if (!FirstDl)
                    {
                        FirstDl = true;
                        richTextBox1.AppendText(string.Format("\n[{0}] {1}", DateTime.Now.ToString("hh:mm"), txt));
                        richTextBox1.ScrollToCaret();
                    }
                    else
                    {
                        string[] lines = richTextBox1.Lines;
                        lines[lines.Length - 1] = string.Format("[{0}] {1}", DateTime.Now.ToString("hh:mm"), txt);
                        richTextBox1.Lines = lines;
                        richTextBox1.SelectionStart = richTextBox1.Text.Length;
                    }
                }
                else
                {
                    richTextBox1.AppendText(string.Format("{0}[{1}] {2}", (bFirst ? "" : "\n"), DateTime.Now.ToString("hh:mm"), txt));
                    richTextBox1.ScrollToCaret();
                    bFirst = false;
                }
            }
        }
    }
}