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
