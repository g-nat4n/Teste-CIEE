using CadastroCurriculos.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CadastroCurriculos.Api.Data;

public class ContextoAplicacao : DbContext
{
    public ContextoAplicacao(DbContextOptions<ContextoAplicacao> options) : base(options)
    {
    }

    public DbSet<Candidato> Candidatos => Set<Candidato>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Candidato>(entidade =>
        {
            entidade.ToTable("Candidatos");
            entidade.HasKey(c => c.Id);
            entidade.Property(c => c.NomeCompleto).IsRequired().HasMaxLength(200);
            entidade.Property(c => c.Email).IsRequired().HasMaxLength(200);
            entidade.Property(c => c.Telefone).HasMaxLength(30);
            entidade.Property(c => c.AreaInteresse).HasMaxLength(150);
            entidade.Property(c => c.ResumoProfissional).HasMaxLength(2000);
            entidade.Property(c => c.FormacaoAcademica).HasMaxLength(2000);
            entidade.Property(c => c.Cursos).HasMaxLength(2000);
            entidade.Property(c => c.ExperienciasProfissionais).HasMaxLength(4000);
            entidade.Property(c => c.DataCadastro).IsRequired();
        });
    }
}
