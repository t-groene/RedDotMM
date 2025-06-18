using Microsoft.EntityFrameworkCore;
using RedDotMM.CommonHelper;
using RedDotMM.Logging;
using RedDotMM.Model;
using RedDotMM.Win.Data;
using RedDotMM.Win.UIHelper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RedDotMM.Win.Model
{
    public class MainModel: INotifyPropertyChanged
    {




        public MainModel()
        {
            try
            {
                Schießbahnen = new List<Model.Schießbahn>();
                var sb = new Schießbahn();
                sb.UpdateRequested += Schießbahn_UpdateRequested;
                Schießbahnen.Add(sb);
                AktiveSchießbahn = Schießbahnen.FirstOrDefault();

                this.NeuerWettbewerbCommand = new RelayCommand(NeuerWettbewerb, () => true);
                this.BearbeiteWettbewerbCommand = new RelayCommand(BearbeiteWettbewerb, (w) => true);

                this.NeuerSchuetzeCommand = new RelayCommand(NeuerSchuetze, () => AktiverWettbewerb != null); 

            }
            catch( Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Initialisieren des MainModel: {ex.Message}", LogType.Fehler);
            }



           

        }


        public string WindowTitle
        {
            get => $"RedDotMM - Wettkampfverwaltung (Version {Assembly.GetExecutingAssembly().GetName().Version})";
        }



        private void Schießbahn_UpdateRequested(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(Serien));

        }

        public event PropertyChangedEventHandler? PropertyChanged;


        private List<Model.Schießbahn> _schießbahnen = new List<Model.Schießbahn>();


        private ObservableCollection<Model.BaseViewModel> _Fenster = new ObservableCollection<Model.BaseViewModel>();
        public ObservableCollection<Model.BaseViewModel> Fenster
        {
            get  => _Fenster;            
            set
            {
                if (_Fenster != value)
                {
                    _Fenster = value;
                    OnPropertyChanged(nameof(Fenster));
                }
            }
        }


        private BaseViewModel _aktivesFenster;
        public BaseViewModel AktivesFenster
        {
            get => _aktivesFenster;
            set
            {
                if (_aktivesFenster != value)
                {
                    _aktivesFenster = value;
                    OnPropertyChanged(nameof(AktivesFenster));
                }
            }
        }


        public List<Model.Schießbahn> Schießbahnen
        {
            get => _schießbahnen;
            set
            {
                if (_schießbahnen != value)
                {
                    _schießbahnen = value;
                    OnPropertyChanged(nameof(Schießbahnen));
                    if(_schießbahnen.Count > 0)
                    {
                        AktiveSchießbahn = _schießbahnen[0];
                    }
                    else
                    {
                        AktiveSchießbahn = null;
                    }
                }
            }
        }

        private Schießbahn _aktiveSchießbahn;
        public Schießbahn AktiveSchießbahn
        {
            get => _aktiveSchießbahn;
            set
            {
                if (_aktiveSchießbahn != value)
                {
                    _aktiveSchießbahn = value;
                    OnPropertyChanged(nameof(AktiveSchießbahn));
                }
            }
        }


        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private List<Wettbewerb> wettbewerbe = new List<Wettbewerb>();
        public List<Wettbewerb> Wettbewerbe
        {
            get
            {
                var context = new Data.RedDotMM_Context();
                return context.Wettbewerbe.ToList();
            }

           
        }

        private Wettbewerb _aktiverWettbewerb;
        public Wettbewerb AktiverWettbewerb
        {
            get => _aktiverWettbewerb;
            set
            {
                if (_aktiverWettbewerb != value)
                {
                    _aktiverWettbewerb = value;
                    OnPropertyChanged(nameof(AktiverWettbewerb));
                    OnPropertyChanged(nameof(Schuetzen));
                }
            }
        }


        private string _schuetzenNameFilter = string.Empty;
        public string SchuetzenNameFilter
        {
            get => _schuetzenNameFilter;
            set
            {
                if (_schuetzenNameFilter != value)
                {
                    _schuetzenNameFilter = value;
                    OnPropertyChanged(nameof(SchuetzenNameFilter));
                    OnPropertyChanged(nameof(Schuetzen)); // Aktualisiere die Schützenliste bei Änderung des Filters
                }
            }
        }


        public List<Schuetze> Schuetzen
        {
            get
            {
                if (AktiverWettbewerb == null)
                {
                    return null;
                }



                using (var context = new Data.RedDotMM_Context())
                {
                    if (string.IsNullOrEmpty(SchuetzenNameFilter))
                    {
                        return context.Schuetzen
                        .Where(s => s.WettbewerbID == AktiverWettbewerb.Guid)
                        .OrderBy(s => s.Name)
                        .ToList();
                    }
                    return context.Schuetzen
                        .Where(s => s.WettbewerbID == AktiverWettbewerb.Guid &&
                                s.Name.StartsWith(SchuetzenNameFilter))
                        .OrderBy(s => s.Name)
                        .ToList();
                }
                   
            }
        }

        private Schuetze _aktiverSchuetze;
        public Schuetze AktiverSchuetze
        {
            get => _aktiverSchuetze;
            set
            {
                if (_aktiverSchuetze != value)
                {
                    _aktiverSchuetze = value;
                    OnPropertyChanged(nameof(AktiverSchuetze));
                    OnPropertyChanged(nameof(Serien));
                }
            }
        }



        public ObservableCollection<Serie> Serien
        {
            get
            {
                if(AktiverSchuetze == null)
                {
                    return null;
                }
                using (var context = new Data.RedDotMM_Context())
                {
                    return new ObservableCollection<Serie>(
                        context.Serien
                        .Where(s => s.SchuetzeID == AktiverSchuetze.SchuetzenId)
                        .Include(s => s.Schuetze)
                        .ThenInclude(s => s.Wettbewerb)
                        .Include(s => s.Ergebnisse)
                        .ThenInclude(e => e.Schuesse)
                        .ToList());                   
                        
                }
            }
        }



        #region Commands


        private ICommand _NeuerWettbewerbCommand;
        public ICommand NeuerWettbewerbCommand
        {
            get => _NeuerWettbewerbCommand;
            set
            {
                if (_NeuerWettbewerbCommand != value)
                {
                    _NeuerWettbewerbCommand = value;
                    OnPropertyChanged(nameof(NeuerWettbewerbCommand));
                }
            }
        }
        public void NeuerWettbewerb()
        {
            try
            {
               
                var f = new WettbewerbViewModel();
                f.SchliessenEvent += FensterSchließenHandler;
                f.UpdateValueEvent += UpdateWettbewerbHandler;

                Fenster.Add(f);
                OnPropertyChanged(nameof(Fenster));

                if (Fenster.Count == 1)
                {
                    AktivesFenster = f;
                }

            }catch(Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Erstellen eines neuen Wettbewerbs: {ex.Message}", LogType.Fehler);
            }
           

        }



        private ICommand _BearbeiteWettbewerbCommand;
        public ICommand BearbeiteWettbewerbCommand
        {
            get => _BearbeiteWettbewerbCommand;
            set
            {
             if(_BearbeiteWettbewerbCommand != value)
                {
                    _BearbeiteWettbewerbCommand = value;
                    OnPropertyChanged(nameof(BearbeiteWettbewerbCommand));
                }
            }
            
        }
        public void BearbeiteWettbewerb(object w)
        {
            try
            {
                if (w == null) return;
                if (!(w is Wettbewerb wettbewerb)) return;


                var f = new WettbewerbViewModel(w as Wettbewerb);
                f.SchliessenEvent += FensterSchließenHandler;
                f.UpdateValueEvent += UpdateWettbewerbHandler;

                Fenster.Add(f);
                OnPropertyChanged(nameof(Fenster));

                if (Fenster.Count == 1)
                {
                    AktivesFenster = f;
                }

            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Erstellen eines neuen Wettbewerbs: {ex.Message}", LogType.Fehler);
            }
        }



        private ICommand _NeuerSchuetzeCommand;
        public ICommand NeuerSchuetzeCommand
        {
            get => _NeuerSchuetzeCommand;
            set
            {
                if (_NeuerSchuetzeCommand != value)
                {
                    _NeuerSchuetzeCommand = value;
                    OnPropertyChanged(nameof(NeuerSchuetzeCommand));
                }
            }
        }   

        private void NeuerSchuetze()
        {
            try
            {
                
                var f = new SchuetzeViewModel();
                f.DatenObjekt = new Schuetze
                {                  
                    WettbewerbID = AktiverWettbewerb?.Guid ?? Guid.Empty,
                };
                f.SchliessenEvent += FensterSchließenHandler;
                f.UpdateValueEvent += UpdateSchuetzenHandler;
                Fenster.Add(f);
                OnPropertyChanged(nameof(Fenster));
                if (Fenster.Count == 1)
                {
                    AktivesFenster = f;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Erstellen eines neuen Schützen: {ex.Message}", LogType.Fehler);
            }
        }


        private ICommand _EditSchuetzeCommand;
        public ICommand EditSchuetzeCommand
        {
            get
            {
                if (_EditSchuetzeCommand == null)
                {
                    _EditSchuetzeCommand = new RelayCommand(EditSchuetze, (object? x) => x != null && AktiverWettbewerb != null);
                }
                return _EditSchuetzeCommand;
            }
        }

        private void EditSchuetze(object? obj)
        {
            if (obj != null)
            {
                if(!(obj is Schuetze schuetze))
                {
                    MessageBox.Show("Bitte einen Schützen auswählen, um ihn zu bearbeiten.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var f = new SchuetzeViewModel(obj as Schuetze);
                

                f.SchliessenEvent += FensterSchließenHandler;
                f.UpdateValueEvent += UpdateSchuetzenHandler;
                Fenster.Add(f);
                OnPropertyChanged(nameof(Fenster));
                if (Fenster.Count == 1)
                {
                    AktivesFenster = f;
                }


            }
        }


        private ICommand _BeendenCommand;
        public ICommand BeendenCommand
        {
            get
            {
                if (_BeendenCommand == null)
                {
                    _BeendenCommand = new RelayCommand(()=>
                    {
                        Application.Current.Shutdown();
                    }
                    , () => true);
                }
                return _BeendenCommand;
            }
        }

        private ICommand _InfoCommand;
        public ICommand InfoCommand
        {
            get
            {
                if (_InfoCommand == null)
                {
                    _InfoCommand = new RelayCommand(() =>
                    {
                        MessageBox.Show($"RedDotMM - Wettkampfverwaltung\nVersion: {Assembly.GetExecutingAssembly().GetName().Version} " +
                            "Software-Alternative zur DISAG RedDot Software für die Verwendung mit DISAG Bluetooth Empfänger\n"+
                            "Projekt unter MIT-Lizenz\n" +
                            "Weitere Infos: https://github.com/t-groene/RedDotMM", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }, () => true);
                }
                return _InfoCommand;
            }
        }





        private ICommand _SchussgeldBezahlenCommand;
        public ICommand SchussgeldBezahlenCommand
        {
            get
            {
                if (_SchussgeldBezahlenCommand == null)
                {
                    _SchussgeldBezahlenCommand = new RelayCommand(SchussgeldBezahlen, (object? x) => x!=null && AktiverSchuetze != null);
                }
                return _SchussgeldBezahlenCommand;
            }
        }

        public void SchussgeldBezahlen(object? serie)
        {
            try
            {


                if (serie == null || !(serie is Serie s))
                {
                    MessageBox.Show("Bitte eine Serie auswählen, um das Schussgeld zu bezahlen.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                Guid id = ((serie as Serie)?.SerienId) ?? Guid.Empty;
                if (id == Guid.Empty)
                {
                    MessageBox.Show("Die ausgewählte Serie ist ungültig.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                using (var context = new RedDotMM_Context())
                {
                    var serieInDb = context.Serien
                        .Where(s => s.SerienId == id)
                        .Include(s => s.Schuetze)
                        .ThenInclude(s => s.Wettbewerb)
                        .FirstOrDefault();
                    if (serieInDb == null)
                    {
                        MessageBox.Show("Die ausgewählte Serie konnte nicht gefunden werden.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (MessageBox.Show($"Möchten Sie das Schussgeld in Höhe von {serieInDb.Schuetze.Wettbewerb.SchussGeld:c} für diese Serie für  '{serieInDb.Schuetze.AnzeigeName}' als Bezahlt vermerken?", "Schussgeld bezahlen", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    {
                        return; // Abbrechen, wenn der Benutzer nicht bezahlen möchte
                    }
                    // Schussgeld bezahlen


                    serieInDb.SchussgeldBezahlt = true;
                    context.Update(serieInDb);
                    context.SaveChanges();
                    OnPropertyChanged(nameof(Serien)); // Aktualisiere die Serienliste, damit die Änderungen angezeigt werden

                }


            




            }
            catch(Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Bezahlen des Schussgeldes: {ex.Message}", LogType.Fehler);
                MessageBox.Show("Fehler beim Bezahlen des Schussgeldes: " + ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private ICommand _SerieAnzeigenCommand;
        public ICommand SerieAnzeigenCommand
        {
            get
            {
                if (_SerieAnzeigenCommand == null)
                {
                    _SerieAnzeigenCommand = new RelayCommand(SerieAnzeigen, (object? x) => x != null && AktiverSchuetze != null);
                }
                return _SerieAnzeigenCommand;
            }
        }


        private void SerieAnzeigen(object? serie)
        {
            try
            {
                if (serie == null || !(serie is Serie s))
                {
                    MessageBox.Show("Bitte eine Serie auswählen, um sie anzuzeigen.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                var f = new SerienViewModel(s);
                f.SchliessenEvent += FensterSchließenHandler;
                Fenster.Add(f);
                OnPropertyChanged(nameof(Fenster));
                if (Fenster.Count == 1)
                {
                    AktivesFenster = f;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Anzeigen der Serie: {ex.Message}", LogType.Fehler);
            }
        }



        private ICommand _ExportExampleSchuetzenCSVCommand;

        public ICommand ExportExampleSchuetzenCSVCommand
        {
            get
            {
                if (_ExportExampleSchuetzenCSVCommand == null)
                {
                    _ExportExampleSchuetzenCSVCommand = new RelayCommand(ExportExampleSchuetzenCSV, () => true);
                }
                return _ExportExampleSchuetzenCSVCommand;
            }
        }

        private void ExportExampleSchuetzenCSV()
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




        private ICommand _SchuetzenImportierenCommand;
        public ICommand SchuetzenImportierenCommand
        {
            get
            {
                if (_SchuetzenImportierenCommand == null)
                {
                    _SchuetzenImportierenCommand = new RelayCommand(SchuetzenImportieren, () => AktiverWettbewerb!=null);
                }
                return _SchuetzenImportierenCommand;
            }
        }

        private void SchuetzenImportieren()
        {
            try
            {
                if (AktiverWettbewerb == null)
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
                    using (var context = new RedDotMM_Context()) {

                        foreach (var schuetze in schuetzeList)
                        {
                            if (schuetze.WettbewerbID == null || schuetze.WettbewerbID == Guid.Empty)
                            {
                                schuetze.WettbewerbID = AktiverWettbewerb.Guid; // Wettbewerb zuweisen, falls nicht vorhanden
                            }
                        context.Schuetzen.Add(schuetze);
                        }
                        context.SaveChanges();
                    }
                    OnPropertyChanged(nameof(Schuetzen));
                    MessageBox.Show($"{schuetzeList.Count} Schützen erfolgreich importiert.", "Import erfolgreich", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Importieren der Schützen: {ex.Message}", LogType.Fehler);
                MessageBox.Show($"Fehler beim Importieren der Schützen: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private ICommand _AuswertungAnzeigenCommand;
        public ICommand AuswertungAnzeigenCommand
        {
            get
            {
                if (_AuswertungAnzeigenCommand == null)
                {
                    _AuswertungAnzeigenCommand = new RelayCommand(AuswertungAnzeigen, (object? x) => x != null && AktiverWettbewerb != null);
                }
                return _AuswertungAnzeigenCommand;
            }
        }

        private void AuswertungAnzeigen(object? obj)
        {
            try
            {
                if (AktiverWettbewerb == null)
                {
                    MessageBox.Show("Bitte wählen Sie zuerst einen Wettbewerb aus, um die Auswertung anzuzeigen.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                var f = new AuswertungViewModel(AktiverWettbewerb.Guid);
                f.SchliessenEvent += FensterSchließenHandler;
                Fenster.Add(f);
                OnPropertyChanged(nameof(Fenster));
                if (Fenster.Count == 1)
                {
                    AktivesFenster = f;
                }

            }
            catch(Exception ex)
            {
                MessageBox.Show($"Beim Anzeigen der Auswertung ist ein Fehler aufgetreten: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




        #endregion Commands
        private void UpdateWettbewerbHandler(object? sender, Wettbewerb e)
        {
            OnPropertyChanged(nameof(Wettbewerbe));
        }
        private void UpdateSchuetzenHandler(object? sender, Schuetze e)
        {
            OnPropertyChanged(nameof(Schuetzen));
        }

        private void FensterSchließenHandler(object? sender, EventArgs e)
        {
            if (sender != null && Fenster.Contains(sender))
            {
                Fenster.Remove((BaseViewModel)sender);
                sender = null;
            }
        }


       


    }
}
