using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace AtlasServerManager
{
    class Translate
    {
        public static bool FirstTranslate = true;
        public static void TranslateComponent(object Obj, string TranslateTo)
        {
            string tData = "", nData = "";
            if (Obj.GetType() == typeof(Label) || Obj.GetType() == typeof(Button)
             || Obj.GetType() == typeof(CheckBox) || Obj.GetType() == typeof(GroupBox)
             || Obj.GetType() == typeof(TabPage))
            {
                tData = ((Control)Obj).Text;
                nData = ((Control)Obj).Name;
                if (Includes.Registry.GetText(nData, tData, TranslateTo, FirstTranslate, ref tData))
                {
                    ((Control)Obj).Text = tData;
                    return;
                }
            }
            else if (Obj.GetType() == typeof(ToolStripMenuItem))
            {
                tData = ((ToolStripMenuItem)Obj).Text;
                nData = ((ToolStripMenuItem)Obj).Name;
                if (Includes.Registry.GetText(nData, tData, TranslateTo, FirstTranslate, ref tData))
                {
                    ((ToolStripMenuItem)Obj).Text = tData;
                    return;
                }
            }
            else if (Obj.GetType() == typeof(ColumnHeader))
            {
                tData = ((ColumnHeader)Obj).Text;
                nData = ((ColumnHeader)Obj).Name;
                if (Includes.Registry.GetText(nData, tData, TranslateTo, FirstTranslate, ref tData))
                {
                    ((ColumnHeader)Obj).Text = tData;
                    return;
                }
            }

            try
            {
                using (WebClient wb = new WebClient())
                {
                    wb.Headers.Add(HttpRequestHeader.UserAgent, "AndroidTranslate/5.3.0.RC02.130475354-53000263 5.1 phone TRANSLATE_OPM5_TEST_1");
                    wb.Headers.Add(HttpRequestHeader.AcceptCharset, "UTF-8");
                    wb.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    wb.Encoding = Encoding.UTF8;
                    wb.UploadStringCompleted += (e, a) =>
                    {
                        if (a.Cancelled) return;
                        if (a.Result.Length > 0 && a.Result.Contains("\"trans\""))
                        {
                            string[] splts = a.Result.Split('"');
                            if (splts.Length > 5)
                            {
                                if (Obj.GetType() == typeof(Label) || Obj.GetType() == typeof(Button)
                                 || Obj.GetType() == typeof(CheckBox) || Obj.GetType() == typeof(GroupBox)
                                 || Obj.GetType() == typeof(TabPage))
                                    ((Control)Obj).Text = splts[5];
                                else if (Obj.GetType() == typeof(ToolStripMenuItem))
                                    ((ToolStripMenuItem)Obj).Text = splts[5];
                                else if (Obj.GetType() == typeof(ColumnHeader))
                                    ((ColumnHeader)Obj).Text = splts[5];
                                else return;

                                Includes.Registry.GetText(nData, splts[5], TranslateTo, false, ref tData);
                            }
                        }
                    };


                    wb.UploadStringAsync(new Uri("https://translate.google.com/translate_a/single?client=at&dt=t&dt=ld&dt=qca&dt=rm&dt=bd&dj=1&hl=es-ES&ie=UTF-8&oe=UTF-8&inputm=2&otf=2&iid=1dd3b944-fa62-4b55-b330-74909a99969e"), "POST", "sl=en&tl=" + TranslateTo + "&q=" + WebUtility.UrlEncode(tData));
                }
            }
            catch
            {
            }
        }

        public static void TranslateComponents(Control.ControlCollection controlCollection, string Language)
        {
            foreach (Control con in controlCollection)
            {
                if (con.GetType() == typeof(Label) || con.GetType() == typeof(Button)
                     || con.GetType() == typeof(CheckBox) || con.GetType() == typeof(GroupBox)
                     || con.GetType() == typeof(TabPage))
                {
                    TranslateComponent(con, Language);
                }
                TranslateComponents(con.Controls, Language);
            }
        }

        private static IEnumerable<ToolStripMenuItem> GetItems(ToolStripMenuItem item)
        {       
            foreach (ToolStripMenuItem dropDownItem in item.DropDownItems)
            {
                if (dropDownItem.HasDropDownItems)
                {
                    foreach (ToolStripMenuItem subItem in GetItems(dropDownItem))
                        yield return subItem;
                }
                yield return dropDownItem;
            }
        }

        public static void TranslateMenu(ToolStripItemCollection controlCollection, string Language)
        {
            foreach (ToolStripMenuItem toolStripItem in controlCollection)
            {
                TranslateComponent(toolStripItem, Language);
                var items = GetItems(toolStripItem);
                foreach (ToolStripMenuItem toolStripSubItem in items) TranslateComponent(toolStripSubItem, Language);
            }
        }

        public static void TranslateListView(ListView.ColumnHeaderCollection controlCollection, string Language)
        {
            foreach (ColumnHeader toolStripItem in controlCollection)
            {
                TranslateComponent(toolStripItem, Language);
            }
        }
    }
}