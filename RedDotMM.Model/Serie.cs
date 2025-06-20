using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RedDotMM.Model
{
    public class Serie
    {
        [Key]
        public Guid SerienId { get; set; }


        [ForeignKey(nameof(Schuetze))]
        public Guid SchuetzeID { get; set; }
        public Schuetze Schuetze { get; set; }


        public bool SerieAbgeschlossen { get; set; } = false;


        /// <summary>
        /// Gibt an, wie viel Schussgeld bezahlt wurde.
        /// </summary>
        public bool SchussgeldBezahlt { get; set; } =false;


        public virtual List<Ergebnis> Ergebnisse { get; set; } = new List<Ergebnis>();


        /// <summary>
        /// Gibt die Summe der Anzahl der Wertungsschüsse in allen Ergebnissen zurück.
        /// </summary>
        [NotMapped]
        public int AnzahlWertungsschuesse
        {
            get
            {


                if (Ergebnisse == null || Ergebnisse.Count == 0)
                {
                    return 0;
                }
                else
                {
                    return Ergebnisse.Sum(e => e.AnzahlWertungsschuesse);
                }
            }
        }


        [NotMapped]
        public int SollAnzahlWertungsschuesse
        {
            get
            {
                if(Schuetze == null || Schuetze.Wettbewerb == null)
                {
                    return 0;
                }
                else
                {
                    return Schuetze.Wettbewerb.AnzahlWertungsSchuss * Schuetze.Wettbewerb.AnzahlSerien;
                }
            }
        }

        /// <summary>
        /// Gibt die Summe der Anzahl der Probeschüsse in allen Ergebnissen zurück.
        /// </summary>
        [NotMapped]
        public int AnzahlProbeschuesse
        {
            get
            {
                if (Ergebnisse == null || Ergebnisse.Count == 0)
                {
                    return 0;
                }
                else
                {
                    return Ergebnisse.Sum(e => e.AnzahlProbeschuesse);
                }
            }
        }


      

        public override string ToString()

        {
            try
            {

                if (Schuetze == null)
                {
                    return $"Serie {SerienId} - NICHT GELADEN ({AnzahlWertungsschuesse} Wertungsschüsse, {AnzahlProbeschuesse} Probeschüsse)";
                }
                else
                {
                    return $"Serie {SerienId} - {Schuetze.AnzeigeName} ({AnzahlWertungsschuesse} Wertungsschüsse, {AnzahlProbeschuesse} Probeschüsse)";
                }
            }
            catch (Exception ex)
            {
                return $"Serie {SerienId}";
           
            }

        }



       
        private decimal _RingZahl = -1;

        [NotMapped]
        /// <summary>
        /// Eigenschaft, um die Ringzahl temporär zu merken. Wird nich in der DB gespeichert!
        /// </summary>
        public decimal RingZahl
        {
            get
            {
                return _RingZahl;
            }
            set
            {
                _RingZahl = value;
            }
        }

    }
}
