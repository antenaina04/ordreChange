using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ordreChange.Models
{
    public class HistoriqueOrdre
    {
        [Key]
        public int IdHistorique { get; set; }

        [Required]
        public DateTime Date { get; set; } // Date du changement

        [Required]
        [MaxLength(20)]
        public required string Statut { get; set; } // Statut au moment du changement

        [Required]
        public float Montant { get; set; } // Montant au moment du changement

        // Relation avec Ordre (clé étrangère)
        public int IdOrdre { get; set; }
        [ForeignKey("IdOrdre")]
        public required Ordre Ordre { get; set; }
    }
}
