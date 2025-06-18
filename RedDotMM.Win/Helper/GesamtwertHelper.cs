using Microsoft.EntityFrameworkCore;
using RedDotMM.Logging;
using RedDotMM.Model;
using RedDotMM.Win.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedDotMM.Win.Helper
{
    public class GesamtwertHelper
    {


        public static decimal getGesamtwert(Guid SerienID)
        {
            using (var context = new RedDotMM_Context())
            {
                try
                {
                    var serie = context.Serien.Where(s => s.SerienId == SerienID)
                    .Include(s => s.Ergebnisse)
                    .ThenInclude(e => e.Schuesse)
                    .Include(s => s.Schuetze)
                    .ThenInclude(s => s.Wettbewerb)
                    .FirstOrDefault(s => s.SerienId == SerienID);
                    if (serie == null || serie.Ergebnisse == null || serie.Ergebnisse.Count == 0)
                    {
                        return -1; // Serie nicht gefunden
                    }

                    if (serie.Schuetze.Wettbewerb.Teilerwertung)
                    {
                        // Teilerwertung
                        return serie.Ergebnisse.Sum(e => e.Schuesse.Where(s => s.Typ == SchussTyp.Wertung).Sum(s => Math.Abs(s.Wert)));
                    }
                    else
                    {
                        // Ganzzahlige Ringwertung
                        return serie.Ergebnisse.Sum(e => e.Schuesse.Where(s => s.Typ == SchussTyp.Wertung).Sum(s => Math.Round(s.Wert, 2)));
                    }
                }catch(Exception ex)
                {
                    Logger.Instance.Log($"Fehler beim Berechnen des Gesamtwerts für Serie {SerienID}: {ex.Message}", logType: LogType.Fehler);
                    return -2; // Fehler beim Berechnen
                }
               

                
            }
        }



    }
}
