﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ordreChange.Data;

#nullable disable

namespace ordreChange.Migrations
{
    [DbContext(typeof(OrdreDeChangeContext))]
    [Migration("20241118061938_InitialMigration")]
    partial class InitialMigration
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("ordreChange.Models.Agent", b =>
                {
                    b.Property<int>("IdAgent")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdAgent"));

                    b.Property<string>("Nom")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("RoleId")
                        .HasColumnType("int");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("IdAgent");

                    b.HasIndex("RoleId");

                    b.ToTable("Agents");
                });

            modelBuilder.Entity("ordreChange.Models.HistoriqueOrdre", b =>
                {
                    b.Property<int>("IdHistorique")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdHistorique"));

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<int>("IdOrdre")
                        .HasColumnType("int");

                    b.Property<float>("Montant")
                        .HasColumnType("real");

                    b.Property<string>("Statut")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.HasKey("IdHistorique");

                    b.HasIndex("IdOrdre");

                    b.ToTable("HistoriqueOrdres");
                });

            modelBuilder.Entity("ordreChange.Models.Ordre", b =>
                {
                    b.Property<int>("IdOrdre")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdOrdre"));

                    b.Property<DateTime>("DateCreation")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateDerniereModification")
                        .HasColumnType("datetime2");

                    b.Property<string>("Devise")
                        .IsRequired()
                        .HasMaxLength(3)
                        .HasColumnType("nvarchar(3)");

                    b.Property<string>("DeviseCible")
                        .IsRequired()
                        .HasMaxLength(3)
                        .HasColumnType("nvarchar(3)");

                    b.Property<int>("IdAgent")
                        .HasColumnType("int");

                    b.Property<float>("Montant")
                        .HasColumnType("real");

                    b.Property<float>("MontantConverti")
                        .HasColumnType("real");

                    b.Property<string>("Statut")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("TypeTransaction")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.HasKey("IdOrdre");

                    b.HasIndex("IdAgent");

                    b.ToTable("Ordres");
                });

            modelBuilder.Entity("ordreChange.Models.Role", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.ToTable("Role");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Name = "Acheteur"
                        },
                        new
                        {
                            Id = 2,
                            Name = "Validateur"
                        });
                });

            modelBuilder.Entity("ordreChange.Models.Agent", b =>
                {
                    b.HasOne("ordreChange.Models.Role", "Role")
                        .WithMany("Agents")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Role");
                });

            modelBuilder.Entity("ordreChange.Models.HistoriqueOrdre", b =>
                {
                    b.HasOne("ordreChange.Models.Ordre", "Ordre")
                        .WithMany("HistoriqueOrdres")
                        .HasForeignKey("IdOrdre")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Ordre");
                });

            modelBuilder.Entity("ordreChange.Models.Ordre", b =>
                {
                    b.HasOne("ordreChange.Models.Agent", "Agent")
                        .WithMany("Ordres")
                        .HasForeignKey("IdAgent")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Agent");
                });

            modelBuilder.Entity("ordreChange.Models.Agent", b =>
                {
                    b.Navigation("Ordres");
                });

            modelBuilder.Entity("ordreChange.Models.Ordre", b =>
                {
                    b.Navigation("HistoriqueOrdres");
                });

            modelBuilder.Entity("ordreChange.Models.Role", b =>
                {
                    b.Navigation("Agents");
                });
#pragma warning restore 612, 618
        }
    }
}
