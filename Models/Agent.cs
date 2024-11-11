using System.ComponentModel.DataAnnotations;
using System.Data;

namespace ordreChange.Models
{
    public enum Role
    {
        Acheteur,
        Validateur
    }

    public class Agent
    {
        [Key]
        public int IdAgent { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Nom { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Username { get; set; }

        [Required]
        public required string PasswordHash { get; set; } // Hashed PASSWORD


        [Required]
        public Role Role { get; set; }

        // Collection d'ordres créés ou validés
        public ICollection<Ordre>? Ordres { get; set; }
    }

}
