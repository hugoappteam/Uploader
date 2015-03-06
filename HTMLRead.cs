using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
//using System.Globalization;

namespace Uploader
{
    class HTMLRead
    {
        //leere Tabellen erstellen
        private static DataTable plan;
        private static DataTable info;
        //Varibale für den Tag der Vertretung
        private static DateTime Tag;
        //Array für die Spalten der
        private static string[] SPALTENplan = { "Klasse", "Lehrer", "Tag", "Stunde", "Fach", "Raum", "Vertreter", "Info" };
        private static string[] SPALTENinfo = { "Datum", "Info", "Abwesende Lehrer" };
        //String für zu durchsuchenen HTML-Code
        private static string html;
        
        public static void Structure()
        {
            Console.WriteLine("Änderung festgestellt, lese Datei...");
            
            //Tabellen vorbereiten
            plan = new DataTable();
            foreach (string value in SPALTENplan)
                plan.Columns.Add(value.Trim());
            
            info = new DataTable();
            foreach (string value in SPALTENinfo)
                info.Columns.Add(value.Trim());

            bool success = true;
            //Alle HTML-Dateien lesen
            foreach (string f in Directory.EnumerateFiles(".", "*.htm", SearchOption.AllDirectories))
            {

                //Versuchen
                try
                {
                    //HTML lesen
                    using (StreamReader sr = new StreamReader(f, Encoding.Default))
                    {
                        html = sr.ReadToEnd();
                    }
                    
                    //Datum auslesen
                    Match Date = Regex.Match(html, "<div class=\"mon_title\">(?<Tag>.*?)\\.(?<Monat>.*?)\\.(?<Jahr>.*?) .*?</div>", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                    //prüfen ob Datum gefunden
                    if (Date.Success)
                    {
                        Tag = new DateTime(Convert.ToInt32(StripHtml(Date.Groups["Jahr"].Value)), Convert.ToInt32(StripHtml(Date.Groups["Monat"].Value)), Convert.ToInt32(StripHtml(Date.Groups["Tag"].Value)));
                        Console.WriteLine(Tag.Date.ToShortDateString());
                        //Datum vergleichen
                        if (Tag.Date < DateTime.Now.Date)
                        {
                            //continue;
                        }
                    }


                    DataRow INFOdr = info.NewRow();
                    //Abwesende Lehrer finden
                    Match Abwesend = Regex.Match(html, "<tr class=\"info\"><td .*?>Abwesende Lehrer.*?</td><td.*?>(?<Lehrer>.*?)</td></tr>");
                    //prüfen ob Lehrer gefunden
                    if (Abwesend.Success)
                    {
                        //Lehrer auslesen
                        string[] Lehrer = Abwesend.Groups["Lehrer"].Value.ToString().Split(',');
                        foreach (string s in Lehrer)
                        {
                            Console.WriteLine(s.Trim());
                        }
                        //Abwesende Lehrer in DataRow speichern
                        INFOdr["Abwesende Lehrer"] = StripHtml(Abwesend.Groups["Lehrer"].Value.ToString());
                    }



                    //Info auslesen
                    Match InfoText = Regex.Match(html, "<tr class='info'>(?<Info>.*?)</tr>");
                    //prüfen ob Info gefunden
                    if (InfoText.Success)
                    {
                        Console.WriteLine(StripHtml(InfoText.Groups["Info"].Value.ToString()));
                        //Info in DataRow speichern
                        INFOdr["Info"] = StripHtml(InfoText.Groups["Info"].Value.ToString());

                    }
                    //prüfen ob Lehrer oder Info gefunden
                    if (InfoText.Success || Abwesend.Success)
                    {
                        //Info zu Tabelle hinzufügen
                        INFOdr["Datum"] = Tag.Date.Year.ToString() + "-" + Tag.Date.Month.ToString() + "." + Tag.Date.Day.ToString();
                        info.Rows.Add(INFOdr);
                    }
                    
                    //Vertretungen finden
                    Match mc = Regex.Match(html, "<table class=\"mon_list\".*?>(?<Table>.*?)</table>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    //prüfen ob Vertretungen gefunden
                    if (mc.Success)
                    {
                        //Vertretungen auslesen
                        MatchCollection Daten = Regex.Matches(mc.Groups["Table"].Value.ToString(), "<tr .*?><td .*?>(?<Klasse>.*?)</td><td .*?>(?<Stunde>.*?)</td><td .*?>(?<Fach>.*?)</td><td .*?>(?<Lehrer>.*?)</td><td .*?>(?<Vertreter>.*?)</td><td .*?>(?<Raum>.*?)</td><td .*?>(?<Info>.*?)</td></tr>",
                        RegexOptions.Singleline | RegexOptions.IgnoreCase);
                        //prüfen ob Tabelle nicht leer
                        if (Daten.Count > 0)
                        {
                            //alle Vertretungen auslesen und in Tabelle speichern
                            foreach (Match m in Daten)
                            {
                                DataRow dr = plan.NewRow();
                                //Vertretung ausgeben
                                Console.Write(StripHtml(m.Groups["Klasse"].Value.ToString()) + "\t");
                                Console.Write(StripHtml(m.Groups["Lehrer"].Value.ToString()) + "\t");
                                Console.Write(StripHtml(m.Groups["Vertreter"].Value.ToString()) + "\t");
                                Console.Write(StripHtml(m.Groups["Stunde"].Value.ToString()) + "\t");
                                Console.Write(StripHtml(m.Groups["Fach"].Value.ToString()) + "\t");
                                Console.Write(StripHtml(m.Groups["Raum"].Value.ToString()) + "\t");
                                Console.Write(StripHtml(m.Groups["Vertretung"].Value.ToString()) + "\t");
                                Console.WriteLine(StripHtml(m.Groups["Info"].Value.ToString()) + "\t");
                                //Vertretung in DataRow einfügen
                                dr["Klasse"] = StripHtml(m.Groups["Klasse"].Value.ToString());
                                dr["Lehrer"] = StripHtml(m.Groups["Lehrer"].Value.ToString());
                                dr["Tag"] = Tag.Date.Year.ToString() + "-" + Tag.Date.Month.ToString() + "." + Tag.Date.Day.ToString();
                                dr["Vertreter"] = StripHtml(m.Groups["Vertreter"].Value.ToString());
                                dr["Stunde"] = StripHtml(m.Groups["Stunde"].Value.ToString());
                                dr["Fach"] = StripHtml(m.Groups["Fach"].Value.ToString());
                                dr["Raum"] = StripHtml(m.Groups["Raum"].Value.ToString());
                                dr["Info"] = StripHtml(m.Groups["Info"].Value.ToString());

                                if (dr["Klasse"].Equals((object)""))
                                {
                                    plan.Rows[plan.Rows.Count - 1]["Info"] += " " + dr["Info"];
                                }
                                else
                                {
                                    //DataRow zu Tabelle hinzufügen
                                    plan.Rows.Add(dr);
                                }
                            }
                        }
                    }
                }
                //Fehler abfangen
                catch (Exception ex)
                {
                    Console.WriteLine("Fehler beim Lesen der Datei: " + ex.Message);
                    //Log
                    DateTime current = DateTime.Now;
                    using (StreamWriter Writer = new StreamWriter(@"../UploadLog/log_" + current.ToShortDateString() + ".txt", true))
                    {
                        Writer.WriteLine("Fehler beim Lesen der Datei(en): " + ex.Message + " " + current.TimeOfDay);
                    }
                    success = false;
                }

            }
            //Wenn erfolgreich...
            if (success && plan.Rows.Count > 0)
            {
                //Log
                using (StreamWriter Writer = new StreamWriter(@"../UploadLog/log_" + DateTime.Now.ToShortDateString() + ".txt", true))
                {
                    Writer.WriteLine("Datei(en) gelesen." + " " + DateTime.Now.TimeOfDay);

                }
                Console.WriteLine("Datei(en) gelesen.");
                //Plan hochladen
                Upload.sendPlanJSONString(plan, info);
            }


        }

        public static object StripHtml(string str)
        {
            //Variable für Rückgabe
            string strippedString;
            //Versuchen
            try
            {
                //Variable für RegularExpression Ausdruck
                string pattern = "<.*?>";
                string pattern2 = "&nbsp;";
                //alle HTML-Tags entfernen
                strippedString = Regex.Replace(str, pattern, string.Empty);
                strippedString = Regex.Replace(strippedString, pattern2, string.Empty);
            }
            //Fehler abfangen
            catch
            {
                strippedString = string.Empty;
            }
            //String zurückgeben
            return (strippedString).Trim();
        }
    }
}
