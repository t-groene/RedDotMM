using RedDotMM.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedDotMM.Win.Model
{
    public class SchuetzeViewModel : DataModelBaseViewModel<Schuetze>
    {
        public override string AnzeigeName => DatenObjekt?.AnzeigeName ?? "Neuer Schütze";


        public SchuetzeViewModel(Schuetze? datenObjekt = null) : base(datenObjekt)
        {
            // Hier können weitere Initialisierungen vorgenommen werden, falls nötig.
        }
        public List<Wettbewerb> Wettbewerbe
        {
            get
            {
                return Context.Wettbewerbe.ToList();
            }
        }


    }


   

}
