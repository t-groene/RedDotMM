using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedDotMM.Win.UIHelper
{

    /// <summary>
    /// Stellt Funktionen bereit, um Koordinaten von -4000 bis +4000 100/mm in 0...8000 umzurechnen und umgekehrt.
    /// </summary>
    public class KoordinatenUmrechner
    {

        public static int UmrechnenVonSchussInBasis0(int koordinaten100mm)
        {
            if (koordinaten100mm < -4000 || koordinaten100mm > 4000)
            {
                throw new ArgumentOutOfRangeException(nameof(koordinaten100mm), "Koordinaten müssen im Bereich von -4000 bis +4000 liegen.");
            }
            return koordinaten100mm + 4000;
        }

        




    }
}
