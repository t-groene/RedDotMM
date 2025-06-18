using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedDotMM.CommonHelper;
namespace RedDotMM.Model.NoDB
{
    public class AuswertungsItem
    {

        [CSVHelperInfo("Name")]
        public string Name { get; set; }

        [CSVHelperInfo("Vorname")]
        public string Vorname { get; set; }

        [CSVHelperInfo("Zusatz")]
        public string? Zusatz { get; set; }

        [CSVHelperInfo("Nr. Schütze")]
        public int LfdNummerSchuetze { get; set; }

        [CSVHelperInfo("Scheibennummer")]
        public int LfdNummerScheibe { get; set; }

        [CSVHelperInfo("Anzahl Wertungsschüsse")]
        public int AnzahlWertungsschuesse { get; set; }

        [CSVHelperInfo("Ring")]
        public double GesamtErgebnis { get; set; }

        [CSVHelperInfo("Bezahlt")]
        public decimal SchussgeldBezahlt { get; set; }

    }
}
