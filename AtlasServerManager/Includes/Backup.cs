using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace AtlasServerManager.Includes
{
    class Backup
    {
        public static void RestoreConfigs(AtlasServerManager AtlasMgr, string UpdatePath)
        {
            if (!AtlasMgr.ConfigReplaceCheck.Checked) return;

            string BackupPath = Path.Combine(UpdatePath + Path.DirectorySeparatorChar, @"ConfigRestore\");
            if (Directory.Exists(BackupPath))
            {
                string FilesPath = Path.Combine(UpdatePath + Path.DirectorySeparatorChar, @"ShooterGame\Config\");
                string[] Files = Directory.GetFiles(BackupPath, "*.ini", SearchOption.TopDirectoryOnly);
                foreach (string file in Files)
                    File.Copy(file, FilesPath + Path.GetFileName(file), true);

                FilesPath = Path.Combine(UpdatePath + Path.DirectorySeparatorChar, @"AtlasTools\RedisDatabase\");

                Files = Directory.GetFiles(BackupPath, "*.conf", SearchOption.TopDirectoryOnly);
                foreach (string file in Files)
                    File.Copy(file, FilesPath + Path.GetFileName(file), true);
            }
        }

        public static void BackupConfigs(AtlasServerManager AtlasMgr)
        {
            List<string> UpdatePaths = new List<string>();
            foreach (ArkServerListViewItem ASLVI in AtlasMgr.ServerList.Items)
            {
                string UpdatePath = ASLVI.GetServerData().ServerPath;
                if (UpdatePath.StartsWith("./") || UpdatePath.StartsWith(@".\")) UpdatePath = UpdatePath.Replace("./", System.AppDomain.CurrentDomain.BaseDirectory).Replace(@".\", System.AppDomain.CurrentDomain.BaseDirectory).Replace("//", "/").Replace(@"\\", @"\");                    
                if (!Directory.Exists(Path.GetDirectoryName(UpdatePath))) Directory.CreateDirectory(Path.GetDirectoryName(UpdatePath));
                if (UpdatePath.Contains(@"ShooterGame\Binaries\Win64")) UpdatePath = Regex.Split(UpdatePath, "\\ShooterGame")[0];
                if (!UpdatePaths.Contains(UpdatePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, '\\'))) UpdatePaths.Add(UpdatePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, '\\'));
            }

            string BackupPath, FilesPath;
            string[] Files;
            foreach (string UpdatePath in UpdatePaths)
            {
                AtlasMgr.Log("[Atlas] Backing up INI and DB Configs at Path: " + UpdatePath);
                BackupPath = Path.Combine(UpdatePath + Path.DirectorySeparatorChar, @"ConfigRestore\");
                if (!Directory.Exists(BackupPath)) Directory.CreateDirectory(BackupPath);
                FilesPath = Path.Combine(UpdatePath + Path.DirectorySeparatorChar, @"ShooterGame\Config\");
                Files = Directory.GetFiles(FilesPath, "*.ini", SearchOption.TopDirectoryOnly);
                foreach (string file in Files)
                    File.Copy(file, BackupPath + Path.GetFileName(file), true);

                FilesPath = Path.Combine(UpdatePath + Path.DirectorySeparatorChar, @"AtlasTools\RedisDatabase\");

                Files = Directory.GetFiles(FilesPath, "*.conf", SearchOption.TopDirectoryOnly);
                foreach (string file in Files)
                    File.Copy(file, BackupPath + Path.GetFileName(file), true);
            }

            AtlasMgr.Log("[Atlas] Backing up INI and DB Configs Completed!");
        }
    }
}
