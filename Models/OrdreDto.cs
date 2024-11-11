namespace ordreChange.Models
{
    public class OrdreDto
    {
        public int IdOrdre { get; set; }
        public float Montant { get; set; }
        public string? Devise { get; set; }
        public string? Statut { get; set; }
        public string? TypeTransaction { get; set; }
        public DateTime DateCreation { get; set; }
        public float MontantConverti { get; set; }
        public int IdAgent { get; set; }
        public AgentDto? Agent { get; set; }
    }

    public class AgentDto
    {
        public int IdAgent { get; set; }
        public string? Nom { get; set; }
        public Role? Role { get; set; }
    }
}
