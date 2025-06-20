using System.Net;
using System.Text.Json;
using System.Text;
using RedDotMM.Model;
using RedDotMM.Model.NoDB;
using System.ComponentModel;
using RedDotMM.Logging;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.Json.Serialization;
using RedDotMM.Web.Model;

namespace RedDotMM.Web
{
    public class Webservice : INotifyPropertyChanged
    {
        private HttpListener _listener;
        private string _webhookUrl;// = "http://example.com/webhook"; // Webhook-URL konfigurieren
        private string _url = "";


        public event PropertyChangedEventHandler? PropertyChanged;

        public event Action<Guid> Fehlschuss;

        public event Action<Guid> ScheibeAuffuellen;



        #region Daten


        private Ergebnis _ergebnis;
        public Ergebnis Ergebnis
        {
            get => _ergebnis;
            set
            {
                if (_ergebnis != value)
                {
                    _ergebnis = value;
                    OnPropertyChanged(nameof(Ergebnis));
                    TriggerWebhook();
                }
            }
        }


        private ScheibeModel ScheibeModel
        {
            get
            {
                if (Ergebnis != null)
                {
                    return new ScheibeModel
                    {
                        SchuetzeName = Ergebnis.Serie?.Schuetze?.AnzeigeName ?? "Unbekannt",
                        WettkampfName = Ergebnis.Serie?.Schuetze?.Wettbewerb?.Name ?? "Unbekannt",
                        SchiebenNummer = Ergebnis.LfdNummer,
                        Teilerwertung = Ergebnis.Serie?.Schuetze?.Wettbewerb?.Teilerwertung ?? false,
                        Schuesse = Ergebnis.Schuesse.Select(s => new SchussModel
                        {
                            Ringzahl = s.Wert,
                            IsProbe = s.Typ == SchussTyp.Probe,
                            x = s.X,
                            y = s.Y,
                             SchussNummer =s.LfdSchussNummer
                        }).ToArray()
                    };
                }
                return null;
            }
        }


        private byte[] _scheibenBild;
        /// <summary>
        /// Die Scheibe als PNG-Bild.
        /// </summary>
        public byte[] ScheibenBild
        {
            get => _scheibenBild;
            set
            {
                if (_scheibenBild != value)
                {
                    _scheibenBild = value;
                    OnPropertyChanged(nameof(ScheibenBild));
                    TriggerWebhook();
                }
            }
        }

        #endregion Daten    


        private void OnPropertyChanged(string v)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(v));
            }
        }


        private void RaiseFehlschuss()
        {
            if (Ergebnis == null)
            {
                throw new InvalidOperationException("Ergebnis ist nicht gesetzt.");
            }
            Fehlschuss?.Invoke(Ergebnis.ErgebnisId);
        }

        private void RaiseScheibeAuffuellen()
        {
            if (Ergebnis == null)
            {
                throw new InvalidOperationException("Ergebnis ist nicht gesetzt.");
            }
            ScheibeAuffuellen?.Invoke(Ergebnis.ErgebnisId);
        }


        public string[] GetURLs(int Port)
        {
            try
            {
                var hostAddresses = System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName());
                var urls = hostAddresses
                    .Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    .Select(ip => $"http://{ip}:{Port}/")
                    .ToList();
                urls.Add($"http://localhost:{Port}/");
                //urls.Add($"http://+:{Port}/");
                return urls.Distinct().ToArray();
            }
            catch (Exception ex)
            {
                Logging.Logger.Instance.Log($"Fehler beim Abrufen der URLs: {ex.Message}", Logging.LogType.Fehler);
                return null;
            }

        }


        private bool cancelListening=false;

        public void StartWebserviceAsync(string url = "http://localhost:7070/")
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                {
                    throw new InvalidOperationException("URL ist leer!");
                    return;
                }

                if (_listener != null && _listener.IsListening)
                {
                    throw new InvalidOperationException("Der Webservice läuft bereits.");
                    return;
                }


             
                var thread = new Thread(() => StartWebService(url) );
                
                thread.Start();

            }
            catch (Exception ex)
            {
                Logging.Logger.Instance.Log($"Fehler beim Starten des Webservices: {ex.Message}", Logging.LogType.Fehler);
                //throw new InvalidOperationException("Fehler beim Starten des Webservices.", ex);
            }


        }

        public void StopWebserviceAsync()
        {
            try
            {
                cancelListening = true;
                if (_listener != null && _listener.IsListening)
                {
                    _listener.Stop();
                    _listener.Close();
                    _listener = null;
                    Logging.Logger.Instance.Log("Webservice wurde gestoppt.", Logging.LogType.Info);
                }

                RemoveUrlRichtline();

            }
            catch (Exception ex)
            {
                Logging.Logger.Instance.Log($"Fehler beim Stoppen des Webservices: {ex.Message}", Logging.LogType.Fehler);
            }

        }

        

        private void StartWebService(string url)
        {
            try
            {
                cancelListening = false;
                if (_listener != null && _listener.IsListening)
                {
                    throw new InvalidOperationException("Listener noch aktiv");
                    return;
                }



                this._url = url.Trim();
                if (_url.StartsWith("http://") || _url.StartsWith("https://"))
                {
                    _url = _url.TrimEnd('/') + "/";
                }
                else
                {
                    throw new ArgumentException("Die URL muss mit 'http://' oder 'https://' beginnen.");
                }


                int port;
                int PortNoINdex = url.IndexOf(':',6) + 1;
                if (!(Int32.TryParse(_url.Substring(PortNoINdex).TrimEnd('/'), out port)))
                {
                    port = 80;
                }

                var formattedUrl = $"http://+:{port}/";

                _webhookUrl = _url + "webhook";

                SetUrlRichtlinie(port);


                //this._webhookUrl = _webhookUrl.TrimEnd('/') + "/webhook/";
                _listener = new HttpListener();

                _listener.Prefixes.Add(formattedUrl);
                _listener.Start();
                Console.WriteLine($"Webserver gestartet unter {_url}");

                // Überwachung der Ergebnisse starten
                //Task.Run(() => MonitorResults());
                Logging.Logger.Instance.Log($"Webserver gestartet unter {_url}", Logging.LogType.Info);
                while (!cancelListening)
                {
                    var context = _listener.GetContext();
                    HandleRequest(context);
                }

                Logger.Instance.Log("Webservice Ende erreicht!", LogType.Info);
            }
            catch (Exception ex)
            {
                Logging.Logger.Instance.Log($"Fehler beim Starten des Webservers: {ex.Message}", Logging.LogType.Fehler);
                //throw;
            }

        }


        private void SetUrlRichtlinie(int port)
        {


            //netsh http add urlacl url=http://+:80/ user=DOMAIN\Username listen=yes
            try
            {
                string user = $"{Environment.UserDomainName}\\{Environment.UserName}";

                // URL auf das richtige Format bringen (z.B. http://+:7070/)
                //string formattedUrl = _url;
                var formattedUrl = $"http://+:{port}/";


                string arguments = $"http add urlacl url={formattedUrl} user=\"{user}\" listen=yes";

                Process.Start(new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = arguments,
                    Verb = "runas", // Adminrechte erforderlich
                    CreateNoWindow = true,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Logging.Logger.Instance.Log($"Fehler beim Setzen der URL-Richtlinie: {ex.Message}", LogType.Fehler);
            }
        }

        private void RemoveUrlRichtline()
        {
            try
            {
                string formattedUrl = _url;


                //netsh http delete urlacl http://+:80/
                string arguments = $"http delete urlacl url={formattedUrl}";

                Process.Start(new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = arguments,
                    Verb = "runas", // Adminrechte erforderlich
                    CreateNoWindow = true,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Logging.Logger.Instance.Log($"Fehler beim zurücknehmen der URL-Richtline{ex.Message}", LogType.Fehler);
            }
        }



        private void HandleRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            try
            {

                Logging.Logger.Instance.Log($"Anfrage empfangen: {request.HttpMethod} {request.Url.AbsolutePath}", Logging.LogType.Info);

                if (request.Url.AbsolutePath.StartsWith("/static"))
                {
                    ServeStaticFiles(request, response);
                }
                else if (request.Url.AbsolutePath == "/api/Command/Fehlerschuss")
                {
                    RaiseFehlschuss();
                }
                else if (request.Url.AbsolutePath == "/api/Command/Auffuellen")
                {
                    RaiseScheibeAuffuellen();
                }
                else if (request.Url.AbsolutePath == "/api/ergebniss")
                {
                    ServeErgebniss(response);
                }
                else if (request.Url.AbsolutePath == "/api/bild")
                {
                    ServeBild(response);
                }
                else if (request.Url.AbsolutePath == "/webhook")
                {
                   //nix machen
                }

                else if (request.Url.AbsolutePath == "/")
                {
                    response.StatusCode = 200;
                    var message = Encoding.UTF8.GetBytes("Willkommen beim RedDotMM Webservice!");
                    response.OutputStream.Write(message, 0, message.Length);
                }
                else
                {
                    response.StatusCode = 404;
                    var message = Encoding.UTF8.GetBytes("Ressource nicht gefunden");
                    response.OutputStream.Write(message, 0, message.Length);
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                var errorMessage = Encoding.UTF8.GetBytes($"Fehler: {ex.Message}");
                response.OutputStream.Write(errorMessage, 0, errorMessage.Length);
            }
            finally
            {
                response.OutputStream.Close();
            }
        }

        private void ServeStaticFiles(HttpListenerRequest request, HttpListenerResponse response)
        {
            var folderContentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "folderContent.txt");
            if (!File.Exists(folderContentPath))
            {
                response.StatusCode = 404;
                var message = Encoding.UTF8.GetBytes("Die Datei folderContent wurde nicht gefunden.");
                response.OutputStream.Write(message, 0, message.Length);
                return;
            }

            var allowedFiles = File.ReadAllLines(folderContentPath);
            var fileList = allowedFiles.Select(f => f.Trim()).Where(f => !string.IsNullOrEmpty(f) && !f.StartsWith("#")).ToList();

            var requestedFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", request.Url.AbsolutePath.Substring(8));

            if (allowedFiles.Contains(Path.GetFileName(requestedFile)) && File.Exists(requestedFile))
            {

                var fileExtension = Path.GetExtension(requestedFile);
                //string contentType = "application/octet-stream";

                string contentType = fileExtension.ToLower() switch
                {
                    ".html" => "text/html",
                    ".htm" => "text/hmtl",
                    ".txt" => "text/plain",
                    ".js" => "text/javascript",
                    ".css" => "text/css",                  
                    _ => "application/octet-stream"
                };


                var fileContent = File.ReadAllBytes(requestedFile);
                response.StatusCode = 200;
                response.ContentType = contentType;                
                response.OutputStream.Write(fileContent, 0, fileContent.Length);
            }
            else
            {
                response.StatusCode = 403;
                var message = Encoding.UTF8.GetBytes("Zugriff verweigert oder Datei nicht gefunden.");
                response.OutputStream.Write(message, 0, message.Length);
            }
        }




        private void ServeBild(HttpListenerResponse response)
        {
            if (ScheibenBild == null || ScheibenBild.Length == 0)
            {
                response.StatusCode = 404;
                var message = Encoding.UTF8.GetBytes("Bild nicht gefunden.");
                response.OutputStream.Write(message, 0, message.Length);
                return;
            }
            response.ContentType = "image/png"; // Oder das entsprechende Bildformat
            response.OutputStream.Write(ScheibenBild, 0, ScheibenBild.Length);
        }



        private void ServeErgebniss(HttpListenerResponse response)
        {


            JsonSerializerOptions options = new()
            {
                ReferenceHandler = ReferenceHandler.Preserve,
                WriteIndented = true
            };
            var json = JsonSerializer.Serialize(ScheibeModel, options);
            var buffer = Encoding.UTF8.GetBytes(json);
            response.ContentType = "application/json";
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }








        #region Webhook


        //private async Task MonitorResults()
        //{
        //    while (true)
        //    {
        //        var currentResults = GetErgebnisse();

        //        if (!AreResultsEqual(_lastResults, currentResults))
        //        {
        //            _lastResults = currentResults;
        //            await TriggerWebhook(currentResults);
        //        }

        //        await Task.Delay(5000); // Überprüfung alle 5 Sekunden
        //    }
        //}

        private async Task TriggerWebhook()
        {
            try
            {

                if (_listener==null || !_listener.IsListening)
                    return;

                JsonSerializerOptions options = new()
                {
                    ReferenceHandler = ReferenceHandler.Preserve,
                    WriteIndented = true
                };
                var json = JsonSerializer.Serialize(Ergebnis, options);
                using var client = new HttpClient();
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(_webhookUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Webhook erfolgreich ausgelöst.");
                }
                else
                {
                    Console.WriteLine($"Fehler beim Auslösen des Webhooks: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Auslösen des Webhooks: {ex.Message}");
            }
        }

        private bool AreResultsEqual(List<object> oldResults, List<object> newResults)
        {
            return JsonSerializer.Serialize(oldResults) == JsonSerializer.Serialize(newResults);
        }





        #endregion Webhook



        public void SetFirewall(string url)
        {

            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("Die URL darf nicht leer sein.", nameof(url));
            }
            string port = url.Split(':').LastOrDefault()?.TrimEnd('/');
            string ruleName = "RedDotMM_" + port;

            Process.Start(new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = $"advfirewall firewall add rule name=\"{ruleName}\" dir=in action=allow protocol=TCP localport={port}",
                Verb = "runas", // Ensures it runs with admin privileges
                CreateNoWindow = true,
                UseShellExecute = true
            });
        }

        public void RemoveFirewall(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("Die URL darf nicht leer sein.", nameof(url));
            }
            string port = url.Split(':').LastOrDefault()?.TrimEnd('/');
            string ruleName = "RedDotMM_" + port;
            Process.Start(new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = $"advfirewall firewall delete rule name=\"{ruleName}\"",
                Verb = "runas", // Ensures it runs with admin privileges
                CreateNoWindow = true,
                UseShellExecute = true
            });

        }



        ~Webservice()
        {
            try
            {
                cancelListening = true;
                if (_listener != null)
                {
                    _listener.Stop();
                    _listener.Close();
                    _listener = null;
                    Logging.Logger.Instance.Log("Webservice wurde gestoppt.", Logging.LogType.Info);
                    RemoveUrlRichtline();
                }
            }catch(Exception ex)
            {

            }
        }
    }
}



