using ordreChange.Models;
using ordreChange.Services.Interfaces;
using ordreChange.UnitOfWork;

namespace ordreChange.Services.Implementations
{
    public class OrdreService : IOrdreService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrdreService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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

        public async Task<bool> ValiderOrdreAsync(int ordreId)
        {
            var ordre = await _unitOfWork.Ordres.GetByIdAsync(ordreId);
            if (ordre == null || ordre.Statut != "En attente")
                return false;

            ordre.Statut = "Validé";
            _unitOfWork.Ordres.Update(ordre);
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
