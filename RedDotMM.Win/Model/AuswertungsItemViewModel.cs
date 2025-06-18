using RedDotMM.CommonHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedDotMM.Win.Model
{
    public class AuswertungsItemViewModel
    {

        public Guid SerienID { get; set; }

        [CSVHelperInfo("Name")]
        public string Name { get; set; }

        [CSVHelperInfo("Vorname")]
        public string? Vorname { get; set; }

        [CSVHelperInfo("Zusatz")]
        public string? Zusatz { get; set; }

        [CSVHelperInfo("Nr. Schütze")]
        public int? LfdNummerSchuetze { get; set; }

        [CSVHelperInfo("Scheibenanzahl")]
        public int ScheibenAnzahl { get; set; }

        [CSVHelperInfo("Anzahl Wertungsschüsse")]
        public int AnzahlWertungsschuesse { get; set; }

        [CSVHelperInfo("Ring")]
        public decimal GesamtErgebnis { get; set; }

        [CSVHelperInfo("Bezahlt")]
        public decimal SchussgeldBezahlt { get; set; }
    }
}
