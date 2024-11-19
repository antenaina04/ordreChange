using ordreChange.Models;
using ordreChange.Repositories.Interfaces;
using ordreChange.Services.Helpers;
using ordreChange.Services.Interfaces;
using ordreChange.Services.Roles;
using ordreChange.UnitOfWork;
using OrdreChange.Dtos;
using System.Reflection.Metadata;

namespace ordreChange.Services.Implementations
{
    public class AcheteurService : BaseRoleService, IAcheteurService
    {
        private readonly CurrencyExchangeService _currencyExchangeService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAgentRepository _agentRepository;


        public AcheteurService(
            IUnitOfWork unitOfWork,
            RoleStrategyContext roleStrategyContext,
            CurrencyExchangeService currencyExchangeService,
            IAgentRepository agentRepository)
            : base(unitOfWork, roleStrategyContext)
        {
            _unitOfWork = unitOfWork;
            _currencyExchangeService = currencyExchangeService;
            _agentRepository = agentRepository;
        }
        public async Task<Ordre> CreerOrdreAsync(
            int agentId,
            string typeTransaction,
            float montant,
            string devise,
            string deviseCible)
        {
            var agent = await _agentRepository.GetByIdAsync(agentId);
            if (agent == null)
                throw new InvalidOperationException("Agent introuvable");

            double montantConverti = await _currencyExchangeService.CurrencyConversion(montant, devise, deviseCible);

            var ordre = new Ordre
            {
                Montant = montant,
                Devise = devise,
                DeviseCible = deviseCible,
                Statut = "En attente",
                TypeTransaction = typeTransaction,
                DateCreation = DateTime.UtcNow,
                MontantConverti = (float)montantConverti,
                Agent = agent
            };
            await _unitOfWork.Ordres.AddAsync(ordre);
            
            var historique = new HistoriqueOrdre
            {
                Date = DateTime.UtcNow,
                Statut = ordre.Statut,
                Action = "Création",
                Montant = ordre.MontantConverti,
                Ordre = ordre
            };
            await _unitOfWork.HistoriqueOrdres.AddAsync(historique);

            await _unitOfWork.CompleteAsync();
            return ordre;
        }

        public async Task<bool> ModifierOrdreAsync(int ordreId, int agentId, ModifierOrdreDto dto)
        {
            var ordreExistant = await _unitOfWork.Ordres.GetByIdAsync(ordreId);

            if (ordreExistant == null)
                throw new InvalidOperationException("L'ordre spécifié est introuvable.");

            var agent = await _unitOfWork.Agents.GetByIdAsync(agentId);
            if (agent == null)
                throw new InvalidOperationException("Agent introuvable.");

            double montantConverti = await _currencyExchangeService.CurrencyConversion(dto.Montant, dto.Devise, dto.DeviseCible);

            // Appliquer les modifications
            ordreExistant.Montant = dto.Montant;
            ordreExistant.Devise = dto.Devise;
            ordreExistant.Statut = "En attente";
            ordreExistant.DeviseCible = dto.DeviseCible;
            ordreExistant.TypeTransaction = dto.TypeTransaction;
            ordreExistant.MontantConverti = (float)montantConverti;
            ordreExistant.DateDerniereModification = DateTime.UtcNow;

            _unitOfWork.Ordres.Update(ordreExistant);

            var historique = new HistoriqueOrdre
            {
                Date = DateTime.UtcNow,
                Statut = "En attente",
                Action = "Modification",
                Montant = ordreExistant.MontantConverti,
                Ordre = ordreExistant
            };
            await _unitOfWork.HistoriqueOrdres.AddAsync(historique);

            await _unitOfWork.CompleteAsync();

            return true;
        }
    }
}
