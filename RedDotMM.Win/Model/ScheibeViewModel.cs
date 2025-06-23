using RedDotMM.Model;
using RedDotMM.Win.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace RedDotMM.Win.Model
{
    public class ScheibeViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler? PropertyChanged;


        public  RedDotMM_Context context { get; set; }


        public event EventHandler? ScheibeChanged;


        public event EventHandler? ScheibeDrucken;

        public event EventHandler ScheibenBildAktualisieren;

        public ScheibeViewModel(RedDotMM_Context context)
        {
            this.context = context;
            
        }


        private byte[] _ScheibenBildPNG;
        public byte[] ScheibenBildPNG
        {
            get {  return _ScheibenBildPNG;}
            set
            {
                _ScheibenBildPNG = value;
            }
        }




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
                }
            }
        }



        private bool _probe = true;
        public bool Probe
        {
            get => _probe;
            set
            {
                if (_probe != value)
                {
                    _probe = value;                    
                    OnPropertyChanged(nameof(Probe));
                    
                }
            }
        }


        public bool AddSchuss(Schuss schuss)
        {
            if (Ergebnis == null || Ergebnis.ErgebnisAbgeschlossen)
            {
                return false; // Kein Ergebnis zugeordnet
            }
            
            if (Probe)
            {
                schuss.Typ = SchussTyp.Probe;
            }
            else
            {
                schuss.Typ = SchussTyp.Wertung;
            }

            //Nummer festlegen

            schuss.LfdSchussNummer = Ergebnis.Schuesse.Count(s=>s.Typ==schuss.Typ) + 1;

            Ergebnis.Schuesse.Add(schuss);
            context.SaveChanges();

            //Prüfen ob Probe toggeln
            if (Probe)
            {
                if (Ergebnis.Schuesse.Count(s => s.Typ == SchussTyp.Probe) >= Ergebnis.Serie.Schuetze.Wettbewerb.AnzahlProbeschuss)
                {
                    Probe = false;
                }
            }
            else
            {
                if (Ergebnis.Schuesse.Count(s => s.Typ == SchussTyp.Wertung) >= Ergebnis.Serie.Schuetze.Wettbewerb.AnzahlWertungsSchuss)
                {
                    Ergebnis.ErgebnisAbgeschlossen = true;
                    context.SaveChanges();
                }
                //Serien prüfen

                using (var ctx = new RedDotMM_Context())
                {
                    var ergCount = ctx.Ergebnisse
                        .Where(e => e.SerienID == Ergebnis.SerienID && e.ErgebnisAbgeschlossen)
                        .Count();
                    var sollCount = ctx.Wettbewerbe.Where(w => w.Guid == Ergebnis.Serie.Schuetze.Wettbewerb.Guid)
                        .Select(w => w.AnzahlSerien)
                        .FirstOrDefault();
                    if (ergCount >= sollCount)
                    {
                        Ergebnis.Serie.SerieAbgeschlossen = true;
                        context.SaveChanges();
                    }
                }               
            }


            OnPropertyChanged(nameof(Ergebnis));
            return true;
        }




        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            ScheibeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
