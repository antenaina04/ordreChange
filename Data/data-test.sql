CREATE TABLE [Role] (
    [Id] INT NOT NULL IDENTITY(1,1), 
    [Name] NVARCHAR(50) NOT NULL, 
    CONSTRAINT [PK_Role] PRIMARY KEY ([Id])
);

-- Données initiales pour la table Role
INSERT INTO [Role] ([Id], [Name]) 
VALUES 
    (1, N'Acheteur'),
    (2, N'Validateur');

CREATE TABLE [Agents] (
    [IdAgent] INT NOT NULL IDENTITY(1,1), 
    [Nom] NVARCHAR(100) NOT NULL, 
    [Username] NVARCHAR(50) NOT NULL, 
    [PasswordHash] NVARCHAR(MAX) NOT NULL, 
    [RoleId] INT NOT NULL, 
    CONSTRAINT [PK_Agents] PRIMARY KEY ([IdAgent]),
    CONSTRAINT [FK_Agents_Role_RoleId] FOREIGN KEY ([RoleId]) 
        REFERENCES [Role] ([Id]) ON DELETE NO ACTION
);

-- Index pour optimiser les recherches sur la clé étrangère
CREATE INDEX [IX_Agents_RoleId] ON [Agents] ([RoleId]);

CREATE TABLE [Ordres] (
    [IdOrdre] INT NOT NULL IDENTITY(1,1), 
    [Montant] REAL NOT NULL, 
    [Devise] NVARCHAR(3) NOT NULL, 
    [DeviseCible] NVARCHAR(3) NOT NULL, 
    [Statut] NVARCHAR(20) NOT NULL, 
    [TypeTransaction] NVARCHAR(20) NOT NULL, 
    [DateCreation] DATETIME2 NOT NULL, 
    [DateDerniereModification] DATETIME2 NULL, 
    [MontantConverti] REAL NOT NULL, 
    [IdAgent] INT NOT NULL, 
    CONSTRAINT [PK_Ordres] PRIMARY KEY ([IdOrdre]),
    CONSTRAINT [FK_Ordres_Agents_IdAgent] FOREIGN KEY ([IdAgent]) 
        REFERENCES [Agents] ([IdAgent]) ON DELETE CASCADE
);

-- Index pour optimiser les recherches sur la clé étrangère
CREATE INDEX [IX_Ordres_IdAgent] ON [Ordres] ([IdAgent]);

CREATE TABLE [HistoriqueOrdres] (
    [IdHistorique] INT NOT NULL IDENTITY(1,1), 
    [Date] DATETIME2 NOT NULL, 
    [Statut] NVARCHAR(20) NOT NULL, 
    [Action] NVARCHAR(20) NOT NULL, 
    [Montant] REAL NOT NULL, 
    [IdOrdre] INT NOT NULL, 
    CONSTRAINT [PK_HistoriqueOrdres] PRIMARY KEY ([IdHistorique]),
    CONSTRAINT [FK_HistoriqueOrdres_Ordres_IdOrdre] FOREIGN KEY ([IdOrdre]) 
        REFERENCES [Ordres] ([IdOrdre]) ON DELETE CASCADE
);

-- Index pour optimiser les recherches sur la clé étrangère
CREATE INDEX [IX_HistoriqueOrdres_IdOrdre] ON [HistoriqueOrdres] ([IdOrdre]);

-------------------------------------------------------------------------------------
-- Données de TEST 
INSERT INTO [Role] ([Id], [Name])
VALUES 
    (1, 'Acheteur'),
    (2, 'Validateur');

INSERT INTO [Agents] ([Nom], [Username], [PasswordHash], [RoleId])
VALUES
    ('Alexis Kotozafy', 'alexis', '4321', 1), -- Agent acheteur
    ('John Doe', 'john', '1234', 1), -- Agent acheteur
    ('Antenaina Randrianantoandro', 'antenaina', '1080', 2); -- Validateur

INSERT INTO [Ordres] ([Montant], [Devise], [DeviseCible], [Statut], [TypeTransaction], [DateCreation], [DateDerniereModification], [MontantConverti], [IdAgent])
VALUES
    (1000, 'EUR', 'USD', 'En attente', 'Achat', GETDATE(), NULL, 1095.50, 1), -- Créé par Jean Dupont
    (5000, 'USD', 'GBP', 'En cours', 'Vente', GETDATE(), GETDATE(), 3980.25, 2), -- Créé par Marie Curie
    (750, 'GBP', 'EUR', 'Terminé', 'Achat', DATEADD(DAY, -10, GETDATE()), GETDATE(), 870.40, 1); -- Créé par Jean Dupont

INSERT INTO [HistoriqueOrdres] ([Date], [Statut], [Action], [Montant], [IdOrdre])
VALUES
    (DATEADD(DAY, -2, GETDATE()), 'En attente', 'Création', 1000, 1), -- Ordre 1 : Initialisé
    (GETDATE(), 'En cours', 'Validation', 1000, 1), -- Ordre 1 : Validé

    (DATEADD(DAY, -7, GETDATE()), 'En attente', 'Création', 5000, 2), -- Ordre 2 : Initialisé
    (DATEADD(DAY, -5, GETDATE()), 'En cours', 'Conversion en cours', 5000, 2), -- Ordre 2 : En traitement

    (DATEADD(DAY, -12, GETDATE()), 'En attente', 'Création', 750, 3), -- Ordre 3 : Initialisé
    (DATEADD(DAY, -10, GETDATE()), 'Terminé', 'Finalisation', 750, 3); -- Ordre 3 : Finalisé
