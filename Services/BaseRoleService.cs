using ordreChange.Models;
using ordreChange.Services.Roles;
using ordreChange.UnitOfWork;

namespace ordreChange.Services
{
    /// <summary>
    /// Classe de base pour les services liés aux rôles.
    /// Implémente le modèle Template Method pour valider et exécuter une action en fonction des rôles.
    /// </summary>
    public abstract class BaseRoleService : IBaseRoleService
    {
        /// <summary>
        /// Instance de l'unité de travail permettant d'accéder aux ressources de la base de données.
        /// </summary>
        protected readonly IUnitOfWork _iUnitOfWork;

        /// <summary>
        /// Contexte de stratégie pour la gestion des rôles et des autorisations.
        /// </summary>
        protected readonly RoleStrategyContext _roleStrategyContext;

        /// <summary>
        /// Constructeur de la classe de base pour les services liés aux rôles.
        /// </summary>
        /// <param name="unitOfWork">Interface pour l'unité de travail (accès aux données).</param>
        /// <param name="roleStrategyContext">Contexte de stratégie pour gérer les rôles et autorisations.</param>
        protected BaseRoleService(IUnitOfWork unitOfWork, RoleStrategyContext roleStrategyContext)
        {
            _iUnitOfWork = unitOfWork;
            _roleStrategyContext = roleStrategyContext;
        }

        /// <summary>
        /// Valide les permissions de l'agent pour une action donnée et exécute une logique personnalisée.
        /// Cette méthode suit le **Template Method Pattern** en permettant aux sous-classes de fournir une logique spécifique.
        /// </summary>
        /// <typeparam name="T">Type de retour de l'exécution personnalisée.</typeparam>
        /// <param name="agentId">Identifiant unique de l'agent.</param>
        /// <param name="action">Nom de l'action à valider pour l'agent.</param>
        /// <param name="execute">
        /// Fonction personnalisée qui contient la logique à exécuter après la validation.
        /// Cette fonction prend en paramètre un agent et retourne un type générique <typeparamref name="T"/>.
        /// </param>
        /// <returns>Un objet de type <typeparamref name="T"/> représentant le résultat de l'exécution.</returns>
        /// <exception cref="InvalidOperationException">Lance une exception si l'agent n'est pas trouvé.</exception>
        public async Task<T> ValidateAndExecuteAsync<T>(int agentId, string action, Func<Agent, Task<T>> execute)
        {
            var agent = await _iUnitOfWork.Agents.GetByIdAsync(agentId);
            if (agent == null)
                throw new InvalidOperationException("Agent introuvable.");

            await _roleStrategyContext.CanExecuteAsync(agent.Role.Name, null, agentId, action);

            // Custom logic provided by sub-classes
            return await execute(agent);
        }
    }
}
