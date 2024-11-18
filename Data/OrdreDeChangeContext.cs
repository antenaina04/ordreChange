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
            // TABLE : Role
            modelBuilder.Entity<Role>()
                .HasMany(r => r.Agents)
                .WithOne(a => a.Role)
                .HasForeignKey(a => a.RoleId)
                .OnDelete(DeleteBehavior.Restrict); // Empêche la suppression en cascade

            modelBuilder.Entity<Role>()
                .Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Role>()
                .HasData(
                    new Role { Id = 1, Name = "Acheteur" },
                    new Role { Id = 2, Name = "Validateur" }
                );

            // TABLE : Agent
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

            modelBuilder.Entity<Agent>()
                .HasOne(a => a.Role)
                .WithMany(r => r.Agents)
                .HasForeignKey(a => a.RoleId)
                .OnDelete(DeleteBehavior.Restrict); // ou .SetNull


            // TABLE : Ordre
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

            // TABLE : HistoriqueOrdre
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
