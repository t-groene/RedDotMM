using RedDotMM.Empfaenger;
using RedDotMM.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedDotMM.Empfaenger
{




   

    public class Empfaenger : IDisposable
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

        public event Action<EmpfangenerSchuss> SchussEmpfangen;

        public event Action<bool> ConnectionChanged;

        private long lastDataSendMS = -1;

        /// <summary>
        /// Gibt eine Liste mit den Verfügbaren seriellen Schnittstellen (z.B. COM1, COM2, etc.) zurück.
        /// </summary>
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

        public Empfaenger()
        {

            CreateCheckThread();

            //Reconnect();

        }



        /// <summary>
        /// Initialisiert einen Thread, der die Serielle SChnittstelle überwacht.
        /// </summary>
        private void CreateCheckThread()
        {
           
            this.CheckDataSendThread = new Thread(() =>
            {
                try
                {
                    while (true  && checkCancelationToken!=null && !checkCancelationToken.Value.IsCancellationRequested)
                    {
                        if (lastDataSendMS != -1 && DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastDataSendMS > 3000)
                        {
                            Logger.Instance.Log("Keine Daten in den letzten 3 Sekunden empfangen. Versuche, die Verbindung wiederherzustellen.", LogType.Warnung);
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

        /// <summary>
        /// Behandelt serielle Fehlerereignisse und versucht, die Verbindung wiederherzustellen.
        /// </summary>
        /// <param name="e"></param>

        private void SerialErrorHandling(SerialErrorReceivedEventArgs? e)
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
        


       

        
        /// <summary>
        /// Stellt die Serielle VErbindung her. Wenn sie bereits besteht, wird sie vorher geschlossen.
        /// </summary>

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

        /// <summary>
        /// Versucht, eine serielle Verbindung herzustellen. Wenn die Verbindung bereits besteht, wird sie nicht erneut geöffnet.
        /// </summary>
        /// <returns></returns>
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


        /// <summary>
        /// Initialisiert den Thread zum Abrufen der Daten vom Empfänger
        /// </summary>
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
                        if (serialPort.IsOpen && !enqThreadPause) // nur Senden, wenn der Port offen ist und nicht gerade daten VErarbeitet werden.
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

        /// <summary>
        /// Öffnet eine serielle Verbindung zu dem angegebenen Port. Wenn die Verbindung bereits besteht, wird sie nicht erneut geöffnet.
        /// </summary>
        /// <param name="port">Name der Seriellen schnittstelle (z.B. COM1), die geöffnet werden soll.</param>
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

                PauseENQ(); // Pausiere den ENQ-Thread, um Konflikte zu vermeiden
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



        /// <summary>
        /// Puffer für empfangene Daten. Wird verwendet, um die empfangenen Bytes zu speichern, bis sie verarbeitet werden können.
        /// </summary>
        private List<byte> _buffer;

        /// <summary>
        /// Ließt die Daten aus der Schnittstelle in den Puffer und tiggert die Verarbeitung.
        /// </summary>
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



        /// <summary>
        /// Verarbeitet die Daten.
        /// </summary>
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
                        if (_buffer[0] == b_nak) //Keine DAten.
                        {

                            //Kein Schuss
                            Logger.Instance.Log("Empfangene Daten sind ungültig (NAK empfangen).", LogType.Warnung);
                            _buffer.RemoveAt(0);
                            //HandleSchuss(null);
                            //_buffer.Clear(); // Puffer leeren bei NAK
                            //return;
                        }
                        else if (_buffer[0] == b_stx && _buffer.Count>=59)
                        {
                            //Schuss
                            List<byte> schussDaten = _buffer.GetRange(0, _buffer.Count);
                           
                           if(HandleSchuss(schussDaten))
                            {
                                _buffer.RemoveRange(0, 59);
                                writeACK();
                            }
                            
                        }
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Verarbeiten der Daten: {ex.Message}", LogType.Fehler);
            }
        }

        /// <summary>
        /// Verarbeitet die Schussdaten.
        /// </summary>
        /// <param name="data">Daten des Schuss (Bytefolge mit >=59byte</param>
        /// <returns>true, wenn die Daten verarbeitet wurden und aus dem Puffer gelöscht werden dürfen.</returns>
        private bool HandleSchuss(List<byte> data)
        {
            try
            {
                if (data == null)
                {

                    return false;
                }

                if (data.Count != 59)
                {
                    Logger.Instance.Log($"Ungültige Schussdaten empfangen. Erwartet: 59 Bytes, Empfangen: {data.Count} Bytes.", LogType.Warnung);
                    return false;
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
                    return false;
                }


                EmpfangenerSchuss schuss = new EmpfangenerSchuss();

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
                   
                }

                //Schuss wurde verarbeitet und kann aus dem Puffer entfernt werden.
                return true;


            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Verarbeiten des Schusses: {ex.Message}", LogType.Fehler);
                return true; //Schuss wurde verarbeitet, auch wenn es ein Fehler gab.
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


        /// <summary>
        /// Gibt zurück, ob eine Serielle VErbindung aufgebaut ist (true) oder nicht (false)
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return (serialPort != null && serialPort.IsOpen);
            }
        }

        public void CloseConnection()
        {
            try
            {
                Logger.Instance.Log("Verbindung wird getrennt...", LogType.Info);

                PauseENQ();

                lastDataSendMS = -1; // Setze den Zeitstempel für den letzten empfangenen Schuss zurück
                try
                {
                    Logger.Instance.Log("Verbindung wird getrennt... Cancel ENQ", LogType.Info);

                    var ctsENQ = new CancellationTokenSource();
                    enqCancelationToken = ctsENQ.Token; // Setze den CancellationToken für den ENQ-Thread
                    ctsENQ.Cancel();
                }
                catch (Exception ex)
                {
                    Logger.Instance.Log($"Cancel ENQ-Thread nicht möglich {ex.Message}", LogType.Warnung);
                }

                lastDataSendMS = -1; // Setze den Zeitstempel für den letzten empfangenen Schuss zurück


                try
                {
                    Logger.Instance.Log("Verbindung wird getrennt... Cancel Check", LogType.Info);

                    var ctsCheck = new CancellationTokenSource();
                    checkCancelationToken = ctsCheck.Token; // Setze den CancellationToken für den Check-Thread
                    ctsCheck.Cancel();
                    
                }catch(Exception ex)
                {
                    Logger.Instance.Log($"Cancel Check-Thread nicht möglich {ex.Message}", LogType.Warnung);
                }



                if (serialPort != null)
                {
                    Logger.Instance.Log("Verbindung wird getrennt... Close Serial", LogType.Info);

                    if (serialPort.IsOpen)
                    {


                        serialPort.DiscardInBuffer();
                        serialPort.DiscardOutBuffer();


                        serialPort.Close();
                    }
                    serialPort.Dispose();
                    Logger.Instance.Log($"Serielle Verbindung zu {Port} geschlossen.", LogType.Info);
                }
                if (ConnectionChanged != null)
                {
                    ConnectionChanged.Invoke(false); // Event auslösen, wenn die Verbindung erfolgreich hergestellt wurde
                }
                Logger.Instance.Log("Verbindung wurde getrennt!", LogType.Info);

                lastDataSendMS = -1; // Setze den Zeitstempel für den letzten empfangenen Schuss zurück

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
               

                CloseConnection();
            }catch(Exception ex)
            {

            }
            
        }
    }
}
