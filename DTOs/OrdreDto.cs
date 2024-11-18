using ordreChange.DTOs;

namespace OrdreChange.Dtos
{

    public class OrdreDto
    {
        public int IdOrdre { get; set; }
        public float Montant { get; set; }
        public string? Devise { get; set; }
        public string? DeviseCible { get; set; }
        public string? Statut { get; set; }
        public string? TypeTransaction { get; set; }
        public DateTime DateCreation { get; set; }
        public float MontantConverti { get; set; }
        public AgentDto? Agent { get; set; } = null!;
    }

    public class CreerOrdreDto
    {
        public required string TypeTransaction { get; set; }
        public required float Montant { get; set; }
        public required string Devise { get; set; }
        public required string DeviseCible { get; set; }

    }
    public class OrdreResponseDto
    {
        public int IdOrdre { get; set; }
        public float Montant { get; set; }
        public string? Devise { get; set; }
        public string? DeviseCible { get; set; }
        public string? Statut { get; set; }
        public string? TypeTransaction { get; set; }
        public DateTime DateCreation { get; set; }
        public float? MontantConverti { get; set; }
        public AgentDto? Agent { get; set; } = null!;
    }
    public class HistoriqueDto
    {
        public int IdOrdre { get; set; }
        public float Montant { get; set; }
        public string? Devise { get; set; }
        public string? DeviseCible { get; set; }
        public string? Statut { get; set; }
        public string? TypeTransaction { get; set; }
        public DateTime DateCreation { get; set; }
        public float MontantConverti { get; set; }
        public AgentDto? Agent { get; set; } = null!;
        public List<HistoriqueOrdreDto>? HistoriqueOrdres { get; set; } = null!;
    }
}
