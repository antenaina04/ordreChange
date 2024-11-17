using Microsoft.EntityFrameworkCore;
using ordreChange.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ordreChange.Data
{
    public class OrdreDeChangeContext : DbContext
    {
        public OrdreDeChangeContext(DbContextOptions<OrdreDeChangeContext> options) : base(options) { }

        public DbSet<Agent> Agents { get; set; }
        public DbSet<Ordre> Ordres { get; set; }
        public DbSet<HistoriqueOrdre> HistoriqueOrdres { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configuration for Role
            modelBuilder.Entity<Role>()
                .HasData(
                    new Role { Id = 1, Name = "Acheteur" },
                    new Role { Id = 2, Name = "Validateur" }
                );

            // Configuration pour Agent
            modelBuilder.Entity<Agent>()
                .Property(a => a.Nom)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Agent>()
                .Property(a => a.Username)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Agent>()
                .Property(a => a.PasswordHash)
                .IsRequired();

            // Configuration for Agent
            modelBuilder.Entity<Agent>()
                .HasOne(a => a.Role)
                .WithMany()
                .HasForeignKey(a => a.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuration pour Ordre
            modelBuilder.Entity<Ordre>()
                .HasOne(o => o.Agent)
                .WithMany(a => a.Ordres)
                .HasForeignKey(o => o.IdAgent);

            modelBuilder.Entity<Ordre>()
                .Property(o => o.Devise)
                .IsRequired()
                .HasMaxLength(3);

            modelBuilder.Entity<Ordre>()
                .Property(o => o.Statut)
                .IsRequired()
                .HasMaxLength(20);

            modelBuilder.Entity<Ordre>()
                .Property(o => o.TypeTransaction)
                .IsRequired()
                .HasMaxLength(20);

            modelBuilder.Entity<Ordre>()
                .Property(o => o.DateCreation)
                .IsRequired();

            modelBuilder.Entity<Ordre>()
                .Property(o => o.DateDerniereModification)
                .IsRequired(false); // Configuration pour autoriser une valeur null

            // Configuration pour HistoriqueOrdre
            modelBuilder.Entity<HistoriqueOrdre>()
                .HasOne(h => h.Ordre)
                .WithMany(o => o.HistoriqueOrdres)
                .HasForeignKey(h => h.IdOrdre);

            modelBuilder.Entity<HistoriqueOrdre>()
                .Property(h => h.Statut)
                .IsRequired()
                .HasMaxLength(20);

            modelBuilder.Entity<HistoriqueOrdre>()
                .Property(h => h.Montant)
                .IsRequired();

            modelBuilder.Entity<HistoriqueOrdre>()
                .Property(h => h.Date)
                .IsRequired();
        }
    }
}
