using RedDotMM.Logging;
using RedDotMM.Model;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedDotMM.HandHeld
{
    public class ArduinoNanoHandHeld
    {

        //Daten, wie sie empfangen werden:

        /*
         * if (Serial.available() > 0) {
        name = Serial.readStringUntil(',');
        points = Serial.readStringUntil(',').toInt();
        number = Serial.readStringUntil(',').toInt();
        pw = Serial.readStringUntil(',').toFloat();
        updateDisplay();
        }

        if (digitalRead(buttonPin1) == LOW) {
        Serial.println("Button.1");
        }
        if (digitalRead(buttonPin2) == LOW) {
        Serial.println("Button.2");
        }
        if (digitalRead(buttonPin3) == LOW) {
        Serial.println("Button.3");
        }
         * 
         */

        private SerialPort SerialPort { get; set; }

        public event Action<int> ButtonPressed;
        public ArduinoNanoHandHeld()
        {
            // Initialisierungscode hier, falls nötig
        }

        public void OpenPort(string portName, int baudRate = 9600)
        {
            ClosePort(); // Sicherstellen, dass der Port geschlossen ist, bevor ein neuer geöffnet wird

            SerialPort = new SerialPort(portName, baudRate);
            SerialPort.Open();
            SerialPort.DataReceived += SerialPort_DataReceived;
        }


        //Timestamp, wann letzte Rückmeldung erfolge
        private long lastResponseTime; //milliseconds
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (SerialPort != null && SerialPort.IsOpen)
                {
                    string data = SerialPort.ReadLine();
                    Logger.Instance.Log($"Handheld Daten Empfangen: {data}", LogType.Info);

                    if (data.StartsWith("Button."))
                    {
                        if (ButtonPressed != null)
                        {
                            // Extrahiere die Nummer des gedrückten Buttons
                            string buttonNumberStr = data.Split('.')[1];
                            if (int.TryParse(buttonNumberStr, out int buttonNumber))
                            {
                                ButtonPressed.Invoke(buttonNumber);
                            }
                        }
                        lastResponseTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(); // Aktuellen Zeitstempel speichern
                    }
                    if (data.StartsWith("ACK"))
                    {
                        lastResponseTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(); // Aktuellen Zeitstempel speichern

                    }


                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Empfangen von Handheld-Daten: {ex.Message}", LogType.Fehler);
            }
        }

        public void ClosePort()
        {
            try
            {
                if (SerialPort != null)
                {
                    SerialPort.DataReceived -= SerialPort_DataReceived;
                    if (SerialPort.IsOpen)
                    {
                        SerialPort.Close();
                    }
                    SerialPort.Dispose();
                }
            }catch(Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Schließen des Handheld-Ports: {ex.Message}", LogType.Fehler);
            }
            finally
            {
                SerialPort = null;
            }


        }

        public void SendData(string name, float points, int number, string pw)
        {
            if (SerialPort != null && SerialPort.IsOpen)
            {
                string data = $"{name},{points},{number},{pw},";
                List<byte> bytes = new List<byte>(Encoding.UTF8.GetBytes(data));
                bytes.Insert(0, 2); // Füge ein Startzeichen hinzu
                SerialPort.WriteLine(data);
            }
            else
            {
                throw new InvalidOperationException("Serial port is not open.");
            }
        }


        public bool IsOpen
        {
            get { return SerialPort != null && SerialPort.IsOpen; }
        }

        public string ReadData()
        {
            if (SerialPort != null && SerialPort.IsOpen)
            {
                return SerialPort.ReadLine();
            }
            else
            {
                throw new InvalidOperationException("Serial port is not open.");
            }
        }



    }
}
