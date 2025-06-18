using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace RedDotMM.Model
{
    public class Ergebnis
    {
        [Key]
        public Guid ErgebnisId { get; set; }

        public DateTime Zeitstempel { get; set; } = DateTime.Now;


        [ForeignKey(nameof(Serie))]
        public Guid SerienID { get; set; }
        public Serie Serie { get; set; }  

        public int LfdNummer { get; set; }

        //[ForeignKey(nameof(Wettbewerb))]
        //public Guid WettbewerbID { get; set; }
        //public Wettbewerb Wettbewerb { get; set; }


        public virtual List<Schuss> Schuesse { get; set; } = new List<Schuss>();

        public decimal BezahltesSchussGeld { get; set; } = 0;

        public bool ErgebnisAbgeschlossen { get; set; } = false;

        [NotMapped]
        public int AnzahlWertungsschuesse
        {

            get
            {
                if (Schuesse == null || Schuesse.Count == 0)
                {
                    return 0;
                }
                return Schuesse.Where(s => s.Typ == SchussTyp.Wertung).Count();
            }
        }
        [NotMapped]
        public int AnzahlProbeschuesse
        {

            get
            {
                if (Schuesse == null || Schuesse.Count == 0)
                {
                    return 0;
                }
                return Schuesse.Where(s => s.Typ == SchussTyp.Probe).Count();
            }
        }

        [NotMapped]
        public decimal WertungTeiler
        {

            get
            {
                if (Schuesse == null || Schuesse.Count == 0)
                {
                    return 0;
                }
                return Schuesse.Where(s => s.Typ == SchussTyp.Wertung).Sum(s=>Math.Round(s.Wert, 2));
            }
        }

        [NotMapped]
        public decimal WertungAbsolut
        {

            get
            {
                if (Schuesse == null || Schuesse.Count == 0)
                {
                    return 0;
                }
                return Schuesse.Where(s => s.Typ == SchussTyp.Wertung).Sum(s => Math.Abs(s.Wert));
            }
        }




    }
}
