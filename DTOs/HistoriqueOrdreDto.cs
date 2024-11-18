namespace ordreChange.DTOs
{
    public class HistoriqueOrdreDto
    {
        public int IdHistorique { get; set; }
        public DateTime Date { get; set; }
        public string? Statut { get; set; }
        public string? Action { get; set; }
        public float Montant { get; set; }
    }
}
