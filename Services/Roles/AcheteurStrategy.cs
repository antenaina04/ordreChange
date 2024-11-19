﻿using ordreChange.Models;

namespace ordreChange.Services.Roles
{
    public class AcheteurStrategy : IRoleStrategy
    {
        public Task ValidateActionAsync(Ordre? ordre, int agentId, string action)
        {
            if (action == "Création")
            {
                // Les acheteurs peuvent toujours créer un ordre
                return Task.CompletedTask;
            }

            if (action == "Modification" || action == "Annulation")
            {
                if (ordre == null || ordre.IdAgent != agentId)
                    throw new InvalidOperationException("Seul le créateur de cet ordre est autorisé à le modifier.");
                if (ordre.Statut == "Validé")
                    throw new InvalidOperationException("Ordre déjà validé, modification ou annulation non autorisée.");
                return Task.CompletedTask;
            }

            if (action == "History")
            {
                if (ordre == null || ordre.IdAgent != agentId)
                    throw new InvalidOperationException("Seul le créateur de cet ordre est autorisé cette historique.");
                return Task.CompletedTask;
            }
            throw new InvalidOperationException($"Action '{action}' non autorisée pour le rôle Acheteur.");
        }
    }
}
