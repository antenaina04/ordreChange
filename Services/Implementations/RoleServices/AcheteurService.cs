using AutoMapper;
using NLog;
using ordreChange.Models;
using ordreChange.Repositories.Interfaces;
using ordreChange.Services.Helpers;
using ordreChange.Services.Interfaces.IRoleServices;
using ordreChange.Strategies.Roles;
using ordreChange.UnitOfWork;
using OrdreChange.Dtos;
using System.Reflection.Metadata;

namespace ordreChange.Services.Implementations.RoleServices
{
    public class AcheteurService : BaseRoleService, IAcheteurService
    {
        private readonly CurrencyExchangeService _currencyExchangeService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAgentRepository _agentRepository;
        private readonly IMapper _mapper;
        private static readonly NLog.ILogger Logger = LogManager.GetCurrentClassLogger();

        public AcheteurService(
            IUnitOfWork unitOfWork,
            RoleStrategyContext roleStrategyContext,
            CurrencyExchangeService currencyExchangeService,
            IAgentRepository agentRepository,
            IMapper mapper)
            : base(unitOfWork, roleStrategyContext)
        {
            _unitOfWork = unitOfWork;
            _currencyExchangeService = currencyExchangeService;
            _agentRepository = agentRepository;
            _mapper = mapper;
        }
        public async Task<OrdreResponseDto> CreerOrdreAsync(
            int agentId,
            string typeTransaction,
            float montant,
            string devise,
            string deviseCible)
        {
            Logger.Info("Creating order for agent {AgentId} with amount {Montant} {Devise} to {DeviseCible}", agentId, montant, devise, deviseCible);
            var agent = await _agentRepository.GetByIdAsync(agentId);
            if (agent == null)
            {
                Logger.Error("Agent with ID {AgentId} not found", agentId);
                throw new UnauthorizedAccessException("No agent found to create Order");
            }

            if (montant <= 0)
            {
                Logger.Warn("Invalid amount {Montant} specified for agent {AgentId}", montant, agentId);
                throw new ArgumentException("Le montant doit être supérieur à 0.");
            }

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
            Logger.Info("Order created successfully for agent {AgentId}", agentId);
            return _mapper.Map<OrdreResponseDto>(ordre);
        }

        public async Task<bool> ModifierOrdreAsync(int ordreId, int agentId, ModifierOrdreDto dto)
        {
            var ordreExistant = await _unitOfWork.Ordres.GetByIdAsync(ordreId);

            if (ordreExistant == null) {
                Logger.Error("Order with ID {OrdreId} not found", ordreId);
                throw new KeyNotFoundException($"Order with ID = '{ordreId}' not found.");
            }

            var agent = await _unitOfWork.Agents.GetByIdAsync(agentId);
            if (agent == null)
            {
                Logger.Error("Agent with ID {AgentId} not found", agentId);
                throw new KeyNotFoundException("The specified agent cannot be found.");
            }

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
            Logger.Info("Order {OrdreId} modified successfully for agent {AgentId}", ordreId, agentId);
            return true;
        }
    }
}
