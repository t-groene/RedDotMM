using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedDotMM.Empfaenger
{
    public class EmpfangenerSchuss
    {

        /// <summary>
        /// Zeitpüunkt, wann der Schuss empfangen wurde (Systemzeit PC)
        /// </summary>
        public DateTime Zeitstempel { get; set; }


       
        /// <summary>
        /// Geschossener Ring (incl. Zehntel (z.B. max. 10.9))
        /// </summary>
        public double Wert { get; set; }

        /// <summary>
        /// Koordinaten Horizontal des Schusses auf der Scheibe in 1/100 mm vom Mittelpunkt (-4000...+4000)
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Koordinaten Vertikal  des Schusses auf der Scheibe in 1/100 mm vom Mittelpunkt (-4000...+4000)
        /// </summary>
        public double Y { get; set; }



        public double Distanz { get; set; }


    }
}
