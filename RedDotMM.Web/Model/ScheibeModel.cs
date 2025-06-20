using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedDotMM.Web.Model
{
    public class ScheibeModel
    {



        public ScheibeModel() 
        { 
        }

       

        public string SchuetzeName { get; set; }

        public string WettkampfName {  get; set; }

        public int SchiebenNummer { get; set; }

        public bool Teilerwertung { get;set; }


        public decimal GesamtWertung { get; set; }

        public SchussModel[] Schuesse { get; set; }

    }
}
