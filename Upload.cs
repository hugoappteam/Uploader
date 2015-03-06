using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using System.Net;
using System.Collections.Specialized;
using Parse;
using System.Threading.Tasks;



namespace Uploader
{
    class Upload
    {

        public static void sendPlanJSONString(DataTable vert, DataTable info)
        {
            bool success = true;
            Task uploadToParse = Task.Run(async () => { await parseUpload(vert); });
            //Versuchen
            try
            {
                
                //Testasugabe
                Console.WriteLine(JsonConvert.SerializeObject(vert, Formatting.Indented));
                
                //WebClient vorbereiten
                string URL = "http://hjg.pf-control.de/insertJSON.php";
                WebClient webClient = new WebClient();


                //Tabellen für POST in JSON-String umwandeln
                NameValueCollection formData = new NameValueCollection();
                formData["vert"] = JsonConvert.SerializeObject(vert, Formatting.Indented);
                formData["info"] = JsonConvert.SerializeObject(info, Formatting.Indented);

                //Daten per POST an PHP-Script übergeben
                byte[] responseBytes = webClient.UploadValues(URL, "POST", formData);
                //Antwort des Webservers abwarten
                string responsefromserver = Encoding.UTF8.GetString(responseBytes);
                Console.WriteLine(responsefromserver);
                //WEbclient entsorgen
                webClient.Dispose(); 
                
            }

            //Fehler abfangen
            catch (Exception ex)
            {
                Console.WriteLine("Fehler beim Hochladen der Daten: " + ex.Message);
                //Log
                DateTime current = DateTime.Now;
                using (StreamWriter Writer = new StreamWriter(@"../UploadLog/log_" + current.ToShortDateString() + ".txt", true))
                {
                    Writer.WriteLine("Fehler beim Hochladen der Daten: " + ex.Message + " " + current.TimeOfDay);
                }
                success = false;
            }

            //Wenn erfolgreich
            if (success)
            {
                //Log
                using (StreamWriter Writer = new StreamWriter(@"../UploadLog/log_" + DateTime.Now.ToShortDateString() + ".txt", true))
                {
                    Writer.WriteLine("JSON-String hochgeladen." + " " + DateTime.Now.TimeOfDay);

                }
                Console.WriteLine("JSON-String hochgeladen.");
            }

            uploadToParse.Wait();
        }
        
        private static async Task parseUpload(DataTable plan)
        {
            await truncateParse();

            Console.WriteLine("Started Parse Upload");
            foreach (DataRow row in plan.Rows)
            {
               await uploadRowToParse(row);
            }
        }

        private static async Task uploadRowToParse(DataRow row)
        {
            ParseObject vert = new ParseObject("VertretungObject");

            vert["Klasse"] = row["Klasse"].ToString();
            vert["Lehrer"] = row["Lehrer"].ToString();
            vert["Tag"] = row["Tag"].ToString();
            vert["Stunde"] = row["Stunde"].ToString();
            vert["Fach"] = row["Fach"].ToString();
            vert["Raum"] = row["Raum"].ToString();
            vert["Vertreter"] = row["Vertreter"].ToString();
            vert["Info"] = row["Info"].ToString();

            await vert.SaveAsync();
        }

        private static async Task truncateParse()
        {
            var query = from vert in ParseObject.GetQuery("VertretungObject")
                        where vert.Get<string>("Klasse") != ""
                        select vert;
            IEnumerable<ParseObject> results = await query.FindAsync();

            foreach (ParseObject obj in results)
            {
                await obj.DeleteAsync();
            }

        }
    }

}
