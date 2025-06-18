using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedDotMM.Model
{

    public enum SchussTyp
    {
        Wertung,
        Probe
            
    }
    public class Schuss
    {

        [Key]
        public Guid SchussId { get; set; }

        public DateTime Zeitstempel { get; set; }


        /// <summary>
        /// Laufende Nummer des Schusses auf dieser Scheibe
        /// </summary>
        public int LfdSchussNummer { get; set; }

        /// <summary>
        /// Geschossener Ring (incl. Zehntel (z.B. max. 10.9))
        /// </summary>
        public decimal Wert { get; set; }

        /// <summary>
        /// Koordinaten Horizontal des Schusses auf der Scheibe in 1/100 mm vom Mittelpunkt (-4000...+4000)
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Koordinaten Vertikal  des Schusses auf der Scheibe in 1/100 mm vom Mittelpunkt (-4000...+4000)
        /// </summary>
        public double Y { get; set; }

        public double Distanz { get; set; }

        /// <summary>
        /// Typ des Schusses (Wertung oder Probe)
        /// </summary>
        public SchussTyp Typ { get; set; }


        [ForeignKey(nameof(Ergebnis))]
        public Guid ErgebnisID { get; set; }
        public virtual Ergebnis Ergebnis { get; set; }

        public override string ToString()
        {
            return $"Schuss: {SchussId}, Zeit: {Zeitstempel}, Lfd-Nummer: {LfdSchussNummer}, Wert: {Wert}, X: {X}, Y: {Y}, Distanz: {Distanz}";
        }

        



    }
}
