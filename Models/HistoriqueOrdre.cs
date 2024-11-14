using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ordreChange.Models
{
    public class HistoriqueOrdre
    {
        [Key]
        public int IdHistorique { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [MaxLength(20)]
        public required string Statut { get; set; } 

        [Required]
        public float Montant { get; set; } 
        public int IdOrdre { get; set; }
        [ForeignKey("IdOrdre")]
        public required Ordre Ordre { get; set; }
    }
}
