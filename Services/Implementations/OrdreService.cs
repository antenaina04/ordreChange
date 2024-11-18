using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ordreChange.Controllers;
using ordreChange.Models;
using ordreChange.Services.Helpers;
using ordreChange.Services.Interfaces;
using ordreChange.UnitOfWork;
using System.Reflection.Metadata;
using ordreChange.Services.Roles;
using System;
using ordreChange.Repositories.Interfaces;
using OrdreChange.Dtos;
using ordreChange.DTOs;

namespace ordreChange.Services.Implementations
{
    public class OrdreService : IOrdreService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITauxChangeService _tauxChangeService;
        private readonly CurrencyExchangeService _currencyExchangeService;
        private readonly RoleStrategyContext _roleStrategyContext;
        private readonly IAgentRepository _agentRepository;
        private string _action;

        public OrdreService(
            IUnitOfWork unitOfWork,
            ITauxChangeService tauxChangeService,
            CurrencyExchangeService currencyExchangeService,
            RoleStrategyContext roleStrategyContext,
            IAgentRepository agentRepository)
        {
            _unitOfWork = unitOfWork;
            _tauxChangeService = tauxChangeService;
            _currencyExchangeService = currencyExchangeService;
            _roleStrategyContext = new RoleStrategyContext();
            _agentRepository = agentRepository;

            _roleStrategyContext.RegisterStrategy("Acheteur", new AcheteurStrategy());
            _roleStrategyContext.RegisterStrategy("Validateur", new ValidateurStrategy());
        }

        public double ConvertirMontantViaMatrice(double montant, string deviseSource, string deviseCible)
        {
            var taux = _tauxChangeService.GetTaux(deviseSource, deviseCible);
            return montant * taux;
        }
        public async Task<Ordre?> GetOrdreByIdAsync(int id)
        {
            return await _unitOfWork.Ordres.GetOrdreByIdAsync(id);
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
            _action = "Création";
            var agent = await _agentRepository.GetByIdAsync(agentId);
            if (agent == null)
                throw new InvalidOperationException("Agent introuvable");

            await _roleStrategyContext.CanExecuteAsync(agent.Role.Name, null, agentId, _action);

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
                Action = _action,
                Montant = ordre.MontantConverti,
                Ordre = ordre
            };
            await _unitOfWork.HistoriqueOrdres.AddAsync(historique);

            // Appliquer changements
            await _unitOfWork.CompleteAsync();

            return ordre;
        }

        public async Task<bool> ValiderOrdreAsync(int ordreId, int agentId)
        {
            _action = "Validation";
            var agent = await _unitOfWork.Agents.GetByIdAsync(agentId);
            if (agent == null)
                throw new InvalidOperationException("L'agent est introuvable.");

            var ordre = await _unitOfWork.Ordres.GetByIdAsync(ordreId);
            if (ordre == null)
                throw new InvalidOperationException("L'ordre spécifié est introuvable.");

            await _roleStrategyContext.CanExecuteAsync(agent.Role.Name, ordre, agentId, _action);

            // Validate ordre
            bool validationRéussie = await _unitOfWork.Ordres.ValiderOrdreAsync(ordreId);
            if (!validationRéussie)
                return false;

            // Update ordre information
            ordre.Statut = "Validé";
            ordre.DateDerniereModification = DateTime.UtcNow;

            // Action History
            await _unitOfWork.Ordres.AjouterHistoriqueAsync(ordre, _action);

            // Execute transaction
            await _unitOfWork.CompleteAsync();

            return true;
        }
        public async Task<bool> ModifierOrdreAsync(int ordreId, int agentId, ModifierOrdreDto dto)
        {
            _action = "Modification";
            var ordreExistant = await _unitOfWork.Ordres.GetByIdAsync(ordreId);

            if (ordreExistant == null)
                throw new InvalidOperationException("L'ordre spécifié est introuvable.");

            var agent = await _unitOfWork.Agents.GetByIdAsync(agentId);
            if (agent == null)
                throw new InvalidOperationException("Agent introuvable.");

            await _roleStrategyContext.CanExecuteAsync(agent.Role.Name, ordreExistant, agentId, _action);

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
                Action = _action,
                Montant = ordreExistant.MontantConverti,
                Ordre = ordreExistant
            };
            await _unitOfWork.HistoriqueOrdres.AddAsync(historique);

            await _unitOfWork.CompleteAsync();

            return true;
        }
        public async Task<bool> UpdateStatusOrdreAsync(int ordreId, int agentId, string statut)
        {
            var ordre = await _unitOfWork.Ordres.GetByIdAsync(ordreId);
            if (ordre == null)
                throw new InvalidOperationException("L'ordre spécifié est introuvable.");

            var agent = await _unitOfWork.Agents.GetByIdAsync(agentId);
            if (agent == null)
                throw new InvalidOperationException("Agent introuvable.");

            string action = statut switch
            {
                "A modifier" => "Refus",
                "Annulé" => "Annulation",
                "Valider" => "Validation",
                _ => throw new InvalidOperationException($"Statut '{statut}' non pris en charge.")
            };

            await _roleStrategyContext.CanExecuteAsync(agent.Role.Name, ordre, agentId, action);

            ordre.Statut = statut;
            ordre.DateDerniereModification = DateTime.UtcNow;
            _unitOfWork.Ordres.Update(ordre);

            var historique = new HistoriqueOrdre
            {
                Date = DateTime.UtcNow,
                Statut = statut,
                Action = action,
                Montant = ordre.MontantConverti,
                Ordre = ordre
            };

            await _unitOfWork.HistoriqueOrdres.AddAsync(historique);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async Task<Dictionary<string, int>> GetOrdreStatutCountsAsync(int agentId)
        {
            var agent = await _unitOfWork.Agents.GetByIdAsync(agentId);
            if (agent == null)
                throw new InvalidOperationException("Agent introuvable.");

            await _roleStrategyContext.CanExecuteAsync(agent.Role.Name, null, agentId, "Stats");

            var counts = await _unitOfWork.Ordres.GetStatutCountsAsync();

            return new Dictionary<string, int>
            {
                { "En attente", counts.GetValueOrDefault("En attente", 0) },
                { "A modifier", counts.GetValueOrDefault("A modifier", 0) },
                { "Annulé", counts.GetValueOrDefault("Annulé", 0) },
                { "Validé", counts.GetValueOrDefault("Validé", 0) }
            };
        }
        public async Task<List<HistoriqueOrdre>> GetHistoriqueByOrdreIdAsync(int ordreId)
        {
            return await _unitOfWork.Ordres.GetHistoriqueByOrdreIdAsync(ordreId);
        }
        public async Task<List<Ordre>> GetOrdresByStatutAsync(string statut)
        {
            return await _unitOfWork.Ordres.GetOrdresByStatutAsync(statut);
        }
    }
}
