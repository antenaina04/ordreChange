using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ordreChange.Controllers;
using ordreChange.Models;
using ordreChange.Services.Helpers;
using ordreChange.Services.Interfaces;
using ordreChange.UnitOfWork;
using System.Reflection.Metadata;

namespace ordreChange.Services.Implementations
{
    public class OrdreService : IOrdreService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITauxChangeService _tauxChangeService;
        private readonly CurrencyExchangeService _currencyExchangeService;

        public OrdreService(IUnitOfWork unitOfWork, ITauxChangeService tauxChangeService, CurrencyExchangeService currencyExchangeService)
        {
            _unitOfWork = unitOfWork;
            _tauxChangeService = tauxChangeService;
            _currencyExchangeService = currencyExchangeService;
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
        public async Task<Ordre> CreerOrdreAsync(int agentId, string typeTransaction, float montant, string devise, string deviseCible)
        {
            var agent = await _unitOfWork.Agents.GetByIdAsync(agentId);
            if (agent == null || agent.Role != Role.Acheteur)
                throw new InvalidOperationException("Agent non valide ou non autorisé à créer un ordre.");

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
                Montant = ordre.MontantConverti,
                Ordre = ordre
            };
            await _unitOfWork.HistoriqueOrdres.AddAsync(historique);

            // Appliquer changements
            await _unitOfWork.CompleteAsync();

            return ordre;
        }


        public async Task<Ordre?> GetOrdreByIdAsync(int id)
        {
            return await _unitOfWork.Ordres.GetByIdAsync(id);
        }


        /* UPDATE STATUS ORDRE */
        public async Task<bool> UpdateStatusOrdreAsync(int ordreId, int agentId, string statut)
        {
            var agent = await _unitOfWork.Agents.GetByIdAsync(agentId);
            if (agent == null)
                throw new InvalidOperationException("L'utilisateur n'est pas trouvé pour faire l'action");
            if (statut == "A modifier" && agent.Role != Role.Validateur)
                throw new InvalidOperationException("Seul un validateur peut effectuer cette action");


            var ordre = await _unitOfWork.Ordres.GetByIdAsync(ordreId);
            if (ordre == null)
                return false;
            if (statut == "Annulé" && agentId != ordre.IdAgent)
                throw new InvalidOperationException("Seul le créateur de cet ordre peut annuler cet ordre");

            ordre.Statut = statut;
            ordre.DateDerniereModification = DateTime.UtcNow;
            _unitOfWork.Ordres.Update(ordre);

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

        public async Task<bool> ValiderOrdreAsync(int ordreId, int agentId)
        {
            var agent = await _unitOfWork.Agents.GetByIdAsync(agentId);
            if (agent == null || agent.Role != Role.Validateur)
                throw new InvalidOperationException("L'agent n'est pas autorisé à valider des ordres.");

            var ordre = await _unitOfWork.Ordres.GetByIdAsync(ordreId);
            if (ordre == null || ordre.Statut != "En attente")
                return false;

            ordre.Statut = "Validé";
            ordre.DateDerniereModification = DateTime.UtcNow;
            _unitOfWork.Ordres.Update(ordre);

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
        /*
        public async Task<bool> ModifierOrdreAsync(int ordreId, int agentId, Ordre ordreModifications)
        {
            var ordreExistant = await _unitOfWork.Ordres.GetByIdAsync(ordreId);

            // Vérification si l'ordre existe
            if (ordreExistant == null)
                throw new InvalidOperationException("L'ordre spécifié est introuvable.");

            // Vérifier que l'utilisateur est le créateur de l'ordre
            if (ordreExistant.IdAgent != agentId)
                throw new InvalidOperationException("Seul le créateur de cet ordre est autorisé à modifier");

            // Vérification si l'ordre est déjà validé
            if (ordreExistant.Statut == "Validé")
                throw new InvalidOperationException("Ordre déjà validé et ne peut plus être modifié");

            double montantConverti = ConvertirMontantViaMatrice(ordreModifications.Montant, ordreModifications.Devise, ordreModifications.DeviseCible); // Matrice
                                                                                                                                                        //double montantConverti = await ConvertirMontantViaExchangeRatesAPI(ordreModifications.Montant, ordreModifications.Devise, ordreModifications.DeviseCible); // API Externe

            // Appliquer les modifications
            ordreExistant.Montant = ordreModifications.Montant;
            ordreExistant.Devise = ordreModifications.Devise;
            ordreExistant.Statut = "En attente";
            ordreExistant.DeviseCible = ordreModifications.DeviseCible;
            ordreExistant.TypeTransaction = ordreModifications.TypeTransaction;
            ordreExistant.MontantConverti = (float)montantConverti;
            ordreExistant.DateDerniereModification = DateTime.UtcNow;

            _unitOfWork.Ordres.Update(ordreExistant);


            // Ajouter un nouvel historique d'ordre pour cette modification
            var historique = new HistoriqueOrdre
            {
                Date = DateTime.UtcNow,
                Statut = ordreExistant.Statut,
                Montant = ordreExistant.MontantConverti,
                Ordre = ordreExistant
            };
            await _unitOfWork.HistoriqueOrdres.AddAsync(historique);

            await _unitOfWork.CompleteAsync();

            return true;
        }
        */
        public async Task<bool> ModifierOrdreAsync(int ordreId, int agentId, ModifierOrdreDto dto)
        {
            var ordreExistant = await _unitOfWork.Ordres.GetByIdAsync(ordreId);

            if (ordreExistant == null)
                throw new InvalidOperationException("L'ordre spécifié est introuvable.");

            if (ordreExistant.IdAgent != agentId)
                throw new InvalidOperationException("Seul le créateur de cet ordre est autorisé à le modifier.");

            if (ordreExistant.Statut == "Validé")
                throw new InvalidOperationException("Ordre déjà validé et ne peut plus être modifié.");

            double montantConverti = ConvertirMontantViaMatrice(dto.Montant, dto.Devise, dto.DeviseCible);

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
                Montant = ordreExistant.MontantConverti,
                Ordre = ordreExistant
            };
            await _unitOfWork.HistoriqueOrdres.AddAsync(historique);

            await _unitOfWork.CompleteAsync();

            return true;
        }


        public async Task<Dictionary<string, int>> GetOrdreStatutCountsAsync(int agentId)
        {
            var agent = await _unitOfWork.Agents.GetByIdAsync(agentId);
            if (agent == null || agent.Role != Role.Validateur)
                throw new InvalidOperationException("Un agent non validateur n'est pas autorisé à voir les statistiques des ordres");

            var ordres = await _unitOfWork.Ordres.GetAllAsync();
            var counts = ordres
                .GroupBy(o => o.Statut)
                .Select(group => new
                {
                    Statut = group.Key,
                    Count = group.Count()
                })
                .ToDictionary(g => g.Statut, g => g.Count);

            var allStatusCounts = new Dictionary<string, int>
            {
                { "En attente", counts.GetValueOrDefault("En attente", 0) },
                { "A modifier", counts.GetValueOrDefault("A modifier", 0) },
                { "Annulé", counts.GetValueOrDefault("Annulé", 0) },
                { "Validé", counts.GetValueOrDefault("Validé", 0) }
            };

            return allStatusCounts;
        }
    }
}
