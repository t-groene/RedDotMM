using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedDotMM.Web.Model
{
    public class SchussModel
    {

        public int SchussNummer {  get; set; }

        public decimal Ringzahl { get; set;}

        public double x { get; set; }

        public double y { get; set; }

        public bool IsProbe { get; set; }


    }
}
