using Microsoft.EntityFrameworkCore;
using RedDotMM.CommonHelper;
using RedDotMM.Model;
using RedDotMM.Win.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace RedDotMM.Win.Model
{
    public class AuswertungViewModel : BaseViewModel
    {



        public AuswertungViewModel() : base(canSave:true, canDelete:false, canNew: false)
        {
          
            WettbewerbsID = Guid.Empty;
        }

        public AuswertungViewModel(Guid wettbewerbsID) : base(canSave: true, canDelete: false, canNew: false)
        {
           
            WettbewerbsID = wettbewerbsID;
        }

        private Guid _wettbewerbsID;

        public Guid WettbewerbsID
        {
            get => _wettbewerbsID;
            set
            {
                if (_wettbewerbsID != value)
                {
                    _wettbewerbsID = value;
                    OnPropertyChanged(nameof(WettbewerbsID));
                    Auswertung();
                }

            }
        }

        public ObservableCollection<Wettbewerb> Wettbewerbe
        {
            get
            {
                using (var context = new RedDotMM_Context())
                {
                    return new ObservableCollection<Wettbewerb>(context.Wettbewerbe.ToList());
                }
            }
        }


        private ObservableCollection<AuswertungsItemViewModel> _items;

        public ObservableCollection<AuswertungsItemViewModel> Items
        {
            get => _items;
            set
            {
                if (_items != value)
                {
                    _items = value;
                    OnPropertyChanged(nameof(Items));
                }
            }
        }

        public override string AnzeigeName => "Auswertung";

        public override void Loeschen()
        {
            throw new NotImplementedException();
        }

        public override void NeuesElement()
        {
            throw new NotImplementedException();
        }

        public override void Schliessen()
        {
            OnSchließenRequested();
        }

        public override void Speichern()
        {
            try
            {
                if(Items==null || Items.Count == 0)
                {
                    MessageBox.Show("Es gibt keine Ergebnisse zur Speicherung.", "Keine Ergebnisse", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                //Items in CSV-DAtei Speichern
                
                //Datei Speichern
                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV-Dateien (*.csv)|*.csv|Alle Dateien (*.*)|*.*",
                    Title = "Auswertung speichern",
                    FileName = "Auswertung.csv"
                };
                if (saveFileDialog.ShowDialog() == true)
                {                    
                    // CSV-Datei erstellen
                    byte[] csvContent = CsvHelper.CreateCSVFile(Items.ToList(), ";");
                    string filePath = saveFileDialog.FileName;
                    System.IO.File.WriteAllBytes(filePath, csvContent);
                    MessageBox.Show("Datei gespeichert.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Speichern: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        public void Auswertung()
        {
            try
            {
                if (WettbewerbsID == Guid.Empty)
                {
                    MessageBox.Show("Bitte wählen Sie einen Wettbewerb aus.", "Wettbewerb auswählen", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }


                using (var context = new RedDotMM_Context())
                {

                    var w = context.Wettbewerbe.FirstOrDefault(w => w.Guid == WettbewerbsID);
                    if (w == null)
                    {
                        MessageBox.Show("Der ausgewählte Wettbewerb existiert nicht.", "Wettbewerb nicht gefunden", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var auswertung = context.Serien.Where(s => s.Schuetze.WettbewerbID == WettbewerbsID
                    && s.SerieAbgeschlossen)
                        .Include(s => s.Ergebnisse)
                        .ThenInclude(e => e.Schuesse)
                        .Include(s => s.Schuetze)
                        .ThenInclude(s => s.Wettbewerb)
                        .Select(e => new AuswertungsItemViewModel
                        {
                            SerienID = e.SerienId,
                            Name = e.Schuetze.Name,
                            Vorname = e.Schuetze.Vorname,
                            Zusatz = e.Schuetze.Zusatz,
                            ScheibenAnzahl = e.Ergebnisse.Count(x => x.ErgebnisAbgeschlossen),
                            LfdNummerSchuetze = e.Schuetze.LfdNummer,
                            AnzahlWertungsschuesse = e.AnzahlWertungsschuesse,
                            SchussgeldBezahlt = e.SchussgeldBezahlt ? w.SchussGeld : 0,
                            GesamtErgebnis = -1 // Wird später berechnet
                        }).ToList();

                    foreach(var item in auswertung)
                    {
                        // Berechnung des GesamtErgebnisses

                        item.GesamtErgebnis = Helper.GesamtwertHelper.getGesamtwert(item.SerienID);

                    }



                    Items = new ObservableCollection<AuswertungsItemViewModel>(auswertung);
                }



            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler bei der Auswertung: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


        }

    }
}
