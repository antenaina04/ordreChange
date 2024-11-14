using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ordreChange.Models
{
    public class Ordre
    {
        [Key]
        public int IdOrdre { get; set; }

        [Required]
        public float Montant { get; set; }

        [Required]
        [MaxLength(3)]
        public required string Devise { get; set; } // USD, EUR, ...

        [Required]
        [MaxLength(3)]
        public required string DeviseCible { get; set; } // USD, EUR, ...

        [Required]
        [MaxLength(20)]
        public required string Statut { get; set; } // En attente, Validé, Annulé

        [Required]
        [MaxLength(20)]
        public required string TypeTransaction { get; set; } // Achat ou Vente

        [Required]
        public DateTime DateCreation { get; set; }

        public DateTime? DateDerniereModification { get; set; }

        public float MontantConverti { get; set; }

        // FK
        public int IdAgent { get; set; }
        [ForeignKey("IdAgent")]
        public required Agent Agent { get; set; }

        public ICollection<HistoriqueOrdre>? HistoriqueOrdres { get; set; }
    }
}
