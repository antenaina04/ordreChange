using AutoMapper;
using NLog;
using ordreChange.Models;
using ordreChange.Repositories.Interfaces;
using ordreChange.Services.Interfaces.IRoleServices;
using ordreChange.Strategies.Roles;
using ordreChange.UnitOfWork;
using OrdreChange.Dtos;

namespace ordreChange.Services.Implementations.RoleServices
{
    public class ValidateurService : BaseRoleService, IValidateurService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAgentRepository _agentRepository;
        private readonly IMapper _mapper;
        private static readonly NLog.ILogger Logger = LogManager.GetCurrentClassLogger();

        public ValidateurService(
            IUnitOfWork unitOfWork,
            RoleStrategyContext roleStrategyContext,
            IAgentRepository agentRepository,
            IMapper mapper)
            : base(unitOfWork, roleStrategyContext)
        {
            _unitOfWork = unitOfWork;
            _agentRepository = agentRepository;
            _mapper = mapper;
        }

        public async Task<bool> ValiderOrdreAsync(int ordreId, int agentId)
        {
            Logger.Info("Validating order {OrdreId} for agent {AgentId}", ordreId, agentId);

            var agent = await _unitOfWork.Agents.GetByIdAsync(agentId);
            if (agent == null)
            {
                Logger.Error("Agent with ID {AgentId} not found", agentId);
                throw new KeyNotFoundException("L'agent spécifié est introuvable.");
            }

            var ordre = await _unitOfWork.Ordres.GetByIdAsync(ordreId);
            if (ordre == null)
            {
                Logger.Error("Order with ID {OrdreId} not found", ordreId);
                throw new KeyNotFoundException("L'ordre à valider est introuvable.");
            }

            bool validationRéussie = await _unitOfWork.Ordres.ValiderOrdreAsync(ordreId);
            if (!validationRéussie)
            {
                Logger.Warn("Validation failed for order {OrdreId}", ordreId);
                return false;
            }

            ordre.Statut = "Validé";
            ordre.DateDerniereModification = DateTime.UtcNow;

            await _unitOfWork.Ordres.AjouterHistoriqueAsync(ordre, "Validation");

            await _unitOfWork.CompleteAsync();
            Logger.Info("Order {OrdreId} validated successfully for agent {AgentId}", ordreId, agentId);
            return true;
        }
        public async Task<Dictionary<string, int>> GetOrdreStatutCountsAsync()
        {
            Logger.Info("Fetching order status counts");
            var counts = await _unitOfWork.Ordres.GetStatutCountsAsync();

            return new Dictionary<string, int>
            {
                { "En attente", counts.GetValueOrDefault("En attente", 0) },
                { "A modifier", counts.GetValueOrDefault("A modifier", 0) },
                { "Annulé", counts.GetValueOrDefault("Annulé", 0) },
                { "Validé", counts.GetValueOrDefault("Validé", 0) }
            };
        }

        public async Task<List<OrdreDto>> GetOrdreDtoByStatutAsync(string statut)
        {
            Logger.Info("Fetching orders with status {Statut}", statut);

            var ordres = await _unitOfWork.Ordres.GetOrdresByStatutAsync(statut);
            if (ordres == null || ordres.Count == 0)
            {
                Logger.Warn("No orders found with status {Statut}", statut);
                return new List<OrdreDto>();
            }
            // AUTO_MAPPER
            return _mapper.Map<List<OrdreDto>>(ordres);
        }
    }
}
