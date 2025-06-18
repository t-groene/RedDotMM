using RedDotMM.CommonHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedDotMM.Model
{
    public class Schuetze : IValidatableObject
    {

        [Key]
        public Guid SchuetzenId { get; set; }

        [ForeignKey(nameof(Wettbewerb))]
        public Guid WettbewerbID { get; set; }
        public Wettbewerb Wettbewerb { get; set; }

        [CSVHelperInfo("Nr")]
        public int? LfdNummer { get; set; } // Laufende Nummer im Wettbewerb

        [CSVHelperInfo("Name")]
        public string Name { get; set; }

        [CSVHelperInfo("Vorname")]
        public string? Vorname { get; set; }

        [CSVHelperInfo("Zusatz")]
        public string? Zusatz { get; set; }

        public virtual List<Serie> Serien { get; set; } = new List<Serie>();


        [NotMapped]
        public string AnzeigeName { get
            {
                if (string.IsNullOrEmpty(Name))
                {
                    return "Neuer Schütze";
                }
                string s = "";
                if (LfdNummer != null)
                {
                    s = $"{LfdNummer} - ";
                }

                if (string.IsNullOrEmpty(Zusatz))
                {
                    return $"{s}{Name}, {Vorname}";
                }
                else
                {
                    return $"{s}{Name}, {Vorname}\n({Zusatz})";
                }
            }
        }
        public override string ToString()
        {
            return AnzeigeName;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                yield return new ValidationResult("Der Name des Schützen darf nicht leer sein.", new[] { nameof(Name) });
            }
            
        }
    }
}
