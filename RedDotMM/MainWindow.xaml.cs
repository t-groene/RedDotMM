using RedDotMM.Data;
using RedDotMM.HandHeld;
using RedDotMM.Helper;
using RedDotMM.Logging;
using RedDotMM.Model;
using RedDotMM.Model.NoDB;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RedDotMM.CommonHelper;
using System.Windows.Threading;

namespace RedDotMM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow, INotifyPropertyChanged

    {

        private EmpfangsConnector.EmpfangsConnector empfangsConnector;

     

        public event PropertyChangedEventHandler? PropertyChanged;

        private RedDotMMContext _context;

        private ArduinoNanoHandHeld HandHeld { get; set; }

        public MainWindow()
        {
            
            InitializeComponent();
            this.DataContext = this;

            Context = new RedDotMMContext();
            Context.Database.EnsureCreated();

             Context.Database.MigrateAsync();

            empfangsConnector = new EmpfangsConnector.EmpfangsConnector();
          

            empfangsConnector.SchussEmpfangen += EmpfangsConnector_SchussEmpfangen;
            empfangsConnector.ConnectionChanged += EmpfangsConnector_ConnectionChanged;

            UpdateBild();

            RefreshPorts();
            //this.Scheibe.ZeichneScheibe();
        }

        private void Notify(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public RedDotMMContext Context
        {
            get { return _context; }
            private set
            {
                if (_context != value)
                {
                    _context = value;
                    Notify(nameof(Context));
                }
            }
        }

        public List<Wettbewerb> Wettbewerbe
        {
            get
            {
                return Context.Wettbewerbe.ToList();
            }
        }

        private string _SchuetzenFilter = null;
        public string SchuetzenFilter
        {
            get => _SchuetzenFilter;
            set
            {
                if (_SchuetzenFilter != value)
                {
                    _SchuetzenFilter = value;
                    Notify(nameof(SchuetzenFilter));
                    Notify(nameof(Schuetzen));
                }
            }
        }
        public List<Schuetze> Schuetzen
        {
            get
            {
                if (SelectedWettbewerb == null)
                {
                    return null;
                }
                else
                {
                    if (!string.IsNullOrEmpty(SchuetzenFilter))
                    {
                        return Context.Schuetzen.Where(s => s.WettbewerbID == SelectedWettbewerb.Guid && s.Name.StartsWith(SchuetzenFilter)).OrderBy(s => s.Name).ToList();
                    }
                    return Context.Schuetzen.Where(s => s.WettbewerbID == SelectedWettbewerb.Guid).OrderBy(s=>s.Name).ToList();
                }
            }
        }

        public List<Ergebnis> Ergebnisse
        {
            get
            {
                if (SelectedSchuetze == null)
                {
                    return null;
                }
                else
                {
                    return Context.Ergebnisse.Where(e => e.Serie.SchuetzeID == SelectedSchuetze.SchuetzenId).Include(s=>s.Schuesse).ToList();
                }
            }
        }

        public List<AuswertungsItem> Auswertung
        {
            get
            {
                try
                {
                    if (SelectedWettbewerb == null)
                    {
                        return null;
                    }
                    return Helper.Auswerter.GetAuswertung(Context, SelectedWettbewerb?.Guid ?? Guid.Empty);
                }
                catch (Exception ex)
                {
                    Logger.Instance.Log($"Fehler bei der Auswertung: {ex.Message}", LogType.Fehler);
                    return null;
                }

            }
        }


        

        private void EmpfangsConnector_ConnectionChanged(bool obj)
        {
            Connected = obj;
        }

       

        #region BindingProperties

        public List<string> Ports { get; set; } = new List<string>();

        private string _selectedPort;
        public string SelectedPort
        {
            get => _selectedPort;
            set
            {
                if (_selectedPort != value)
                {
                    _selectedPort = value;
                    Notify(nameof(SelectedPort));
                }
            }
        }

        private bool _connected;
        public bool Connected
        {
            get => _connected;
            set
            {
                if (_connected != value)
                {
                    _connected = value;
                    Notify(nameof(Connected));
                }
            }
        }

        private bool _probe;
        public bool Probe
        {
            get => _probe;
            set
            {
                if (_probe != value)
                {
                    _probe = value;
                    this.Scheibe.Probe = _probe;
                    Notify(nameof(Probe));
                }
            }
        }

        private Wettbewerb? _selectedWettbewerb;
        public Wettbewerb? SelectedWettbewerb
        {
            get => _selectedWettbewerb;
            set
            {
                if (_selectedWettbewerb != value)
                {
                    _selectedWettbewerb = value;
                    _SchuetzenFilter = null;
                    Notify(nameof(SchuetzenFilter));
                    Notify(nameof(SelectedWettbewerb));
                    Notify(nameof(Schuetzen));
                    Notify(nameof(Auswertung));
                    SelectedSchuetze = null;
                }
            }
        }

        public Schuetze? _selectedSchuetze;
        public Schuetze? SelectedSchuetze
        {
            get => _selectedSchuetze;
            set
            {
                if (_selectedSchuetze != value)
                {
                    _selectedSchuetze = value;
                    Notify(nameof(SelectedSchuetze));
                    Ergebnis = null; // Ergebnis zurücksetzen, wenn Schütze geändert wird

                    if (_selectedSchuetze != null && _selectedWettbewerb != null)
                    {

                        //Ergebnis finden
                        var erg = _context.Ergebnisse.Where(e =>
                            e.Serie.SchuetzeID == _selectedSchuetze.SchuetzenId &&
                            e.ErgebnisAbgeschlossen == false)
                            .FirstOrDefault();
                        if (erg == null)
                        {
                            NeuesErgebnis();
                        }
                        else
                        {
                            Ergebnis = erg; //Ergebnis setzen, wenn gefunden
                        }

                    }
                    else
                    {
                        Ergebnis = null;
                    }
                    Notify(nameof(Ergebnisse));
                }
            }
        }

        public Ergebnis _ergebnis;
        public Ergebnis Ergebnis
        {
            get => _ergebnis;
            set
            {
                if (_ergebnis != value)
                {
                    _ergebnis = value;


                    if (_ergebnis != null)
                    {

                        if (_ergebnis.Schuetze == null)
                        {
                            _context.Entry(_ergebnis).Reference(e => e.Schuetze).Load();
                        }
                        if (_ergebnis.Schuetze != null && _ergebnis.Schuetze.Wettbewerb == null)
                        {
                            _context.Entry(_ergebnis.Schuetze).Reference(s => s.Wettbewerb).Load(); //Wettbewerb aus DB nachladen
                        }

                        //Schüsse laden
                        _context.Entry(_ergebnis).Collection(e => e.Schuesse).Load();

                        if (_ergebnis.Schuetze == null || _ergebnis.Schuetze.Wettbewerb == null)
                        {
                            MessageBox.Show("Ergebnis hat keinen zugeordneten Wettbewerb oder Schützen. Bitte überprüfen Sie die Daten.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                            _ergebnis = null;
                            return;

                        }


                        //Probeschuss festlegen
                        if (_ergebnis.Schuetze.Wettbewerb.Probeschuss > 0 && _ergebnis.Schuesse.Where(s => s.Typ == SchussTyp.Probe).Count() < _ergebnis.Schuetze.Wettbewerb.Probeschuss)
                        {
                            Probe = true;
                        }
                        else
                        {
                            Probe = false;
                        }
                    }
                    //Scheibe aktualisieren
                    this.Scheibe.ergebnis = _ergebnis;
                    UpdateBild();

                    //An HandHeld senden
                    if(HandHeld != null && HandHeld.IsOpen)
                    {
                        WriteToHandHeld();
                    }

                    Notify(nameof(Ergebnis));
                    Notify(nameof(Auswertung));


                }
            }
        }


        #endregion //BindingProperties

        /// <summary>
        /// Speichert, sofern vorhanden das exisitierende Ergebnis uns erstellt ein neues.
        /// </summary>
        private void NeuesErgebnis()
        {
            try
            {
                if (Ergebnis != null)
                {
                    if (_context.Entry(Ergebnis).State == EntityState.Detached)
                    {
                        _context.Ergebnisse.Add(Ergebnis); //Falls Ergebnis noch nicht in der DB, hinzufügen
                    }
                    else
                    {
                        _context.Update(Ergebnis); //Ansonsten aktualisieren
                    }
                    _context.SaveChanges();
                }
                if (SelectedWettbewerb == null || SelectedSchuetze == null)
                {
                    Logger.Instance.Log("Kein Wettbewerb oder Schütze ausgewählt, Ergebnis kann nicht erstellt werden.", LogType.Fehler);
                    MessageBox.Show("Kein Wettbewerb oder Schütze ausgewählt, Ergebnis kann nicht erstellt werden.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (SelectedSchuetze.Ergebnisse == null || SelectedSchuetze.Ergebnisse.Count == 0)
                {
                    _context.Entry(SelectedSchuetze).Collection(s => s.Ergebnisse).Load();
                }

                //hier noch Serie laden

                Ergebnis = new Ergebnis
                {
                    //Schuetze = SelectedSchuetze,
                    Zeitstempel = DateTime.Now,
                    BezahltesSchussGeld = 0,
                    Schuesse = new List<Schuss>(),
                    LfdNummer = SelectedSchuetze.Ergebnisse.Count + 1 // Laufende Nummer basierend auf der Anzahl der bisherigen Ergebnisse des Schützen
                };

            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Erstellen eines neuen Ergebnisses: {ex.Message}", LogType.Fehler);
                MessageBox.Show($"Fehler beim Erstellen eines neuen Ergebnisses: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshPorts()
        {
            try
            {
                if (empfangsConnector != null)
                {
                    this.Ports.Clear();
                    this.Ports.AddRange(empfangsConnector.Ports);
                    Notify(nameof(Ports));
                }
            }
            catch
            {
                Logger.Instance.Log("Fehler beim Aktualisieren der Ports.", LogType.Fehler);
                MessageBox.Show("Fehler beim Aktualisieren der Ports.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateBild()
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    this.Scheibe.ZeichneScheibe();
                });


            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Starten des Zeichnen-Threads: {ex.Message}", LogType.Fehler);
            }
        }



        private void EmpfangsConnector_SchussEmpfangen(Schuss schuss)
        {
            if (schuss == null)
            {
                Logger.Instance.Log("Empfangener Schuss ist null.", LogType.Fehler);
                MessageBox.Show("Empfangener Schuss ist null.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (SelectedSchuetze == null)
            {
                Logger.Instance.Log("Kein Schütze ausgewählt, Schuss kann nicht hinzugefügt werden.", LogType.Fehler);
                MessageBox.Show("Kein Schütze ausgewählt, Schuss kann nicht hinzugefügt werden.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }



            if (SelectedSchuetze.Wettbewerb == null)
            {
                _context.Entry(SelectedSchuetze).Reference(s => s.Wettbewerb).Load(); //Wettbewerb aus DB nachladen
            }

            if (SelectedSchuetze.Wettbewerb == null)
            {
                Logger.Instance.Log("Wettbewerb des Schützen ist null, Schuss kann nicht hinzugefügt werden.", LogType.Fehler);
                MessageBox.Show("Wettbewerb des Schützen ist null, Schuss kann nicht hinzugefügt werden.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Wettbewerb w = SelectedSchuetze.Wettbewerb;

            //Falls noch kein Ergebnis existiert oder die Scheibe bereits voll ist (Wertungsschüsse), neue Scheibe erstellen
            if (Ergebnis == null)
            {
                NeuesErgebnis();
            }

            //Vorher prüfen
            if (Ergebnis != null &&
                (Ergebnis.ErgebnisAbgeschlossen == true || (
                Ergebnis.AnzahlWertungsschuesse >= SelectedSchuetze.Wettbewerb.Wertung &&
                SelectedSchuetze.Wettbewerb.Wertung > 0))
              )
            {

                //Altes als Abgeschlossen markieren
                Ergebnis.ErgebnisAbgeschlossen = true;

                //Altes Speichern und neues Erstellen
                NeuesErgebnis();
            }

            //Zur Sicherheit prüfen
            if (Ergebnis == null)
            {
                Logger.Instance.Log("Ergebnis ist null, Schuss kann nicht hinzugefügt werden.", LogType.Fehler);
                MessageBox.Show("Ergebnis ist null, Schuss kann nicht hinzugefügt werden.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //Schüsse pro Scheibe hochzählen
            schuss.LfdSchussNummer = Ergebnis.Schuesse.Count + 1; //Laufende Schussnummer basierend auf der Anzahl der bisherigen Schüsse

            if (Probe)
            {
                schuss.Typ = SchussTyp.Probe;
            }
            else
            {
                schuss.Typ = SchussTyp.Wertung;
            }

            Ergebnis.Schuesse.Add(schuss);

            //Prüfen ob Anzahl Wertungsschüsse erreicht
            if (Ergebnis != null &&
                (Ergebnis.ErgebnisAbgeschlossen == true || (
                Ergebnis.AnzahlWertungsschuesse >= SelectedSchuetze.Wettbewerb.Wertung &&
                SelectedSchuetze.Wettbewerb.Wertung > 0)))
            {

                //Altes als Abgeschlossen markieren
                Ergebnis.ErgebnisAbgeschlossen = true;

            }


            //Ergebnis aktualisieren
            if (_context.Entry(Ergebnis).State == Microsoft.EntityFrameworkCore.EntityState.Detached)
            {
                _context.Ergebnisse.Add(Ergebnis); //Falls Ergebnis noch nicht in der DB, hinzufügen
            }
            else
            {
                _context.Update(Ergebnis); //Ansonsten aktualisieren
            }
            _context.SaveChanges();

            //Probe Togglen falls notwendig
            if ((Ergebnis != null && Ergebnis.AnzahlProbeschuesse >= w.Probeschuss && w.Probeschuss >= 0))
            {
                Probe = false; //Wenn die Probeschüsse voll sind, dann ist der nächste Schuss ein Wertungsschuss
            }
            Notify(nameof(Auswertung));

            UpdateBild();


            //this.Scheibe.ZeichneScheibe();
            //MessageBox.Show("Shcuss empfangen." + schuss.ToString(), "Info", MessageBoxButton.OK, MessageBoxImage.Information);

        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrEmpty(SelectedPort))
            {
                MessageBox.Show("Bitte wählen Sie einen seriellen Port aus.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            //empfaengerConfig.baudRate = int.Parse((string)((ComboBoxItem)this.BaudRateComboBox.SelectedItem).Content);
            try
            {
                empfangsConnector.OpenConnection(SelectedPort);
                MessageBox.Show("Verbindung erfolgreich hergestellt.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Herstellen der Verbindung: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }





        private void cmdRefreshPorts_Click(object sender, RoutedEventArgs e)
        {
            RefreshPorts();
        }



        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            empfangsConnector.CloseConnection();
        }


        private void UpdateControlData()
        {
            try
            {
                IInputElement focusedControl = FocusManager.GetFocusedElement(this);
                if (focusedControl != null)
                {
                    if (focusedControl is TextBox textBox)
                    {
                        var bindEx = textBox.GetBindingExpression(TextBox.TextProperty);
                        if (bindEx != null)
                        {
                            bindEx.UpdateSource(); // Aktualisiert die Bindung
                        }

                    }
                    else if (focusedControl is ComboBox comboBox)
                    {
                        var bindEx = comboBox.GetBindingExpression(ComboBox.SelectedItemProperty);
                        if (bindEx != null)
                        {
                            bindEx.UpdateSource(); // Aktualisiert die Bindung
                        }
                    }
                    else if (focusedControl is DatePicker datePicker)
                    {

                        var bindEx = datePicker.GetBindingExpression(DatePicker.SelectedDateProperty);
                        if (bindEx != null)
                        {
                            bindEx.UpdateSource(); // Aktualisiert die Bindung
                        }
                    }
                    else
                    {

                        // Fallback für andere Steuerelemente
                        //focusedControl.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Fokussieren des Controls: {ex.Message}", LogType.Fehler);
            }
        }

        private void cmdSpeichern_Click(object sender, RoutedEventArgs e)
        {
            UpdateControlData();


            try
            {
                if (tabWettkampf.IsSelected && this.SelectedWettbewerb != null)
                {
                    if (_context.Entry(SelectedWettbewerb).State == Microsoft.EntityFrameworkCore.EntityState.Detached)
                    {
                        _context.Wettbewerbe.Add(SelectedWettbewerb);
                    }
                    else
                    {
                        _context.Update(SelectedWettbewerb);
                    }

                    _context.SaveChanges();
                    Notify(nameof(Wettbewerbe));
                    return;
                }
                if (tabSchuetze.IsSelected && this.SelectedSchuetze != null)
                {

                    if (_context.Entry(SelectedSchuetze).State == Microsoft.EntityFrameworkCore.EntityState.Detached)
                    {
                        _context.Schuetzen.Add(SelectedSchuetze);
                    }
                    else
                    {
                        _context.Update(SelectedSchuetze);
                    }
                    _context.SaveChanges();
                    Notify(nameof(Schuetzen));
                    return;
                }

                if (tabErgebnis.IsSelected && this.Ergebnis != null)
                {
                    if (_context.Entry(Ergebnis).State == Microsoft.EntityFrameworkCore.EntityState.Detached)
                    {
                        _context.Ergebnisse.Add(Ergebnis);
                    }
                    else
                    {
                        _context.Update(Ergebnis);
                    }
                    _context.SaveChanges();
                    return;
                }

                MessageBox.Show("Bitte wählen Sie einen Wettbewerb, Schützen oder Ergebnis zum Speichern aus.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Speichern: {ex.Message}", LogType.Fehler);
                MessageBox.Show($"Fehler beim Speichern: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmdNeu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (tabWettkampf.IsSelected)
                {

                    if (this.SelectedWettbewerb != null)
                    {
                        _context.Update(SelectedWettbewerb);
                        _context.SaveChanges();
                    }
                    this.SelectedWettbewerb = new Wettbewerb()
                    {
                        Datum = DateTime.Now,
                        Name = "Neuer Wettbewerb",
                        Probeschuss = 1,
                        Wertung = 3,
                        Teilerwertung = false,
                        SchussGeld = 0
                    };
                    return;
                }

                if (tabSchuetze.IsSelected)
                {
                    if (SelectedWettbewerb == null)
                    {
                        MessageBox.Show("Bitte wählen Sie zuerst einen Wettbewerb aus, um einen neuen Schützen anzulegen.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    if (this.SelectedSchuetze != null)
                    {
                        _context.Update(SelectedSchuetze);
                        _context.SaveChanges();
                    }
                    this.SelectedSchuetze = new Schuetze()
                    {
                        Name = "Neuer Schütze",
                        Vorname = "",
                        Wettbewerb = SelectedWettbewerb


                    };
                    return;
                }
                MessageBox.Show("Nur Wettbewerbe und Schützen können neu angelegt werden", "Info", MessageBoxButton.OK, MessageBoxImage.Information);


            }
            catch (Exception ex)
            {

            }
        }

        private void cmdFehlschuss_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectedSchuetze == null)
                {
                    MessageBox.Show("Bitte wählen Sie zuerst einen Schützen aus, um einen Fehlschuss hinzuzufügen.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (Ergebnis == null)
                {
                    NeuesErgebnis();
                }

                if (SelectedSchuetze.Wettbewerb == null)
                {
                    _context.Entry(SelectedSchuetze).Reference(e => e.Wettbewerb).Load();
                }

                if (SelectedSchuetze.Wettbewerb == null)
                {
                    MessageBox.Show("Wettbewerb des Schützen ist nicht gesetzt. Bitte überprüfen Sie die Daten.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (Ergebnis.AnzahlWertungsschuesse >= SelectedSchuetze.Wettbewerb.Wertung || Ergebnis.ErgebnisAbgeschlossen)
                {
                    MessageBoxResult result = MessageBox.Show("Die Scheibe ist voll. Möchten Sie einen Fehlschuss hinzufügen?", "Fehlschuss hinzufügen", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.No)
                    {
                        return;
                    }
                }
                int lfdNr = Ergebnis.Schuesse.Count + 1; // Laufende Schussnummer basierend auf der Anzahl der bisherigen Schüsse
                Ergebnis.Schuesse.Add(new Schuss()
                {
                    Typ = Probe ? SchussTyp.Probe : SchussTyp.Wertung,
                    Wert = 0,
                    LfdSchussNummer = lfdNr,
                    Zeitstempel = DateTime.Now,
                    X = -5000,
                    Y = 3800 - (lfdNr * 500),
                    Distanz = 0
                });

                if (Probe = true && Ergebnis.Schuesse.Count >= SelectedSchuetze.Wettbewerb.Probeschuss && SelectedSchuetze.Wettbewerb.Probeschuss > 0)
                {
                    Probe = false; // Wenn die Probeschüsse voll sind, dann ist der nächste Schuss ein Wertungsschuss
                }

                if (Ergebnis.AnzahlWertungsschuesse >= SelectedSchuetze.Wettbewerb.Wertung)
                {
                    Ergebnis.ErgebnisAbgeschlossen = true;
                }

                if (_context.Entry(Ergebnis).State== EntityState.Detached){
                    _context.Ergebnisse.Add(Ergebnis);
                }
                _context.SaveChanges();
                UpdateBild();

            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Hinzufügen eines Fehlschusses: {ex.Message}", LogType.Fehler);
                MessageBox.Show($"Fehler beim Hinzufügen eines Fehlschusses: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void ControlDrucken(Control control)
        {
            try
            {
                // Druckdialog öffnen
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    // Drucker auswählen und Einstellungen vornehmen
                    printDialog.PrintTicket.PageOrientation = System.Printing.PageOrientation.Landscape;
                    printDialog.PrintTicket.PageMediaSize = new System.Printing.PageMediaSize(System.Printing.PageMediaSizeName.ISOA4);
                    // Scheibe drucken

                    printDialog.PrintVisual(control, "RedDotMM - Drucken");
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Drucken: {ex.Message}", LogType.Fehler);
                MessageBox.Show("Fehler beim Drucken: " + ex.Message, "Druckfehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmdDrucken_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (tabWettkampf.IsSelected)
                {
                    ControlDrucken(tabWettkampf);
                    return;
                }

                if (tabScheibe.IsSelected)
                {
                    this.Scheibe.Drucken();
                    return;
                }

                if (tabSchuetze.IsSelected)
                {
                    ControlDrucken(tabSchuetze);
                    return;
                }
                if (tabAuswertung.IsSelected)
                {
                    ControlDrucken(tabAuswertung);
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Drucken: {ex.Message}", LogType.Fehler);
                MessageBox.Show("Fehler beim Drucken: " + ex.Message, "Druckfehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void cmdErgebnisSchussLoeschen_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                var s = (sender as Button)?.DataContext as Schuss;
                if (s != null)
                {
                    if (MessageBox.Show(GetWindow(this), $"Möchten Sie den Schuss {s.LfdSchussNummer} wirklich löschen?", "Schuss löschen", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        if (Ergebnis != null && Ergebnis.Schuesse.Contains(s))
                        {
                            Ergebnis.Schuesse.Remove(s);
                            _context.SaveChanges();
                            Notify(nameof(Ergebnis));
                            UpdateBild();
                        }
                        else
                        {
                            MessageBox.Show("Der Schuss konnte nicht gefunden werden.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Löschen des Schusses: {ex.Message}", LogType.Fehler);
                MessageBox.Show($"Fehler beim Löschen des Schusses: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);

            }

        }

        private void cmdSchuetzeImportieren_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(SelectedWettbewerb == null)
                {
                    MessageBox.Show("Bitte wählen Sie zuerst einen Wettbewerb aus, um Schützen zu importieren.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                //CSV-DAtei Öffnen
                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "CSV-Dateien (*.csv)|*.csv|Alle Dateien (*.*)|*.*",
                    Title = "Schützen importieren"
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    string filePath = openFileDialog.FileName;
                    // CSV-Datei einlesen
                    var schuetzeList = CsvHelper.ImportCSV<Schuetze>(filePath, ';');
                    // Schützen zur Datenbank hinzufügen
                    foreach (var schuetze in schuetzeList)
                    {
                        if (schuetze.Wettbewerb == null)
                        {
                            schuetze.Wettbewerb = SelectedWettbewerb; // Wettbewerb zuweisen, falls nicht vorhanden
                        }
                        _context.Schuetzen.Add(schuetze);
                    }
                    _context.SaveChanges();
                    Notify(nameof(Schuetzen));
                    MessageBox.Show("Schützen erfolgreich importiert.", "Import erfolgreich", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }
            catch(Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Importieren der Schützen: {ex.Message}", LogType.Fehler);
                MessageBox.Show($"Fehler beim Importieren der Schützen: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);   
            }
        }

        private void cmdSchuetzeImportTemplate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<Schuetze> list = new List<Schuetze>();
                list.Add(new Schuetze()
                {
                    LfdNummer = 1,
                    Name = "Mustermann",
                    Vorname = "Max",
                    Zusatz = "Beispiel"
                });

                // CSV-Datei erstellen
                byte[] csvContent = CsvHelper.CreateCSVFile(list, ";");
                //Datei Speichern
                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV-Dateien (*.csv)|*.csv|Alle Dateien (*.*)|*.*",
                    Title = "Importvorlage speichern",
                    FileName = "Importvorlage_Schuetzen.csv"
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;
                    System.IO.File.WriteAllBytes(filePath, csvContent);
                    MessageBox.Show("Importvorlage erfolgreich erstellt.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                }


            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Erstellen der Importvorlage: {ex.Message}", LogType.Fehler);
                MessageBox.Show($"Fehler beim Erstellen der Importvorlage: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmdWettbewerbExportieren_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(SelectedWettbewerb == null)
                {
                    MessageBox.Show("Bitte wählen Sie zuerst einen Wettbewerb aus, um ihn zu exportieren.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // CSV-Datei erstellen
                var list = Auswerter.GetAuswertung(Context, SelectedWettbewerb.Guid);

                byte[] csvContent = CsvHelper.CreateCSVFile(list, ";");
                // Datei speichern  
                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV-Dateien (*.csv)|*.csv|Alle Dateien (*.*)|*.*",
                    Title = "Wettbewerb exportieren",
                    FileName = $"{SelectedWettbewerb.Name}_Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;
                    System.IO.File.WriteAllBytes(filePath, csvContent);
                    MessageBox.Show("Wettbewerb erfolgreich exportiert.", "Export erfolgreich", MessageBoxButton.OK, MessageBoxImage.Information);
                }   


            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Exportieren des Wettbewerbs: {ex.Message}", LogType.Fehler);
                MessageBox.Show($"Fehler beim Exportieren des Wettbewerbs: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }


        }

        private void cmdScheibeLoeschen_Click(object sender, EventArgs e)
        {
            try
            {
                if (Ergebnis == null)
                {
                    MessageBox.Show("Kein Ergebnis ausgewählt zum Löschen.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (MessageBox.Show($"Möchten Sie das Ergebnis von {Ergebnis.Schuetze.AnzeigeName} wirklich löschen?", "Ergebnis löschen", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    ////Zuerst alle Schüsse des Ergebnisses löschen
                    //var schuesse = _context.Schuesse.Where(s => s.ErgebnisID == Ergebnis.ErgebnisId).ToList();
                    //foreach (var schuss in schuesse)
                    //{
                    //    _context.Schuesse.Remove(schuss);
                    //}
                    ////Dann das Ergebnis selbst löschen
                    _context.Ergebnisse.Remove(Ergebnis);
                    _context.SaveChanges();
                    Notify(nameof(Ergebnisse));
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Löschen des Ergebnisses: {ex.Message}", LogType.Fehler);
                MessageBox.Show($"Fehler beim Löschen des Ergebnisses: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmdSchuetzeLoeschen_Click(object sender, EventArgs e)
        {
            try
            {
                Schuetze? schuetze = (sender as Button)?.DataContext as Schuetze;
                if (schuetze == null)
                {
                    MessageBox.Show("Kein Schütze ausgewählt zum Löschen.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (MessageBox.Show($"Möchten Sie den Schützen {schuetze.AnzeigeName} wirklich löschen?", "Schützen löschen", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    //Zuerst alle Ergebnisse des Schützen löschen
                    var ergebnisse = _context.Ergebnisse.Where(e => e.SchuetzeID == schuetze.SchuetzenId).ToList();
                    foreach (var erg in ergebnisse)
                    {
                        _context.Ergebnisse.Remove(erg);
                    }
                    //Dann den Schützen selbst löschen
                    _context.Schuetzen.Remove(schuetze);
                    _context.SaveChanges();
                    Notify(nameof(Schuetzen));
                    Notify(nameof(Ergebnisse));
                }


            } catch (Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Löschen des Schützen: {ex.Message}", LogType.Fehler);
                MessageBox.Show($"Fehler beim Löschen des Schützen: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void cmdErgebnisSchussgeldSpeichern_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Ergebnis == null)
                {
                    MessageBox.Show("Kein Ergebnis ausgewählt zum Speichern des Schussgeldes.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
               
                UpdateControlData();
               if(_context.Entry(Ergebnis).State == EntityState.Detached)
                {
                    _context.Ergebnisse.Add(Ergebnis); //Falls Ergebnis noch nicht in der DB, hinzufügen
                }
                else
                {
                    _context.Update(Ergebnis); //Ansonsten aktualisieren
                }                  
                _context.SaveChanges();
                Notify(nameof(Ergebnis));
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Speichern des Schussgeldes: {ex.Message}", LogType.Fehler);
                MessageBox.Show($"Fehler beim Speichern des Schussgeldes: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            empfangsConnector.CloseConnection();
            if (HandHeld != null)
            {
                HandHeld.ClosePort();
                HandHeld.ButtonPressed -= HandHeld_ButtonPressed;
                HandHeld = null;
            }
            Application.Current.Shutdown();
        }



        private void mnuInfo_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("RedDotMM - Digitales Schießstand-Management System\nAlternative zum DISAG RedDot-System.\nDieses System unterstützt keine Einstellungen oder Updates!\nVersion 1.0\n\n© 2025 Thomas Gröne", "Über RedDotMM", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #region HandHeld


        public bool HandHeldConnected
        {
            get { return HandHeld != null && HandHeld.IsOpen; }
        }

        public string HandHeldSelectedPort { get; set; }

        public void ConnectHandHeldButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(HandHeldSelectedPort))
                {
                    MessageBox.Show("Bitte wählen Sie einen seriellen Port für das Handheld-Gerät aus.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                HandHeld = new ArduinoNanoHandHeld();
                HandHeld.OpenPort(HandHeldSelectedPort);
                HandHeld.ButtonPressed += HandHeld_ButtonPressed;
                Notify(nameof(HandHeldConnected));
                WriteToHandHeld();
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Verbinden mit dem Handheld-Gerät: {ex.Message}", LogType.Fehler);
                MessageBox.Show($"Fehler beim Verbinden mit dem Handheld-Gerät: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                Notify(nameof(HandHeldConnected));
            }
        }

        public void CloseHandHeldButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (HandHeld != null)
                {
                    HandHeld.ClosePort();
                    HandHeld.ButtonPressed -= HandHeld_ButtonPressed;
                    HandHeld = null;
                }
                Notify(nameof(HandHeldConnected));
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Schließen des Handheld-Geräts: {ex.Message}", LogType.Fehler);
                MessageBox.Show($"Fehler beim Schließen des Handheld-Geräts: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                Notify(nameof(HandHeldConnected));
            }
        }

        private void HandHeld_ButtonPressed(int obj)
        {
            try
            {
                switch (obj)
                {
                    case 1: 
                        Application.Current.Dispatcher.Invoke(() => cmdFehlschuss_Click(this, null));
                        break; // Neu

                    case 2:
                        //Proble toggeln
                        Application.Current.Dispatcher.Invoke(() => Probe = !Probe);
                        break;
                    default:
                        break;
                        //nix
                }
            }catch(Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Verarbeiten des Handheld-Button-Events: {ex.Message}", LogType.Fehler);
                MessageBox.Show($"Fehler beim Verarbeiten des Handheld-Button-Events: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void WriteToHandHeld()
        {
            try
            {
                if (Ergebnis != null)
                {
                    if (HandHeld != null)
                    {
                        if (HandHeld.IsOpen)
                        {

                            if (Ergebnis.Schuesse == null)
                            {
                                _context.Entry(Ergebnis).Collection(e => e.Schuesse).Load(); //Schüsse aus DB nachladen
                            }
                            if (Ergebnis.Schuetze==null)
                            {
                                _context.Entry(Ergebnis).Reference(e => e.Schuetze).Load(); //Schützen aus DB nachladen
                            }

                            int letzteSchussNr = 0;
                            float letzterSChuss = 0;
                            if (Ergebnis.Schuesse == null || Ergebnis.Schuesse.Count == 0)
                            {
                               letzterSChuss = 0;
                                return;
                            }
                            else
                            {
                                letzterSChuss = (float)Ergebnis.Schuesse.Last().Wert;
                                letzteSchussNr = Ergebnis.Schuesse.Last().LfdSchussNummer;
                            }
                                HandHeld.SendData(
                                    Ergebnis.Schuetze.Vorname + " " + Ergebnis.Schuetze.Name,
                                    letzterSChuss,
                                    letzteSchussNr,
                                    Probe ? "P" : "W");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (HandHeld != null)
                {
                    HandHeld.ClosePort();
                }
                Logger.Instance.Log($"Fehler beim Senden von Daten an das Handheld-Gerät: {ex.Message}", LogType.Fehler);
                MessageBox.Show($"Fehler beim Senden von Daten an das Handheld-Gerät: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
           
            
        }

        #endregion HandHeld

      
    }
}
