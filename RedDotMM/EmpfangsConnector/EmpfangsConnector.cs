using RedDotMM.Logging;
using RedDotMM.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedDotMM.EmpfangsConnector
{




   

    public class EmpfangsConnector : IDisposable
    {

        #region Konstanten für Scheibensystem

        private const byte b_stx = 2;

        private const byte b_enq = 5;

        private const byte b_ack = 6;

        private const byte b_dc1 = 17;

        private const byte b_nak = 21;

        private const byte b_etb = 23;

        #endregion





        private SerialPort serialPort;
        

        private Thread ENQThread;
        private bool enqThreadPause;
        private CancellationToken? enqCancelationToken;

        private CancellationToken? checkCancelationToken;

        private Thread CheckDataSendThread;

        private string Port = null;

        private bool lastConStateOpen = false;

        public event Action<Schuss> SchussEmpfangen;

        public event Action<bool> ConnectionChanged;

        private long lastDataSendMS = -1;

        public string[] Ports
        {
            get
            {
                try
                {
                    return SerialPort.GetPortNames();
                }
                catch (Exception ex)
                {
                    Logger.Instance.Log($"Fehler beim Abrufen der seriellen Ports: {ex.Message}", LogType.Fehler);
                    return Array.Empty<string>();
                }
            }
        }

        public EmpfangsConnector()
        {

            CreateCheckThread();

            Reconnect();

        }


        private void CreateCheckThread()
        {
            checkCancelationToken = new CancellationToken();
            this.CheckDataSendThread = new Thread(() =>
            {
                try
                {
                    while (true  && checkCancelationToken!=null && !checkCancelationToken.Value.IsCancellationRequested)
                    {
                        if (lastDataSendMS != -1 && DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastDataSendMS > 1000)
                        {
                            Logger.Instance.Log("Keine Daten in den letzten 1 Sekunden empfangen. Versuche, die Verbindung wiederherzustellen.", LogType.Warnung);
                            Reconnect();
                        }
                        Thread.Sleep(1000); // Überprüfe alle 1 Sekunde
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.Log($"Fehler im CheckDataSendThread: {ex.Message}", LogType.Fehler);
                }
            });
        }

        public void SerialErrorHandling(SerialErrorReceivedEventArgs? e)
        {
            try
            {
                if (e != null)
                {
                    Logger.Instance.Log($"Fehler bei Verbindung: {e.ToString()}", LogType.Fehler);

                }
                else
                {
                    Logger.Instance.Log("Fehler bei Verbindung: Keine Event-Informationen verfügbar.", LogType.Fehler);
                    

                }
                //Logger.Instance.Log($"Fehler bei Verbindung: {e.ToString()}", LogType.Fehler);
                Logger.Instance.Log("Reconnect....", LogType.Info);
                Reconnect();
            }catch(Exception ex)
            {
                Logger.Instance.Log($"Fehler beim SerialErrorHandling: {ex.Message}", LogType.Fehler);
            }
        }
        


        //Sicherheitsprüfung

        


        public void Reconnect()
        {
            try
            {

                CloseConnection();

                Thread.Sleep(300); // Kurze Pause, um sicherzustellen, dass der Port geschlossen ist

                if (!Connect())
                {
                    Logger.Instance.Log("Verbindung konnte nicht hergestellt werden.", LogType.Warnung);

                    return;
                }


                Logger.Instance.Log("EmpfangsConnector initialisiert.", LogType.Info);
                return;

            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Herstellen der Verbindung: {ex.Message}", LogType.Fehler);
                if (ConnectionChanged != null)
                {
                    ConnectionChanged.Invoke(false); // Event auslösen, wenn die Verbindung erfolgreich hergestellt wurde
                }
            }
        }

        private bool Connect()
        {
            try
            {
                if (serialPort !=null && serialPort.IsOpen)
                {
                    Logger.Instance.Log("Serielle Verbindung ist bereits geöffnet.", LogType.Info);
                    return false;
                }




                serialPort = new SerialPort();
                serialPort.DataReceived += SerialPort_DataReceived;
                serialPort.ErrorReceived += SerialPort_ErrorReceived;
                if (Port != null)
                {
                    serialPort.PortName = Port;
                    serialPort.BaudRate = 9600; // Standard-Baudrate, kann angepasst werden
                    serialPort.Open();
                }

                InitENQ();



                Logger.Instance.Log("EmpfangsConnector initialisiert.", LogType.Info);
                if(ConnectionChanged != null)
                {
                    ConnectionChanged.Invoke(true); // Event auslösen, wenn die Verbindung erfolgreich hergestellt wurde
                }

                return true;

            }
            catch (Exception ex)
            {
                CloseConnection();
                
                Logger.Instance.Log($"Fehler beim Herstellen der Verbindung: {ex.Message}", LogType.Fehler);
                return false;
            }
        }

        private void InitENQ()
        {
            enqThreadPause = false;

            enqCancelationToken = new CancellationToken();

            //Thread zum zyklischen senden von ENQ Nachrichten initialisieren
            ENQThread = new Thread(() =>
            {
                try
                {

                    while (true && enqCancelationToken != null && !enqCancelationToken.Value.IsCancellationRequested)
                    {
                        if (serialPort.IsOpen && !enqThreadPause)
                        {
                            writeENQ();
                        }
                        Thread.Sleep(300); // Sende alle 300ms ein ENQ

                    }
                }
                catch (Exception ex)
                {
                    SerialErrorHandling(null); // Beispiel für Fehlerbehandlung  
                    Logger.Instance.Log($"Fehler im ENQ-Thread: {ex.Message}", LogType.Fehler);

                }
            });

            ENQThread.Start();
        }

        private void PauseENQ()
        {
            enqThreadPause = true;
        }
        private void ContinueENQ()
        {
            enqThreadPause = false;
            if (ENQThread!=null &&  !ENQThread.IsAlive)
            {
                Logger.Instance.Log($"Fehler beim starten von ENQ. Thread beendet!", LogType.Fehler );

            }
        }
        public void OpenConnection(string port)
        {
            try
            {
                

                this.Port = port;

                Connect(); // Versuche, die Verbindung herzustellen
                             
               
                Logger.Instance.Log($"Serielle Verbindung zu {port} geöffnet.", LogType.Info);
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Öffnen der seriellen Verbindung: {ex.Message}", LogType.Fehler);
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            try
            {

                PauseENQ(); // Stoppe den ENQ-Thread, um Konflikte zu vermeiden
                this.lastDataSendMS = DateTimeOffset.Now.ToUnixTimeMilliseconds(); // Aktuellen Zeitstempel speichern

                HandleBuffer();
                // Beispiel: Datenverarbeitung

            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Verarbeiten der empfangenen Daten: {ex.Message}", LogType.Fehler);
            }
            ContinueENQ(); // Starte den Thread zum Senden von ENQ erneut

        }


        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            SerialErrorHandling(e);
            Logger.Instance.Log($"Fehler bei der seriellen Kommunikation: {e.EventType}", LogType.Fehler);
        }


        private List<byte> _buffer;
        private void HandleBuffer()
        {
            try
            {
                if (serialPort.BytesToRead > 0)
                {
                    byte[] bytes = new byte[serialPort.BytesToRead];
                    serialPort.Read(bytes, 0, bytes.Length);
                    if (_buffer == null)
                    {
                        _buffer = new List<byte>();
                    }
                    _buffer.AddRange(bytes);
                    Logger.Instance.Log($"Daten empfangen: {Encoding.UTF8.GetString(bytes)}", LogType.Info);

                   
                    HandleData();
                    

                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Lesen der seriellen Daten: {ex.Message}", LogType.Fehler);
            }   
        }


        private void HandleData()
        {
            try
            {
                if (_buffer != null && _buffer.Count >= 0)
                {


                    // Beispiel: Überprüfen auf STX und NAK (Flashen und STatus in der Original-App)
                    while( _buffer.Count > 0 && _buffer[0] != b_stx && _buffer[0] != b_nak)
                    {
                        _buffer.RemoveAt(0); // Entfernen von Bytes, die nicht STX oder NAK sind
                    }

                    if (_buffer.Count == 0)
                    {
                        Logger.Instance.Log("Puffer ist leer oder enthält keine gültigen Daten.", LogType.Warnung);
                        return;
                    }
                    else
                    {
                        if (_buffer[0] == b_nak)
                        {

                            //Kein Schuss
                            Logger.Instance.Log("Empfangene Daten sind ungültig (NAK empfangen).", LogType.Warnung);
                            _buffer.RemoveAt(0);
                            HandleSchuss(null);
                            //_buffer.Clear(); // Puffer leeren bei NAK
                            //return;
                        }
                        else if (_buffer[0] == b_stx && _buffer.Count>=59)
                        {
                            //Schuss
                            List<byte> schussDaten = _buffer.GetRange(0, 59);
                            _buffer.RemoveRange(0, 59);
                            writeACK();
                            HandleSchuss(schussDaten);
                            
                        }
                    }



                        // Hier können Sie die Logik zur Verarbeitung der Daten implementieren
                        // Beispiel: Konvertieren der Bytes in einen String und Protokollierung
                        string data = Encoding.UTF8.GetString(_buffer.ToArray());
                    Logger.Instance.Log($"Verarbeitete Daten: {data}", LogType.Info);
                    _buffer.Clear(); // Puffer leeren nach der Verarbeitung
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Verarbeiten der Daten: {ex.Message}", LogType.Fehler);
            }
        }


        private void HandleSchuss(List<byte> data)
        {
            try
            {
                if (data == null)
                {
                    Logger.Instance.Log("Kein gültiger Schuss empfangen (Daten sind null).", LogType.Warnung);

                    //Hier nochmal prüfen

                    //var leererSchuss = new Schuss(); // Leerer Schuss, um ein Event zu triggern 
                    //leererSchuss.Wert = 0;
                    //leererSchuss.Zeitstempel = DateTime.Now;
                    //if (SchussEmpfangen != null)
                    //{
                    //    Logger.Instance.Log("Leerer Schuss empfangen, Event wird ausgelöst.", LogType.Info);
                    //    SchussEmpfangen.Invoke(leererSchuss); // Event auslösen, um einen leeren Schuss zu signalisieren
                    //}
                    //else
                    //{
                    //    Logger.Instance.Log("SchussEmpfangen-Event ist nicht abonniert.", LogType.Warnung);
                    //}
                                                            
                    return;
                }

                if (data.Count != 59)
                {
                    Logger.Instance.Log($"Ungültige Schussdaten empfangen. Erwartet: 59 Bytes, Empfangen: {data.Count} Bytes.", LogType.Warnung);
                    return;
                }


                string strWert = Encoding.ASCII.GetString(data.GetRange(32, 4).ToArray());
                string strDistance = Encoding.ASCII.GetString(data.GetRange(37, 6).ToArray());
                string strX = Encoding.ASCII.GetString(data.GetRange(44, 5).ToArray());
                string StrY = Encoding.ASCII.GetString(data.GetRange(50, 5).ToArray());



                //Test:
                try
                {
                    string str;
                    str = Encoding.ASCII.GetString(data.ToArray());
                    Logger.Instance.Log($"Empfangene Schussdaten(Test): {str}", LogType.Info);
                }
                catch
                {
                    Logger.Instance.Log("Fehler beim Konvertieren der Schussdaten (Test).", LogType.Fehler);
                    return;
                }


                Schuss schuss = new Schuss();

                try
                {
                    schuss.X = int.Parse(strX);
                    schuss.Y = int.Parse(StrY);
                    schuss.Wert = double.Parse(strWert, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture);
                    schuss.Distanz = double.Parse(strDistance);
                    schuss.Zeitstempel = DateTime.Now;

                    //Hier:
                    //Loggen und Event
                    if (SchussEmpfangen != null)
                    {
                        Logger.Instance.Log("Schuss empfangen, Event wird ausgelöst.", LogType.Info);
                        SchussEmpfangen.Invoke(schuss); // Event auslösen, um einen leeren Schuss zu signalisieren
                    }
                    else
                    {
                        Logger.Instance.Log("SchussEmpfangen-Event ist nicht abonniert.", LogType.Warnung);
                    }
                }
                catch
                {
                    Logger.Instance.Log("Fehler beim Konvertieren der Schussdaten.", LogType.Fehler);
                    return;
                }

            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Verarbeiten des Schusses: {ex.Message}", LogType.Fehler);
            }
        }


        private void writeACK()
        {
            byte[] bytes = new byte[1] { b_ack };
            Write(bytes, "ack");
        }

        private void writeENQ()
        {
            byte[] bytes = new byte[1] { b_enq };
            Write(bytes, "enq");
        }

        private void Write(byte[] b, string info = "---")
        {
            try
            {
                if (serialPort != null)
                {
                    if (!serialPort.IsOpen)
                    {
                        Logger.Instance.Log("Serielle Verbindung ist nicht geöffnet.", LogType.Warnung);
                        return;
                    }
                    serialPort.Write(b, 0, b.Length);
                    Logger.Instance.Log($"Daten gesendet: {info} - {BitConverter.ToString(b)}", LogType.Info);
                }
                else
                {
                    Logger.Instance.Log("Serielle Verbindung ist null.", LogType.Fehler);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Schreiben in die serielle Verbindung: {ex.Message}", LogType.Fehler);
            }   
        }


        public void CloseConnection()
        {
            try
            {


                PauseENQ();
                try
                {
                    CancellationTokenSource cts = new CancellationTokenSource();
                    enqCancelationToken = cts.Token; // Setze den CancellationToken für den ENQ-Thread
                    cts.Cancel();
                }
                catch (Exception ex)
                {
                    Logger.Instance.Log($"Cancel ENQ-Thread nicht möglich {ex.Message}", LogType.Warnung);
                }

                

                if (serialPort != null)
                {
                    if (serialPort.IsOpen)
                    {
                        serialPort.Close();
                    }
                    serialPort.Dispose();
                    Logger.Instance.Log($"Serielle Verbindung zu {Port} geschlossen.", LogType.Info);
                }
                if (ConnectionChanged != null)
                {
                    ConnectionChanged.Invoke(false); // Event auslösen, wenn die Verbindung erfolgreich hergestellt wurde
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Schließen der seriellen Verbindung: {ex.Message}", LogType.Fehler);
            }
        }

        public void Dispose()
        {
            try
            {
                var cts = new CancellationTokenSource();
                checkCancelationToken = cts.Token; // Setze den CancellationToken für den ENQ-Thread
                cts.Cancel();

                CloseConnection();
            }catch(Exception ex)
            {

            }
            
        }
    }
}
