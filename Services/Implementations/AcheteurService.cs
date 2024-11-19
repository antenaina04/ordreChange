using ordreChange.Models;
using ordreChange.Repositories.Interfaces;
using ordreChange.Services.Interfaces;
using ordreChange.Services.Roles;
using ordreChange.UnitOfWork;

namespace ordreChange.Services.Implementations
{
    public class AcheteurService : BaseRoleService, IAcheteurService
    {
        private readonly ITauxChangeService _tauxChangeService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAgentRepository _agentRepository;


        public AcheteurService(
            IUnitOfWork unitOfWork,
            RoleStrategyContext roleStrategyContext,
            ITauxChangeService tauxChangeService,
            IAgentRepository agentRepository)
            : base(unitOfWork, roleStrategyContext)
        {
            _unitOfWork = unitOfWork;
            _tauxChangeService = tauxChangeService;
            _agentRepository = agentRepository;
        }
        public double ConvertirMontantViaMatrice(double montant, string deviseSource, string deviseCible)
        {
            var taux = _tauxChangeService.GetTaux(deviseSource, deviseCible);
            return montant * taux;
        }
        /*
        public async Task<double> ConvertirMontantViaExchangeRatesAPI(double montant, string deviseSource, string deviseCible)
        {
            var taux = await _currencyExchangeService.GetExchangeRateAsync(deviseSource, deviseCible);

            if (taux == null)
            {
                throw new Exception("Erreur lors de la récuperation du taux de change par l'API externe");
            }

            return montant * (double)taux;
        }
        */
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

            double montantConverti = ConvertirMontantViaMatrice(montant, devise, deviseCible); // Matrice
            //double montantConverti = await ConvertirMontantViaExchangeRatesAPI(montant, devise, deviseCible); // API Externe

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
    }
}
