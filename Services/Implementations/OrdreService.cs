using Microsoft.Extensions.Logging;
using ordreChange.Models;
using ordreChange.Services.Interfaces;
using ordreChange.UnitOfWork;

namespace ordreChange.Services.Implementations
{
    public class OrdreService : IOrdreService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITauxChangeService _tauxChangeService;

        public OrdreService(IUnitOfWork unitOfWork, ITauxChangeService tauxChangeService)
        {
            _unitOfWork = unitOfWork;
            _tauxChangeService = tauxChangeService;
        }

        public async Task<Ordre> CreerOrdreAsync(int agentId, string typeTransaction, float montant, string devise)
        {
            var agent = await _unitOfWork.Agents.GetByIdAsync(agentId);
            if (agent == null || agent.Role != Role.Acheteur)
                throw new InvalidOperationException("Agent non valide ou non autorisé à créer un ordre.");

            var ordre = new Ordre
            {
                Montant = montant,
                Devise = devise,
                Statut = "En attente",
                TypeTransaction = typeTransaction,
                DateCreation = DateTime.UtcNow,
                //IdAgent = agentId
                Agent = agent
            };

            await _unitOfWork.Ordres.AddAsync(ordre);
            await _unitOfWork.CompleteAsync();

            return ordre;
        }
        public async Task<Ordre?> GetOrdreByIdAsync(int id)
        {
            return await _unitOfWork.Ordres.GetByIdAsync(id);
        }

        public async Task<bool> ValiderOrdreAsync(int ordreId, int agentId)
        {
            // Vérifiez que l'agent est un Validateur
            var agent = await _unitOfWork.Agents.GetByIdAsync(agentId);
            if (agent == null || agent.Role != Role.Validateur)
                throw new InvalidOperationException("L'agent n'est pas autorisé à valider des ordres.");

            // Récupérez l'ordre
            var ordre = await _unitOfWork.Ordres.GetByIdAsync(ordreId);
            if (ordre == null || ordre.Statut != "En attente")
                return false;

            // Mettre à jour le statut de l'ordre
            ordre.Statut = "Validé";
            ordre.DateDerniereModification = DateTime.UtcNow;
            _unitOfWork.Ordres.Update(ordre);

            // Ajouter une entrée dans l'historique
            var historique = new HistoriqueOrdre
            {
                Date = DateTime.UtcNow,
                Statut = ordre.Statut,
                Montant = ordre.MontantConverti,
                Ordre = ordre
            };
            await _unitOfWork.HistoriqueOrdres.AddAsync(historique);

            // Appliquer les changements
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> ModifierOrdreAsync(Ordre ordre)
        {
            var ordreExistant = await _unitOfWork.Ordres.GetByIdAsync(ordre.IdOrdre);
            if (ordreExistant == null || ordreExistant.Statut != "En attente")
                return false;

            ordreExistant.Montant = ordre.Montant;
            ordreExistant.Devise = ordre.Devise;
            _unitOfWork.Ordres.Update(ordreExistant);
            await _unitOfWork.CompleteAsync();

            return true;
        }
    }
}
