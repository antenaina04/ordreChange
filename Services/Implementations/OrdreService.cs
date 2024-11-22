using ordreChange.Models;
using ordreChange.Services.Helpers;
using ordreChange.Services.Interfaces;
using ordreChange.UnitOfWork;
using ordreChange.Repositories.Interfaces;
using OrdreChange.Dtos;
using ordreChange.DTOs;
using ordreChange.Services.Interfaces.IRoleServices;
using ordreChange.Strategies.Roles;
using AutoMapper;
using NLog;

namespace ordreChange.Services.Implementations
{
    public class OrdreService : IOrdreService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CurrencyExchangeService _currencyExchangeService;
        private readonly IAcheteurService _acheteurService;
        private readonly IValidateurService _validateurService;
        private readonly RoleStrategyContext _roleStrategyContext;
        private readonly IAgentRepository _agentRepository;
        private readonly IMapper _mapper;
        private static readonly NLog.ILogger Logger = LogManager.GetCurrentClassLogger();

        public OrdreService(
            IUnitOfWork unitOfWork,
            IAcheteurService acheteurService,
            IValidateurService validateurService,
            CurrencyExchangeService currencyExchangeService,
            RoleStrategyContext roleStrategyContext,
            IAgentRepository agentRepository,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _currencyExchangeService = currencyExchangeService;
            _acheteurService = acheteurService;
            _validateurService = validateurService;
            _roleStrategyContext = new RoleStrategyContext();
            _agentRepository = agentRepository;
            _mapper = mapper;

            _roleStrategyContext.RegisterStrategy("Acheteur", new AcheteurStrategy());
            _roleStrategyContext.RegisterStrategy("Validateur", new ValidateurStrategy());
        }
        public async Task<OrdreDto?> GetOrdreDtoByIdAsync(int id)
        {
            Logger.Info("Fetching order with ID {OrdreId}", id);
            var ordre = await _unitOfWork.Ordres.GetOrdreByIdAsync(id);
            if (ordre == null)
            {
                Logger.Warn("Order with ID {OrdreId} not found", id);
                return null;
            }
            // AUTO_MAPPER
            return _mapper.Map<OrdreDto>(ordre);
        }
        public async Task<OrdreResponseDto> CreerOrdreAsync(int agentId, string typeTransaction, float montant, string devise, string deviseCible)
        {
            return await _acheteurService.ValidateAndExecuteAsync<OrdreResponseDto>(agentId, null, "Création",
                agent => _acheteurService.CreerOrdreAsync(agentId, typeTransaction, montant, devise, deviseCible)
            );
        }

        public async Task<bool> ModifierOrdreAsync(int ordreId, int agentId, ModifierOrdreDto dto)
        {
            return await _acheteurService.ValidateAndExecuteAsync<bool>(agentId, ordreId, "Modification",
                agent => _acheteurService.ModifierOrdreAsync(ordreId, agentId, dto)
            );
        }
        /*
        public async Task<bool> ValiderOrdreAsync(int ordreId, int agentId)
        {
            return await _validateurService.ValidateAndExecuteAsync<bool>(agentId, ordreId, "Validation",
                agent => _validateurService.ValiderOrdreAsync(ordreId, agentId)
            );
        }
        */
        public async Task<object> GetOrdreStatutCountsAsync(int agentId)
        {
            return await _validateurService.ValidateAndExecuteAsync<Dictionary<string, int>>(agentId, null, "Stats",
                agent => _validateurService.GetOrdreStatutCountsAsync()
            );
        }
        public async Task<List<OrdreDto>> GetOrdreDtoByStatutAsync(int agentId, string statut)
        {
            return await _validateurService.ValidateAndExecuteAsync<List<OrdreDto>>(agentId, null, "Statut",
                agent => _validateurService.GetOrdreDtoByStatutAsync(statut)
            );
        }
        public async Task<bool> UpdateStatusOrdreAsync(int ordreId, int agentId, string newStatut)
        {
            Logger.Info("Updating status of order {OrdreId} to {NewStatut} by agent {AgentId}", ordreId, newStatut, agentId);
            var ordre = await _unitOfWork.Ordres.GetByIdAsync(ordreId);
            if (ordre == null)
            {
                Logger.Error("Order with ID {OrdreId} not found", ordreId);
                throw new InvalidOperationException("L'ordre spécifié est introuvable.");
            }

            var agent = await _unitOfWork.Agents.GetByIdAsync(agentId);
            if (agent == null)
            {
                Logger.Error("Agent with ID {AgentId} not found", agentId);
                throw new InvalidOperationException("Agent introuvable.");
            }

            string action = newStatut switch
            {
                "A modifier" => "Refus",
                "Annulé" => "Annulation",
                "Validé" => "Validation",
                _ => throw new InvalidOperationException($"Statut '{newStatut}' non pris en charge.")
            };

            await _roleStrategyContext.CanExecuteAsync(agent.Role.Name, ordre, agentId, action);

            ordre.Statut = newStatut;
            ordre.DateDerniereModification = DateTime.UtcNow;
            _unitOfWork.Ordres.Update(ordre);

            var historique = new HistoriqueOrdre
            {
                Date = DateTime.UtcNow,
                Statut = newStatut,
                Action = action,
                Montant = ordre.MontantConverti,
                Ordre = ordre
            };

            await _unitOfWork.HistoriqueOrdres.AddAsync(historique);
            await _unitOfWork.CompleteAsync();

            Logger.Info("Order {OrdreId} status updated to {NewStatut} by agent {AgentId}", ordreId, newStatut, agentId);
            return true;
        }

        public async Task<HistoriqueDto?> GetHistoriqueDtoByOrdreIdAsync(int agentId, int ordreId)
        {
            Logger.Info("Fetching history for order {OrdreId} by agent {AgentId}", ordreId, agentId);

            var agent = await _unitOfWork.Agents.GetByIdAsync(agentId);
            if (agent == null)
            {
                Logger.Error("Agent with ID {AgentId} not found", agentId);
                throw new InvalidOperationException("Agent introuvable.");
            }

            // Récupérer l'ordre et ses historiques depuis le repository
            var ordre = await _unitOfWork.Ordres.GetOrdreWithHistoriqueByIdAsync(ordreId);
            if (ordre == null)
            {
                Logger.Error("Order with ID {OrdreId} not found", ordreId);
                throw new InvalidOperationException("Ordre introuvable.");
            }

            await _roleStrategyContext.CanExecuteAsync(agent.Role.Name, ordre, agentId, "History");

            // AUTO_MAPPER
            return _mapper.Map<HistoriqueDto>(ordre);
        }
    }
}
