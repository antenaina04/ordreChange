using ordreChange.Models;
using ordreChange.Services.Helpers;
using ordreChange.Services.Interfaces;
using ordreChange.UnitOfWork;
using ordreChange.Repositories.Interfaces;
using OrdreChange.Dtos;
using ordreChange.Strategies.Roles;
using AutoMapper;
using NLog;

namespace ordreChange.Services.Implementations
{
    public class OrdreService : IOrdreService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CurrencyExchangeService _currencyExchangeService;
        private readonly RoleStrategyContext _roleStrategyContext;
        private readonly IAgentRepository _agentRepository;
        private readonly IAbilityRoleService _abilityRoleService;
        private readonly IMapper _mapper;
        private static readonly NLog.ILogger Logger = LogManager.GetCurrentClassLogger();

        public OrdreService(
            IUnitOfWork unitOfWork,
            CurrencyExchangeService currencyExchangeService,
            RoleStrategyContext roleStrategyContext,
            IAgentRepository agentRepository,
            IMapper mapper,
            IAbilityRoleService abilityRoleService)
        {
            _unitOfWork = unitOfWork;
            _currencyExchangeService = currencyExchangeService;
            _roleStrategyContext = new RoleStrategyContext();
            _agentRepository = agentRepository;
            _mapper = mapper;

            _roleStrategyContext.RegisterStrategy("Acheteur", new OrdreAcheteurStrategy());
            _roleStrategyContext.RegisterStrategy("Validateur", new OrdreValidateurStrategy());
            _abilityRoleService = abilityRoleService;
        }
        public async Task<OrdreDto?> GetOrdreDtoByIdAsync(int agentId, int ordreId)
        {
            Logger.Info("Fetching order with ID {OrdreId}", ordreId);
            
            var ordre = await _unitOfWork.Ordres.GetOrdreByIdAsync(ordreId);
            if (ordre == null)
            {
                Logger.Warn("Order with ID {OrdreId} not found", ordreId);
                throw new KeyNotFoundException($"Order with ID = '{ordreId}' not found.");
            }

            await _abilityRoleService.ValidateAgentAndPermissionAsync<object>(agentId, ordre, "GET_ORDRE");

            // AUTO_MAPPER
            return _mapper.Map<OrdreDto>(ordre);
        }
        public async Task<OrdreResponseDto> CreerOrdreAsync(int agentId, string typeTransaction, float montant, string devise, string deviseCible)
        {
            Logger.Info("Creating order for agent {AgentId} with amount {Montant} {Devise} to {DeviseCible}", agentId, montant, devise, deviseCible);

            await _abilityRoleService.ValidateAgentAndPermissionAsync<object>(agentId, null, "Création");

            if (montant <= 0)
            {
                Logger.Warn("Invalid amount {Montant} specified for agent {AgentId}", montant, agentId);
                throw new ArgumentException("Le montant doit être supérieur à 0.");
            }

            double montantConverti = await _currencyExchangeService.CurrencyConversion(montant, devise, deviseCible);
            
            var agent = await _unitOfWork.Agents.GetByIdAsync(agentId);

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
            if (ordreExistant == null)
            {
                Logger.Error("Order with ID {OrdreId} not found", ordreId);
                throw new KeyNotFoundException($"Order with ID = '{ordreId}' not found.");
            }

            await _abilityRoleService.ValidateAgentAndPermissionAsync<object>(agentId, ordreExistant, "Modification");

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
        public async Task<object> GetOrdreStatutCountsAsync(int agentId)
        {
            Logger.Info("Fetching order status counts");
           
            await _abilityRoleService.ValidateAgentAndPermissionAsync<object>(agentId, null, "Stats");
           
            var counts = await _unitOfWork.Ordres.GetStatutCountsAsync();

            return new Dictionary<string, int>
            {
                { "En attente", counts.GetValueOrDefault("En attente", 0) },
                { "A modifier", counts.GetValueOrDefault("A modifier", 0) },
                { "Annulé", counts.GetValueOrDefault("Annulé", 0) },
                { "Validé", counts.GetValueOrDefault("Validé", 0) }
            };
        }
        public async Task<List<OrdreDto>> GetOrdreDtoByStatutAsync(int agentId, string statut)
        {
            Logger.Info("Fetching orders with status {Statut}", statut);
            
            await _abilityRoleService.ValidateAgentAndPermissionAsync<object>(agentId, null, "Statut");

            var ordres = await _unitOfWork.Ordres.GetOrdresByStatutAsync(statut);
            if (ordres == null || ordres.Count == 0)
            {
                Logger.Warn("No orders found with status {Statut}", statut);
                return new List<OrdreDto>();
            }
            return _mapper.Map<List<OrdreDto>>(ordres);
        }

        public async Task<bool> UpdateStatusOrdreAsync(int ordreId, int agentId, string newStatut)
        {
            Logger.Info("Updating status of order {OrdreId} to {NewStatut} by agent {AgentId}", ordreId, newStatut, agentId);
            var ordre = await _unitOfWork.Ordres.GetByIdAsync(ordreId);
            if (ordre == null)
            {
                Logger.Error("Order with ID {OrdreId} not found", ordreId);
                throw new KeyNotFoundException($"Order with ID = '{ordreId}' not found.");
            }
            string action = newStatut switch
            {
                "A modifier" => "Refus",
                "Annulé" => "Annulation",
                "Validé" => "Validation",
                _ => throw new ArgumentException($"Status {newStatut} not supported.")
            };

            await _abilityRoleService.ValidateAgentAndPermissionAsync<object>(agentId, ordre, action);

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

            // Récupérer l'ordre et ses historiques depuis le repository
            var ordre = await _unitOfWork.Ordres.GetOrdreWithHistoriqueByIdAsync(ordreId);
            if (ordre == null)
            {
                Logger.Error("Order with ID {OrdreId} not found", ordreId);
                throw new KeyNotFoundException($"Order with ID = '{ordreId}' not found.");
            }
            
            await _abilityRoleService.ValidateAgentAndPermissionAsync<object>(agentId, ordre, "History");

            // AUTO_MAPPER
            return _mapper.Map<HistoriqueDto>(ordre);
        }
    }
}
