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
        public required string Devise { get; set; } // Code de devise (ex. USD, EUR)

        [Required]
        [MaxLength(20)]
        public required string Statut { get; set; } // En attente, Validé, Annulé, etc.

        [Required]
        [MaxLength(20)]
        public required string TypeTransaction { get; set; } // Achat ou Vente

        [Required]
        public DateTime DateCreation { get; set; }

        public DateTime? DateDerniereModification { get; set; }

        // Conversion en fonction du taux de change, stocké si calculé
        public float MontantConverti { get; set; }

        // Relation avec l'Agent (clé étrangère)
        public int IdAgent { get; set; }
        [ForeignKey("IdAgent")]
        public required Agent Agent { get; set; }

        // Historique des statuts de l'ordre
        public ICollection<HistoriqueOrdre>? HistoriqueOrdres { get; set; }
    }
}
