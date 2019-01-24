using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AtlasServerManager
{
    static class Program
    {
        public static JObject LanguageJObject;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            string language = Includes.Registry.LoadRegkey("Language") != null ? Includes.Registry.LoadRegkey("Language").ToString() : "en";
            string arkManagerPath = Path.GetDirectoryName(Application.ExecutablePath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Replace("/", @"\") + Path.DirectorySeparatorChar;
            if (File.Exists(Path.GetFullPath(arkManagerPath + "/Language/" + language + ".json")))
            {
                //System.IO.StreamReader file1=new StreamReader(Path.GetFullPath(arkManagerPath + "/Language/" + language + ".json"),System.Text.Encoding.Default)
                //using (System.IO.StreamReader file = System.IO.File.OpenText(Path.GetFullPath(arkManagerPath + "/Language/" + language + ".json"),e))
                using (System.IO.StreamReader file = new StreamReader(Path.GetFullPath(arkManagerPath + "/Language/" + language + ".json"), System.Text.Encoding.Default))
                {
                    using (JsonTextReader reader = new JsonTextReader(file))
                    {
                        LanguageJObject = (JObject)JToken.ReadFrom(reader);
                    }
                }
            }

            Application.Run(new AtlasServerManager(arkManagerPath, language));
        }
    }
}