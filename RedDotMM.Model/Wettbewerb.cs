using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedDotMM.Model
{
    public class Wettbewerb : IValidatableObject
    {
        [Key]
        public Guid Guid { get; set; }

        public string Name {get; set; }

        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}")]
        public DateTime Datum { get; set; } = DateTime.Now; // Datum des Wettbewerbs, Standard ist das aktuelle Datum

        /// <summary>
        /// Anzahl der Probeschüsse in diesem Wettbewerb
        /// </summary>
        public int AnzahlProbeschuss { get; set; }



        public bool ProbeNurAufErsterScheibe { get; set; } = true; // Gibt an, ob die Probeschüsse nur auf der ersten Scheibe geschossen werden sollen (true) oder auf jeder Scheibe der Serie zuert Probeschüsse erfolgen (false)
        /// <summary>
        /// Anzahl der Wertungsschüsse in diesem Wettbewerb
        /// </summary>
        public int AnzahlWertungsSchuss { get; set; }


        /// <summary>
        /// Anzahl der Serien (Scheiben) die geschossen werden sollen. Standart ist 1 Serie.
        /// </summary>
        public int AnzahlSerien { get; set; } = 1; // Anzahl der Serien, die geschossen werden sollen. Standard ist 1 Serie.

        /// <summary>
        /// Gibt an, ob eine Teilerwertung (max 10.9) (true) oder eine ganzzahlige Ringwertung (max 10) durchgeführt wird.
        /// </summary>
        public bool Teilerwertung { get; set; }

        /// <summary>
        /// Gibt an, wie viel Schussgeld pro Serie bezahlt werden soll.
        /// </summary>
        public decimal SchussGeld { get; set; }

       // public virtual List<Ergebnis> Ergebnisse { get; set; } = new List<Ergebnis>();

        public virtual List<Schuetze> Schuetzen { get; set; } = new List<Schuetze>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                yield return new ValidationResult("Der Name des Wettbewerbs darf nicht leer sein.", new[] { nameof(Name) });
            }
            if (Datum == default)
            {
                yield return new ValidationResult("Das Datum des Wettbewerbs muss ein gültiges Datum sein.", new[] { nameof(Datum) }); 
            }
            if (AnzahlProbeschuss < 0)
            {
                yield return new ValidationResult("Die Anzahl der Probeschüsse muss größer oder gleich 0 sein.", new[] { nameof(AnzahlProbeschuss) });
            }
            if (AnzahlWertungsSchuss < 1)
            {
                yield return new ValidationResult("Die Anzahl der Wertungsschüsse muss mindestens 1 sein.", new[] { nameof(AnzahlWertungsSchuss) });
            }
            if (AnzahlSerien < 1)
            {
                yield return new ValidationResult("Die Anzahl der Serien muss mindestens 1 sein.", new[] { nameof(AnzahlSerien) });
            }   
        }
    }
}
