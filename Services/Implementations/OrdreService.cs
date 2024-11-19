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
        private readonly IAcheteurService _acheteurService;
        private readonly IValidateurService _validateurService;
        private readonly RoleStrategyContext _roleStrategyContext;
        private readonly IAgentRepository _agentRepository;
        private string _action;

        public OrdreService(
            IUnitOfWork unitOfWork,
            ITauxChangeService tauxChangeService,
            IAcheteurService acheteurService,
            IValidateurService validateurService,
            CurrencyExchangeService currencyExchangeService,
            RoleStrategyContext roleStrategyContext,
            IAgentRepository agentRepository)
        {
            _unitOfWork = unitOfWork;
            _tauxChangeService = tauxChangeService;
            _currencyExchangeService = currencyExchangeService;
            _acheteurService = acheteurService;
            _validateurService = validateurService;
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

        /// <summary>
        /// Creates an order for a specific agent after validating the agent's permission to perform the action.
        /// </summary>
        /// <param name="agentId">The unique identifier of the agent creating the order.</param>
        /// <param name="typeTransaction">The type of transaction (e.g., "buy" or "sell").</param>
        /// <param name="montant">The amount involved in the transaction.</param>
        /// <param name="devise">The currency of the transaction.</param>
        /// <param name="deviseCible">The target currency for the transaction.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains the created order of type <see cref="Ordre"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">Thrown if the agent cannot be found or is not authorized to perform the action.</exception>
        public async Task<Ordre> CreerOrdreAsync(int agentId, string typeTransaction, float montant, string devise, string deviseCible)
        {
            return await _acheteurService.ValidateAndExecuteAsync<Ordre>(agentId, null, "Création",
                agent => _acheteurService.CreerOrdreAsync(agentId, typeTransaction, montant, devise, deviseCible)
            );
        }
        public async Task<bool> ValiderOrdreAsync(int ordreId, int agentId)
        {
            return await _validateurService.ValidateAndExecuteAsync<bool>(agentId, ordreId, "Validation",
                agent => _validateurService.ValiderOrdreAsync(ordreId, agentId)
            );
        }
        public async Task<bool> ModifierOrdreAsync(int ordreId, int agentId, ModifierOrdreDto dto)
        {
            return await _acheteurService.ValidateAndExecuteAsync<bool>(agentId, ordreId, "Modification",
                agent => _acheteurService.ModifierOrdreAsync(ordreId, agentId, dto)
            );
        }
        public async Task<bool> UpdateStatusOrdreAsync(int ordreId, int agentId, string newStatut)
        {
            var ordre = await _unitOfWork.Ordres.GetByIdAsync(ordreId);
            if (ordre == null)
                throw new InvalidOperationException("L'ordre spécifié est introuvable.");

            var agent = await _unitOfWork.Agents.GetByIdAsync(agentId);
            if (agent == null)
                throw new InvalidOperationException("Agent introuvable.");

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

            return true;
        }
        public async Task<Dictionary<string, int>> GetOrdreStatutCountsAsync(int agentId)
        {
            return await _validateurService.ValidateAndExecuteAsync<Dictionary<string, int>>(agentId, null, "Stats",
                agent => _validateurService.GetOrdreStatutCountsAsync()
            );
        }
        public async Task<HistoriqueDto?> GetHistoriqueByOrdreIdAsync(int agentId, int ordreId)
        {
            var agent = await _unitOfWork.Agents.GetByIdAsync(agentId);
            if (agent == null)
                throw new InvalidOperationException("Agent introuvable.");

            await _roleStrategyContext.CanExecuteAsync(agent.Role.Name, null, agentId, "History");

            // Récupérer l'ordre et ses historiques depuis le repository
            var ordre = await _unitOfWork.Ordres.GetOrdreWithHistoriqueByIdAsync(ordreId);
            if (ordre == null)
                throw new InvalidOperationException("Ordre introuvable.");

            // Mapper les données en DTO
            return new HistoriqueDto
            {
                IdOrdre = ordre.IdOrdre,
                Montant = ordre.Montant,
                Devise = ordre.Devise,
                DeviseCible = ordre.DeviseCible,
                Statut = ordre.Statut,
                TypeTransaction = ordre.TypeTransaction,
                DateCreation = ordre.DateCreation,
                MontantConverti = ordre.MontantConverti,
                Agent = new AgentDto
                {
                    IdAgent = ordre.Agent.IdAgent,
                    Nom = ordre.Agent.Nom,
                    RoleName = ordre.Agent.Role?.Name
                },
                HistoriqueOrdres = ordre.HistoriqueOrdres.Select(h => new HistoriqueOrdreDto
                {
                    IdHistorique = h.IdHistorique,
                    Date = h.Date,
                    Statut = h.Statut,
                    Action = h.Action,
                    Montant = h.Montant
                }).ToList()
            };
        }
        public async Task<List<Ordre>> GetOrdresByStatutAsync(int agentId, string statut)
        {
            var agent = await _unitOfWork.Agents.GetByIdAsync(agentId);
            if (agent == null)
                throw new InvalidOperationException("Agent introuvable.");

            await _roleStrategyContext.CanExecuteAsync(agent.Role.Name, null, agentId, "Statut");

            return await _unitOfWork.Ordres.GetOrdresByStatutAsync(statut);
        }
    }
}
