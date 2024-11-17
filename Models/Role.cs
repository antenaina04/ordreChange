using System.ComponentModel.DataAnnotations;

namespace ordreChange.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Name { get; set; } // Exemple : "Acheteur", "Validateur"

        // Relation avec les agents
        public ICollection<Agent>? Agents { get; set; }
    }
}
