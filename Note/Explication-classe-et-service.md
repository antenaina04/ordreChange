Classe Acheteur (Acheteur) :
    - Représente les agents qui peuvent créer des ordres d'achat ou de vente.
    - Méthodes principales : creerOrdre(typeTransaction, montant, devise) : ordre [ la seule méthode pour initier des ordres d'achat et de vente. Le paramètre typeTransaction détermine si l'ordre est un achat ou une vente] et modifierOrdre(ordre: Ordre) : void.
    - L’acheteur initie la transaction, mais ne peut pas la valider.

Classe Validateur (Validateur) :
    - Représente les agents avec le droit de valider les ordres une fois que ceux-ci ont été créés et éventuellement modifiés par un acheteur.
    - Méthode principale : validerOrdre(ordre: Ordre) : void, qui passe l’ordre en statut “Validé” une fois toutes les vérifications effectuées.

Classe Ordre : 
    - représente un ordre d'achat ou de vente de devise avec des attributs comme id, montant, devise, statut, etc.
    - methodes : calculerMontantConverti(taux: float) : float et changerStatut(nouveauStatut: String) : void
Classe TauxChangeService : 
    - Lors de la création ou de la validation, la méthode calculerMontantConverti(taux: float) de la classe Ordre interagit avec TauxChangeService pour obtenir le taux de change actuel et calculer le montant converti. Cette opération permet d’obtenir la valeur finale de la transaction d'achat en devise cible.

Classe HistoriqueOrdre : 
    - HistoriqueOrdre enregistre chaque changement d'état de l'ordre, incluant les actions de création, validation, ou annulation effectuées par l'agent.

Héritage ou Composition :
    - Si un agent peut endosser les deux rôles (Acheteur et Validateur), nous pourrions les regrouper via une classe parent Agent avec deux sous-classes Acheteur et Validateur.
    - Si les rôles sont bien distincts, une simple relation d’association peut suffire pour indiquer qu’un Ordre peut être manipulé par ces deux types d’agents.

RELATION:
    - Association entre Agent et Ordre pour la création et modification des ordres.
    - Composition entre Ordre et HistoriqueOrdre, car chaque ordre possède un historique propre.
    - Dépendance entre Ordre et TauxChangeService pour la conversion des montants.
--------------------------------------------------------------------------------------------------------------------------------------------------------------------------

DESIGN PATTERNS
    - Factory pour créer les ordres via une méthode creerOrdre() dans Agent.
    - Singleton pour TauxChangeService si un seul service de taux de change est nécessaire.