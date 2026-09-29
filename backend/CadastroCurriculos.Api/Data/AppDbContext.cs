using CadastroCurriculos.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CadastroCurriculos.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Candidate> Candidates => Set<Candidate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Candidate>(entity =>
        {
            entity.ToTable("Candidates");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.NomeCompleto).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Email).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Telefone).HasMaxLength(30);
            entity.Property(c => c.AreaInteresse).HasMaxLength(150);
            entity.Property(c => c.ResumoProfissional).HasMaxLength(2000);
            entity.Property(c => c.DataCadastro).IsRequired();
        });
    }
}
