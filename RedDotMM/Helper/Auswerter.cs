using RedDotMM.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedDotMM.Model.NoDB;
using Microsoft.EntityFrameworkCore;
using RedDotMM.Model;
namespace RedDotMM.Helper
{
    public class Auswerter
    {

        public static List<AuswertungsItem> GetAuswertung(RedDotMMContext context, Guid WettbewerbsID)
        {
            try
            {
                var auswertung = context.Ergebnisse
                    .Where(e => e.Serie.Schuetze.WettbewerbID == WettbewerbsID &&
                    e.ErgebnisAbgeschlossen)
                    .Select(e => new AuswertungsItem
                    {
                        Name = e.Serie.Schuetze.Name,
                        Vorname = e.Serie.Schuetze.Vorname,
                        Zusatz = e.Serie.Schuetze.Zusatz,
                        LfdNummerScheibe = e.LfdNummer,
                        LfdNummerSchuetze = e.Serie.Schuetze.LfdNummer,
                        AnzahlWertungsschuesse = e.AnzahlWertungsschuesse,
                        SchussgeldBezahlt = e.BezahltesSchussGeld,
                        GesamtErgebnis = GetGesamtWertung(context, e)
                    })
                    .ToList()
                    .OrderByDescending(a => a.GesamtErgebnis)
                    .ToList();
                return auswertung;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                throw new Exception("Fehler bei der Auswertung: " + ex.Message, ex);
            }

        }


        public static double GetGesamtWertung(RedDotMMContext? context, Ergebnis erg)
        {
            if (context != null)
            {


                if (erg.Schuesse == null || erg.Schuesse.Count == 0)
                {
                    context.Entry(erg).Collection(e => e.Schuesse).Load();
                }

                if (erg.Serie == null)
                {
                    context.Entry(erg).Reference(e => e.Serie).Load();
                }
                if (erg.Serie.Schuetze == null)
                {
                    context.Entry(erg).Reference(e => e.Serie.Schuetze).Load();
                }
                if (erg.Serie.Schuetze.Wettbewerb == null)
                {
                    context.Entry(erg.Serie.Schuetze).Reference(s => s.Wettbewerb).Load();
                }
            }
           
            if (erg.Schuesse == null || erg.Schuesse.Count == 0)
            {
                return 0;
            }
            if (erg.Serie.Schuetze != null && erg.Serie.Schuetze.Wettbewerb != null)
            {
                if (erg.Serie.Schuetze.Wettbewerb.Teilerwertung)
                {
                    return erg.Schuesse.Where(s => s.Typ == SchussTyp.Wertung).Sum(s => s.Wert);
                }
                else
                {
                    return erg.Schuesse.Where(s => s.Typ == SchussTyp.Wertung).Sum(s => Math.Abs(s.Wert));
                }
            }
            return -1;


        }
    }
}
