using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using RedDotMM.Empfaenger;
using RedDotMM.Logging;
using RedDotMM.Model;
using RedDotMM.Web;
using RedDotMM.Win.Data;
using RedDotMM.Win.UIHelper;



namespace RedDotMM.Win.Model
{
    public class Schießbahn : INotifyPropertyChanged
    {




        private static int _nextBahnennummer = 0; // Static variable to keep track of the next Bahnennummer

        private Empfaenger.Empfaenger empfaenger;

        private  RedDotMM_Context context = new RedDotMM_Context();

        private Webservice webservice = new Webservice();


        public event EventHandler<EventArgs>? UpdateRequested;

        public Schießbahn()
        {
            BahnenNummer = _nextBahnennummer++;
            Scheibe = new ScheibeViewModel(context);
            
            empfaenger = new Empfaenger.Empfaenger();
            empfaenger.ConnectionChanged += Empfaenger_ConnectionChanged;
            empfaenger.SchussEmpfangen += Empfaenger_SchussEmpfangen;
            OnPropertyChanged(nameof(IsConnected));
            OnPropertyChanged(nameof(Ports));
        }

        private void Empfaenger_ConnectionChanged(bool obj)
        {
            OnPropertyChanged(nameof(IsConnected));
        }

        private void Empfaenger_SchussEmpfangen(EmpfangenerSchuss schuss)
        {
            try
            {
                if (Scheibe != null)
                {
                    var count = Scheibe.Ergebnis.Schuesse.Count;

                    var s = new Schuss {
                        Wert  = (decimal)schuss.Wert,
                        X = schuss.X,
                        Y = schuss.Y,
                        Distanz = schuss.Distanz,
                        Zeitstempel = schuss.Zeitstempel,
                         LfdSchussNummer = count + 1 // Nächste laufende Nummer für den Schuss
                    } ;
                    Scheibe.AddSchuss(s);
                    // Aktualisiere die Anzeige
                    OnPropertyChanged(nameof(Scheibe));
                    OnPropertyChanged(nameof(AnzeigeName));
                    // Event auslösen, um die UI zu aktualisieren
                    OnUpdateRequested();

                }
            }catch(Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Empfangen eines Schusses: {ex.Message}", Logging.LogType.Fehler);
                MessageBox.Show($"Fehler beim Empfangen eines Schusses: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public string[] Ports
        {
            get
            {
                return empfaenger.Ports;
            }
        }

        private string selectedPort = null;
        public string SelectedPort
        {
            get
            {
                return selectedPort;
            }
            set
            {
                if (selectedPort != value)
                {
                    if (!empfaenger.IsConnected)
                    {
                        selectedPort = value;
                    }
                    OnPropertyChanged(nameof(SelectedPort));
                }
            }
        }

        public bool IsConnected
        {
            get
            {
                return empfaenger.IsConnected;
            }
        }


        #region Commands

        //Beispiel

        /*
         * public ICommand MyCommand => new RelayCommand(
    execute: () => { / Your logic here /
    },
    canExecute: () => true // Optional: Add condition for enabling/disabling the command
);
        */


        private RelayCommand _updatePortsCommand => new RelayCommand(
            execute: (T) =>
            {
                OnPropertyChanged(nameof(Ports));
            },
            canExecute: (T) => true);
        public RelayCommand UpdatePortsCommand
        {
            get
            {
                return _updatePortsCommand;
            }
        }

        private RelayCommand _connectEmpfaengerCommand => new RelayCommand(
            execute: (T) =>
            {
                empfaenger.OpenConnection(SelectedPort);
                OnPropertyChanged(nameof(IsConnected));
                MessageBox.Show(AnzeigeName + " verbunden.", "Verbindung", MessageBoxButton.OK, MessageBoxImage.Information);
            },
            canExecute: (T) => { return !empfaenger.IsConnected; });
        public RelayCommand ConnectEmpfaengerCommand
        {
            get
            {
                return _connectEmpfaengerCommand;
            }
        }


        private RelayCommand _disconnectEmpfaengerCommand => new RelayCommand(
           execute: (T) =>
           {
               empfaenger.CloseConnection();
               OnPropertyChanged(nameof(IsConnected));
           },
           canExecute: (T) => { return empfaenger.IsConnected; });
        public RelayCommand DisconnectEmpfaengerCommand
        {
            get
            {
                return _disconnectEmpfaengerCommand;
            }
        }



        private ICommand _StartCommand;
        public ICommand StartCommand
        {
            get
            {
                if (_StartCommand == null)
                {
                    _StartCommand = new RelayCommand(
                        execute: (T) =>
                        {
                            Start(T as Schuetze);
                        },
                        canExecute: (T) =>
                        {
                            if (T is Schuetze schuetze)
                            {
                                return schuetze != null;
                            }
                            return false;
                        });
                }
                return _StartCommand;
            }
        }


        private ICommand _ProbeWechselnCommand;
        public ICommand ProbeWechselnCommand
        {
            get
            {
                if (_ProbeWechselnCommand == null)
                {
                    _ProbeWechselnCommand = new RelayCommand(
                        execute: (T) =>
                        {
                            if (Scheibe != null)
                            {
                                Scheibe.Probe = !Scheibe.Probe;
                                OnUpdateRequested();
                            }
                        },
                        canExecute: (T) => { return Scheibe != null; });
                }
                return _ProbeWechselnCommand;
            }
        }

        private ICommand _FehlschussCommand;

        public ICommand FehlschussCommand
        {
            get
            {
                if (_FehlschussCommand == null)
                {
                    _FehlschussCommand = new RelayCommand(
                        execute: (T) =>
                        {
                            if (Scheibe != null)
                            {
                                var count = Scheibe.Ergebnis.Schuesse.Count;

                                Scheibe.AddSchuss(new Schuss
                                {
                                    Wert = 0,
                                    X = -5000,
                                    Y = -3500 + ((count ) * 200),
                                    Distanz = 0
                                });
                                OnUpdateRequested();

                            }
                        },
                        canExecute: (T) => { return Scheibe != null && Scheibe.Ergebnis != null; });
                }
                return _FehlschussCommand;
            }
        }


        private ICommand _ScheibeAuffüllenCommand;

        public ICommand ScheibeAuffüllenCommand
        {
            get
            {
                if (_ScheibeAuffüllenCommand == null)
                {
                    _ScheibeAuffüllenCommand = new RelayCommand(
                        execute: (T) =>
                        {
                            if (Scheibe != null && Scheibe.Ergebnis!=null)
                            {
                                var count = Scheibe.Ergebnis.Schuesse.Count;
                                var soll = Scheibe.Ergebnis.Serie.Schuetze.Wettbewerb.AnzahlWertungsSchuss;
                                                            

                                if (count < soll)
                                {

                                    if(MessageBox.Show($"Es sollen {soll} Schüsse auf der Scheibe sein. Es sind aktuell {count} Schüsse eingetragen. Möchten Sie die Scheibe auffüllen?", "Scheibe auffüllen", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                                    {
                                        return;
                                    }
                                    Scheibe.Probe = false; // Setze Probe auf false, da wir nur Wertungsschüsse auffüllen wollen
                                    for (int i = count; i < soll; i++)
                                    {
                                        Scheibe.AddSchuss(new Schuss
                                        {
                                            Wert = 0,
                                            X = -5000,
                                            Y = -3500 + ((count+i)*200) ,
                                            Distanz = 0
                                        });
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Die Scheibe ist bereits vollständig.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                                OnUpdateRequested();
                            }
                        },
                        canExecute: (T) => { return Scheibe != null && Scheibe.Ergebnis != null; });
                }
                return _ScheibeAuffüllenCommand;
            }
        }



        #endregion Commands








        private int _bahnennummer;
        public int BahnenNummer
        {
            get => _bahnennummer;
            set
            {
                if (_bahnennummer != value)
                {
                    _bahnennummer = value;
                    OnPropertyChanged(nameof(BahnenNummer));
                }
            }
        }

        public string AnzeigeName
        {
            get
            {

                string? schuetze = Scheibe?.Ergebnis?.Serie.Schuetze.AnzeigeName;

                if (string.IsNullOrEmpty(schuetze))
                {

                    return $"Bahn {BahnenNummer}";
                }
                else
                {
                    if (BahnenNummer <= 1)
                    {
                        return $"{schuetze}";
                    }
                    return $"Bahn {BahnenNummer} - {schuetze}";
                }
            }
        }






        private ScheibeViewModel _scheibe;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ScheibeViewModel Scheibe
        {
            get => _scheibe;
            set
            {
                if (_scheibe != value)
                {
                    _scheibe = value;
                    OnPropertyChanged(nameof(Scheibe));
                    OnUpdateRequested();
                }
            }
        }



        #region Webservice


        private int _WebPort = 7070;
        public int WebPort
        {
            get
            {
                return _WebPort;
            }
            set
            {
                if (_WebPort != value)
                {
                    _WebPort = value;
                    OnPropertyChanged(nameof(WebPort));
                    OnPropertyChanged(nameof(WebURLs));
                }
            }
        }

        public string[] WebURLs
        {
            get
            {
                return webservice.GetURLs(WebPort);
            }
        }

        private string _WebURL;
        public string WebURL
        {
            get
            {
                return _WebURL;
            }
            set
            {
                if (_WebURL != value)
                {
                    _WebURL = value;
                    OnPropertyChanged(nameof(WebURL));
                }
            }
        }

        public void StartWebservice()
        {
            try
            {
                webservice.StartWebserviceAsync(WebURL);
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Starten des Webservice: {ex.Message}", Logging.LogType.Fehler);
                MessageBox.Show($"Fehler beim Starten des Webservice: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private ICommand _StartWebserviceCommand;
        public ICommand StartWebserviceCommand
        {
            get
            {
                if (_StartWebserviceCommand == null)
                {
                    _StartWebserviceCommand = new RelayCommand(
                        execute: (T) =>
                        {
                            StartWebservice();
                        },
                        canExecute: (T) => { return webservice!=null; });
                }
                return _StartWebserviceCommand;
            }
        }



        private ICommand _SetFirewallCommand;
        public ICommand SetFirewallCommand
        {
            get
            {
                if (_SetFirewallCommand == null)
                {
                    _SetFirewallCommand = new RelayCommand(
                        execute: (T) =>
                        {
                            try
                            {
                                webservice.SetFirewall(WebURL);
                                MessageBox.Show("Firewall-Regeln wurden gesetzt.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            catch (Exception ex)
                            {
                                Logger.Instance.Log($"Fehler beim Setzen der Firewall-Regeln: {ex.Message}", Logging.LogType.Fehler);
                                MessageBox.Show($"Fehler beim Setzen der Firewall-Regeln: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        },
                        canExecute: (T) => { return webservice != null; });
                }
                return _SetFirewallCommand;
            }
        }


        private ICommand _RemoveFirewallCommand;
        public ICommand RemoveFirewallCommand
        {
            get
            {
                if (_RemoveFirewallCommand == null)
                {
                    _RemoveFirewallCommand = new RelayCommand(
                        execute: (T) =>
                        {
                            try
                            {
                                webservice.RemoveFirewall(WebURL);
                                MessageBox.Show("Firewall-Regeln wurden entfernt.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            catch (Exception ex)
                            {
                                Logger.Instance.Log($"Fehler beim Entfernen der Firewall-Regeln: {ex.Message}", Logging.LogType.Fehler);
                                MessageBox.Show($"Fehler beim Entfernen der Firewall-Regeln: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        },
                        canExecute: (T) => { return webservice != null; });
                }
                return _RemoveFirewallCommand;
            }
        }


        #endregion Webservice




        protected void OnUpdateRequested()
        {

            try
            {
                if (webservice != null && Scheibe.Ergebnis!=null)
                {

                    webservice.Ergebnis = Scheibe.Ergebnis;                   


                    var canv = new Canvas
                    {
                        Width = 1000,
                        Height = 1000
                    };
                    canv.UpdateLayout();
                    UIHelper.Scheibenzeichner.ZeichneScheibe(canv, Scheibe.Ergebnis, Scheibe.Probe, Scheibe.Ergebnis.Serie.Schuetze.Wettbewerb.Teilerwertung);

                    byte[] b = UIHelper.Scheibenzeichner.getImage(canv);
                    webservice.ScheibenBild = b;

                    
                }


            }
            catch(Exception ex)
            {

            }



            UpdateRequested?.Invoke(this, EventArgs.Empty);
        }




        protected virtual void OnPropertyChanged(string propertyName)
        {

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



        public void Start(Schuetze schuetze)
        {
            try
            {

                if (schuetze == null)
                {
                    MessageBox.Show("Kein Schütze ausgewählt.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                //Prüfen, ob aktuell Bahn belegt und/oder fertig
                if (Scheibe != null)
                {
                    if (Scheibe.Ergebnis != null)
                    {
                        if (!Scheibe.Ergebnis.ErgebnisAbgeschlossen)
                        {
                            if (MessageBox.Show("Die aktuelle Scheibe ist noch nicht abgeschlossen. Möchten Sie die Serie trotzdem starten?", "Serie starten", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                            {
                                return;
                            }
                        }
                    }
                }

                //Datencontext neu laden
                this.context.Dispose();
                this.context = new RedDotMM_Context();
                this.Scheibe.context = this.context;

                Guid ErgID = Guid.Empty;

                Guid SerienID = Guid.Empty;

                //offene Ergebnisse
                var offeneErg = context.Ergebnisse
                    .Where(e => e.Serie.Schuetze.SchuetzenId == schuetze.SchuetzenId && !e.ErgebnisAbgeschlossen)
                    .FirstOrDefault();


                if (offeneErg != null)
                {
                    ErgID = offeneErg.ErgebnisId;
                    SerienID = offeneErg.SerienID;
                    MessageBox.Show($"Es gibt noch ein offenes Ergebnis für {schuetze.AnzeigeName}. Dies wird jetzt fortgeführt", "Offenes Ergebnis", MessageBoxButton.OK, MessageBoxImage.Information);

                }
                else
                {

                    //Prüfen, ob es noch offene Serien gibt.
                    var offeneSerie = context.Serien
                        .Where(s => s.Schuetze.SchuetzenId == schuetze.SchuetzenId && !s.SerieAbgeschlossen)
                        .FirstOrDefault();
                    if (offeneSerie == null)
                    {
                        var neueSerie = new Serie
                        {
                            SchuetzeID = schuetze.SchuetzenId,
                            SchussgeldBezahlt = false,
                            Ergebnisse = new List<Ergebnis>(),
                            SerieAbgeschlossen = false


                        };
                        context.Serien.Add(neueSerie);
                        context.SaveChanges();
                        SerienID = neueSerie.SerienId;
                    }
                    else
                    {
                        MessageBox.Show($"Es gibt noch eine offene Serie für {schuetze.AnzeigeName}. Diese wird jetzt fortgeführt", "Offene Serie", MessageBoxButton.OK, MessageBoxImage.Information);

                        SerienID = offeneSerie.SerienId;
                    }
                }

                if (SerienID == Guid.Empty)
                {
                    Logger.Instance.Log("Es konnte keine Serie gefunden werden.", Logging.LogType.Fehler);
                    MessageBox.Show("Es konnte keine Seriegefunden werden. Bitte überprüfen Sie die Datenbank.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                int lfd = 0;
                if (context.Ergebnisse.Count() > 0)
                {
                    lfd = context.Ergebnisse.Max(e=>e.LfdNummer);
                }

                //Falls es kein Offenes Ergebnis gibt, neues erstellen.
                if (ErgID == Guid.Empty)
                {
                    var erg = new Ergebnis
                    {
                        ErgebnisAbgeschlossen = false,
                        SerienID = SerienID,
                        LfdNummer = lfd + 1 // Nächste laufende Nummer
                    };
                    context.Ergebnisse.Add(erg);
                    context.SaveChanges();
                    ErgID = erg.ErgebnisId;
                }




                //Setze Scheibe.
                if (ErgID != Guid.Empty)
                {
                   

                    this.Scheibe.Ergebnis = context.Ergebnisse.Where(e => e.ErgebnisId == ErgID)
                   .Include(e => e.Serie)
                   .ThenInclude(s => s.Schuetze)
                   .ThenInclude(s2 => s2.Wettbewerb)
                   .Include(e => e.Schuesse)                   
                   .FirstOrDefault();


                    //Daten aus der Datenbank aktualisieren um sicherzustellen, dass die aktuellen Werte genutzt werden



                    if (this.Scheibe.Ergebnis == null)
                    {
                        Logger.Instance.Log($"Ergebnis mit ID {ErgID} konnte nicht gefunden werden.", Logging.LogType.Fehler);
                        MessageBox.Show("Das Ergebnis konnte nicht gefunden werden. Bitte überprüfen Sie die Datenbank.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);

                        return;
                    }

                    var SollanzProbe = this.Scheibe.Ergebnis.Serie.Schuetze.Wettbewerb.AnzahlProbeschuss;

                    var ProbeSchuss = this.Scheibe.Ergebnis.Schuesse
                        .Where(s => s.Typ == SchussTyp.Probe)
                        .Count();

                    //Prüfen, ob Wertungsschüsse oder Probeschüsse geschossen werden sollen
                    

                    //Prüfen ob Probe oder Wertungsschuss (basieren auf nummer der Scheibe in Serie)
                    if (this.Scheibe.Ergebnis.Serie.Schuetze.Wettbewerb.ProbeNurAufErsterScheibe && this.Scheibe.Ergebnis.Serie.Ergebnisse.Count > 1)
                    {
                        this.Scheibe.Probe = false; // Wenn ProbeNurAufErsterScheibe true ist, dann nur auf der ersten Scheibe
                    }
                    else
                    {
                        this.Scheibe.Probe = ProbeSchuss < SollanzProbe; // Wenn weniger als SollanzProbe, dann Probe
                    }





                    }
                OnPropertyChanged(nameof(Scheibe));
                OnPropertyChanged(nameof(AnzeigeName));
                OnPropertyChanged(nameof(BahnenNummer));
                OnUpdateRequested();


            }
            catch
            {
                Logging.Logger.Instance.Log($"Fehler beim Starten der Serie auf Bahn {BahnenNummer}.", Logging.LogType.Fehler);
                MessageBox.Show("Fehler beim Starten der Serie. Bitte überprüfen Sie die Verbindung zum Empfänger.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }




        }


    }



}

