using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using energia_que_compensa.Models;

namespace energia_que_compensa.Data
{
    // Herda de IdentityDbContext para já incluir as tabelas de autenticação (AspNetUsers, AspNetRoles, etc.)
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Tabela de simulações realizadas (auditoria ODS 7 / relatórios de impacto)
        public DbSet<SimulationRecord> SimulationRecords { get; set; }

        // Tabela de leads / contatos que pediram orçamento de instalação solar
        public DbSet<Lead> Leads { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Garante que todas as tabelas vão para o schema public do PostgreSQL
            modelBuilder.HasDefaultSchema("public");

            modelBuilder.Entity<SimulationRecord>(entity =>
            {
                entity.HasKey(e => e.Id);

                // CreatedAt preenchido automaticamente pelo banco
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");

                // Campos de texto com tamanho máximo definido
                entity.Property(e => e.EfficiencyCategory).HasMaxLength(50);
                entity.Property(e => e.Cep).HasMaxLength(9);
                entity.Property(e => e.Uf).HasMaxLength(2);

                // FK opcional para AspNetUsers — se o usuário for deletado, UserId vira null (SetNull)
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.SetNull);

                // Índice para facilitar busca de simulações por usuário
                entity.HasIndex(e => e.UserId);

                // Índice para relatórios por data
                entity.HasIndex(e => e.CreatedAt);
            });

            modelBuilder.Entity<Lead>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");

                entity.Property(e => e.Name).HasMaxLength(150).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(254).IsRequired();
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Cep).HasMaxLength(9);
                entity.Property(e => e.Message).HasMaxLength(1000);

                // Email único — evita duplicatas de lead
                entity.HasIndex(e => e.Email).IsUnique();
            });
        }
    }
}
