using Microsoft.EntityFrameworkCore;
using RedDotMM.Logging;
using RedDotMM.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace RedDotMM.Win.Model
{
    public class SerienViewModel : BaseViewModel
    {



        private Serie _serie;

        public Serie Serie
        {
            get => _serie;
            set
            {
                if (_serie != value)
                {
                    _serie = value;
                    OnPropertyChanged(nameof(Serie));
                    OnPropertyChanged(nameof(Scheiben));
                }
            }
        }   


        public ObservableCollection<ScheibeViewModel> Scheiben
        {
            get
            {
                if (Serie?.Ergebnisse == null)
                {
                    return new ObservableCollection<ScheibeViewModel>();
                }
                return new ObservableCollection<ScheibeViewModel>(
                    Serie.Ergebnisse.Select(e => new ScheibeViewModel(Context) { Ergebnis = e }));
            }
        }


        private ScheibeViewModel _SelectedScheibe
            ;
        public ScheibeViewModel SelectedScheibe
        {
            get => _SelectedScheibe;
            set
            {
                if (_SelectedScheibe != value)
                {
                    _SelectedScheibe = value;
                    OnPropertyChanged(nameof(SelectedScheibe));
                }
            }

        }


        public SerienViewModel(Serie s):base(canSave:false, canDelete:true, canNew: false)
        {
            var serie = Context.Serien.Where(x => x.SerienId == s.SerienId)
                .Include(s => s.Schuetze)
                .ThenInclude(s => s.Wettbewerb)
                .Include(s => s.Ergebnisse)
                .ThenInclude(e => e.Schuesse)
                .FirstOrDefault();
            this.Serie = serie ?? throw new ArgumentNullException(nameof(s), "Serie cannot be null or not found in the database.");


        }

        public override string AnzeigeName
        {
            get
            {
                return "Serie";                
            }
        }

        public override void Loeschen()
        {
            try
            {
                if (SelectedScheibe != null)
                {

                    if(MessageBox.Show($"Möchten Sie das Ergebnis Nr. {SelectedScheibe.Ergebnis.LfdNummer} wirklich löschen?", "Ergebnis löschen", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        Context.Remove(SelectedScheibe.Ergebnis);
                        Context.SaveChanges();
                        Serie.Ergebnisse.Remove(SelectedScheibe.Ergebnis);
                        OnPropertyChanged(nameof(Scheiben));
                        SelectedScheibe = null; // Reset the selected Scheibe
                    }
                }
                else
                {
                    MessageBox.Show("Bitte wählen Sie ein Ergebnis zum Löschen aus.", "Keine Auswahl", MessageBoxButton.OK, MessageBoxImage.Warning);

                }
            }catch(Exception ex)
            {
                Logger.Instance.Log($"Fehler beim Löschen eines Ergebnisses: {ex.Message}",logType: LogType.Fehler);
                MessageBox.Show($"Fehler beim Löschen eines Ergebnisses: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
           


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
            throw new NotImplementedException();
        }
    }
}
