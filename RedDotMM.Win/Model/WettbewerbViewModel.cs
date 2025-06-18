using RedDotMM.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RedDotMM.Win.Model
{
    public class WettbewerbViewModel : DataModelBaseViewModel<Wettbewerb>
    {


        public WettbewerbViewModel() : base(null)
        {
            // Hier können weitere Initialisierungen vorgenommen werden, falls nötig.
        }
        public WettbewerbViewModel(Wettbewerb? datenObjekt = null) : base(datenObjekt)
        {
            // Hier können weitere Initialisierungen vorgenommen werden, falls nötig.
        }
        public override string AnzeigeName
        {
            get
            {
                if (DatenObjekt == null || String.IsNullOrEmpty(DatenObjekt.Name))
                {
                    return $"Neuer Wettbewerb";
                }
                else
                {
                    return $"{DatenObjekt.Name}";
                }
                
            }
        }
    }
}
